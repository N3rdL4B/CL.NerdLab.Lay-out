using CL.NerdLab.Lay_out.API.Controllers.Base;
using CL.NerdLab.Lay_out.API.Models;
using CL.NerdLab.Lay_out.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroActividadBusesController : BaseController<RegistroActividadBuses>
    {
        private readonly Lay_out_DBContext _context;

        public RegistroActividadBusesController(IGenericRepository<RegistroActividadBuses> repository, Lay_out_DBContext context) : base(repository)
        {
            _context = context;
        }

        [HttpGet("EstadoFlota")]
        public async Task<IActionResult> GetEstadoFlota()
        {
            try
            {
                // Ajustamos a la hora de Chile (UTC-3)
                DateTime horaChile = DateTime.UtcNow.AddHours(-3);
                DateTime inicioHoyUtc = horaChile.Date.AddHours(3); // Las 00:00 de Chile a UTC

                var flotaBruta = await _context.Flota
                    .Where(f => f.Activo == true)
                    .Select(f => new
                    {
                        Patente = f.Patente,
                        UltimaActividad = _context.RegistroActividadBuses
                            .Where(r => r.IdPatente == f.IdPatente)
                            .OrderByDescending(r => r.FechaReg)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                // FILTRO ESTRICTO: Solo buses con actividad HOY (00:00 a 23:59)
                var resultado = flotaBruta
                    .Where(x => x.UltimaActividad != null && x.UltimaActividad.FechaReg >= inicioHoyUtc)
                    .Select(x => new
                    {
                        patente = x.Patente,
                        estado = x.UltimaActividad!.EstadoActividadBus,
                        porcentaje = x.UltimaActividad.PorcentajeCarga,
                        recorrido = x.UltimaActividad.NumeroRecorrido,
                        tipoBus = _context.Flota.Include(f => f.IdTipoVehiculoNavigation)
                                    .Where(f => f.IdPatente == x.UltimaActividad.IdPatente)
                                    .Select(f => f.IdTipoVehiculoNavigation.Nombre).FirstOrDefault() ?? "Estándar",
                        largoPx = _context.Flota.Include(f => f.IdTipoVehiculoNavigation)
                                    .Where(f => f.IdPatente == x.UltimaActividad.IdPatente)
                                    .Select(f => f.IdTipoVehiculoNavigation.LargoPx).FirstOrDefault() ?? 90,
                        activo = true
                    });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", details = ex.Message });
            }
        }

        [HttpPost("CambiarEstado")]
        public async Task<IActionResult> CambiarEstado([FromBody] CambioEstadoDto request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rut == request.RutUsuario);
                if (usuario == null) return Ok(new { success = false, message = "Usuario no autorizado." });

                request.Patente = request.Patente?.ToUpper().Trim() ?? "";

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var bus = await _context.Flota.FirstOrDefaultAsync(f => f.Patente == request.Patente);
                    if (bus == null)
                    {
                        var patio = await _context.Patios.FirstOrDefaultAsync() ?? new Patios { Nombre = "Terminal Central" };
                        if (patio.IdPatio == 0) { _context.Patios.Add(patio); await _context.SaveChangesAsync(); }
                        bus = new Flota { Patente = request.Patente, IdPatio = patio.IdPatio, Activo = true };
                        _context.Flota.Add(bus);
                        await _context.SaveChangesAsync();
                    }

                    var actividad = await _context.RegistroActividadBuses.FirstOrDefaultAsync(r => r.IdPatente == bus.IdPatente);
                    if (actividad == null)
                    {
                        actividad = new RegistroActividadBuses { IdPatente = bus.IdPatente };
                        _context.RegistroActividadBuses.Add(actividad);
                    }

                    actividad.EstadoActividadBus = request.Estado;
                    actividad.PorcentajeCarga = request.Porcentaje;
                    if (request.NumeroRecorrido != null) { actividad.NumeroRecorrido = request.NumeroRecorrido; }
                    actividad.IdUsuario = usuario.IdUsuario;
                    actividad.FechaReg = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    string accionLog = $"Cambió patente {bus.Patente} a estado {request.Estado}";
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO LogsActividadUsuarios (IdUsuario, Accion, FechaReg) 
                        VALUES ({usuario.IdUsuario}, {accionLog}, {DateTime.UtcNow})");

                    if (request.Estado == "Carga" && request.Porcentaje.HasValue)
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync($@"
                            INSERT INTO HistorialRegistroCarga (IdRegistroActividad, PorcentajeCarga, IdUsuarioModif, FechaModifRegistro) 
                            VALUES ({actividad.IdRegistroActividad}, {request.Porcentaje.Value}, {usuario.IdUsuario}, {DateTime.UtcNow})");
                    }

                    await transaction.CommitAsync();
                    return Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { success = false, message = "Error BD", details = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error servidor", details = ex.Message });
            }
        }

        [HttpPost("AsignarRecorrido")]
        public async Task<IActionResult> AsignarRecorrido([FromBody] AsignarRecorridoDto request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rut == request.RutUsuario);
                if (usuario == null) return Ok(new { success = false, message = "Usuario no autorizado." });

                request.Patente = request.Patente?.ToUpper().Trim() ?? "";

                var bus = await _context.Flota.FirstOrDefaultAsync(f => f.Patente == request.Patente);
                if (bus == null) return Ok(new { success = false, message = "El bus no existe." });

                var actividad = await _context.RegistroActividadBuses.FirstOrDefaultAsync(r => r.IdPatente == bus.IdPatente);
                if (actividad == null)
                {
                    actividad = new RegistroActividadBuses { IdPatente = bus.IdPatente, EstadoActividadBus = "Disponible" };
                    _context.RegistroActividadBuses.Add(actividad);
                }

                actividad.NumeroRecorrido = request.NumeroRecorrido;
                actividad.IdUsuario = usuario.IdUsuario;
                actividad.FechaReg = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                string accionLog = $"Asignó recorrido '{request.NumeroRecorrido}' a patente {bus.Patente}";
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO LogsActividadUsuarios (IdUsuario, Accion, FechaReg) 
                    VALUES ({usuario.IdUsuario}, {accionLog}, {DateTime.UtcNow})");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", details = ex.Message });
            }
        }

        [HttpGet("Patentes")]
        public async Task<IActionResult> GetTodasLasPatentes()
        {
            var patentes = await _context.Flota.Where(f => f.Activo == true).Select(f => f.Patente).ToListAsync();
            return Ok(patentes);
        }

        [HttpGet("HistorialCarga/Bus/{patente}")]
        public async Task<IActionResult> GetHistorialBus(string patente)
        {
            var historial = await _context.HistorialRegistroCarga
                .Include(h => h.IdRegistroActividadNavigation).ThenInclude(r => r.IdPatenteNavigation)
                .Include(h => h.IdUsuarioModifNavigation)
                .Where(h => h.IdRegistroActividadNavigation.IdPatenteNavigation.Patente == patente)
                .OrderByDescending(h => h.FechaModifRegistro)
                .Select(h => new { Fecha = h.FechaModifRegistro, Porcentaje = h.PorcentajeCarga, Usuario = h.IdUsuarioModifNavigation.NombreCompleto })
                .ToListAsync();
            return Ok(historial);
        }

        // --- HISTORIALES GLOBALES (Ahora incluyen el Estado del vehículo) ---
        [HttpGet("HistorialCarga/Hoy")]
        public async Task<IActionResult> GetHistorialCargaHoy()
        {
            DateTime horaChile = DateTime.UtcNow.AddHours(-3);
            DateTime inicioHoyUtc = horaChile.Date.AddHours(3);
            DateTime finHoyUtc = inicioHoyUtc.AddDays(1).AddTicks(-1);

            var historial = await _context.HistorialRegistroCarga
                .Include(h => h.IdRegistroActividadNavigation).ThenInclude(r => r.IdPatenteNavigation)
                .Include(h => h.IdUsuarioModifNavigation)
                .Where(h => h.FechaModifRegistro >= inicioHoyUtc && h.FechaModifRegistro <= finHoyUtc)
                .OrderByDescending(h => h.FechaModifRegistro)
                .Select(h => new {
                    Patente = h.IdRegistroActividadNavigation.IdPatenteNavigation.Patente,
                    Fecha = h.FechaModifRegistro,
                    Porcentaje = h.PorcentajeCarga,
                    Estado = h.IdRegistroActividadNavigation.EstadoActividadBus, // ¡NUEVO!
                    Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                })
                .ToListAsync();
            return Ok(historial);
        }

        [HttpGet("HistorialCarga/Reciente")]
        public async Task<IActionResult> GetHistorialCargaReciente()
        {
            DateTime haceDosDiasUtc = DateTime.UtcNow.AddHours(-3).Date.AddDays(-2).AddHours(3);
            var historial = await _context.HistorialRegistroCarga
                .Include(h => h.IdRegistroActividadNavigation).ThenInclude(r => r.IdPatenteNavigation)
                .Include(h => h.IdUsuarioModifNavigation)
                .Where(h => h.FechaModifRegistro >= haceDosDiasUtc)
                .OrderByDescending(h => h.FechaModifRegistro)
                .Select(h => new {
                    Patente = h.IdRegistroActividadNavigation.IdPatenteNavigation.Patente,
                    Fecha = h.FechaModifRegistro,
                    Porcentaje = h.PorcentajeCarga,
                    Estado = h.IdRegistroActividadNavigation.EstadoActividadBus, // ¡NUEVO!
                    Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                })
                .ToListAsync();
            return Ok(historial);
        }

        [HttpGet("HistorialCarga/Rango")]
        public async Task<IActionResult> GetHistorialCargaRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        {
            var fechaFin = fin.Date.AddDays(1).AddTicks(-1);
            var historial = await _context.HistorialRegistroCarga
                .Include(h => h.IdRegistroActividadNavigation).ThenInclude(r => r.IdPatenteNavigation)
                .Include(h => h.IdUsuarioModifNavigation)
                .Where(h => h.FechaModifRegistro >= inicio.Date && h.FechaModifRegistro <= fechaFin)
                .OrderByDescending(h => h.FechaModifRegistro)
                .Select(h => new {
                    Patente = h.IdRegistroActividadNavigation.IdPatenteNavigation.Patente,
                    Fecha = h.FechaModifRegistro,
                    Porcentaje = h.PorcentajeCarga,
                    Estado = h.IdRegistroActividadNavigation.EstadoActividadBus, // ¡NUEVO!
                    Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                })
                .ToListAsync();
            return Ok(historial);
        }
    }
    public class CambioEstadoDto { public string Patente { get; set; } public string Estado { get; set; } public int? Porcentaje { get; set; } public string? NumeroRecorrido { get; set; } public string RutUsuario { get; set; } }
    public class AsignarRecorridoDto { public string Patente { get; set; } public string NumeroRecorrido { get; set; } public string RutUsuario { get; set; } }
}