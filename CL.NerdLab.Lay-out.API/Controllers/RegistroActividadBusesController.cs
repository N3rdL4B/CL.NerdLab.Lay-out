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

        // =====================================================================
        // TABLA PRINCIPAL: MUESTRA SOLO LOS REGISTROS DEL DÍA DE HOY
        // =====================================================================
        [HttpGet("EstadoFlota")]
        public async Task<IActionResult> GetEstadoFlota()
        {
            try
            {
                // TRUCO DE COLEGA: Ajustamos a la hora de Chile (UTC-3)
                // Así nos aseguramos de que el rango sea de 00:00 a 23:59 hora local chilena
                DateTime horaChile = DateTime.UtcNow.AddHours(-3);
                DateTime inicioHoyUtc = horaChile.Date.AddHours(3); // Las 00:00 de Chile, convertidas a UTC para la BD

                // Traemos los buses activos y sus últimas actividades cruzadas
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

                // FILTRO ESTRICTO: Solo mandamos al Frontend los buses que tengan actividad a partir de las 00:00 de hoy
                var resultado = flotaBruta
                    .Where(x => x.UltimaActividad != null && x.UltimaActividad.FechaReg >= inicioHoyUtc)
                    .Select(x => new
                    {
                        patente = x.Patente,
                        estado = x.UltimaActividad!.EstadoActividadBus,
                        porcentaje = x.UltimaActividad.PorcentajeCarga,
                        activo = true
                    });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno al traer la flota", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("CambiarEstado")]
        public async Task<IActionResult> CambiarEstado([FromBody] CambioEstadoDto request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rut == request.RutUsuario);
                if (usuario == null) return Ok(new { success = false, message = "Tu usuario no está autorizado para esto." });

                if (string.IsNullOrWhiteSpace(request.Patente)) return Ok(new { success = false, message = "Patente no válida." });
                request.Patente = request.Patente.ToUpper();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. AUTO-INSERCIÓN: Si la patente no existe, se crea
                    var bus = await _context.Flota.FirstOrDefaultAsync(f => f.Patente == request.Patente);
                    if (bus == null)
                    {
                        var patio = await _context.Patios.FirstOrDefaultAsync();
                        if (patio == null)
                        {
                            patio = new Patios { Nombre = "Terminal Central" };
                            _context.Patios.Add(patio);
                            await _context.SaveChangesAsync();
                        }

                        bus = new Flota { Patente = request.Patente, IdPatio = patio.IdPatio, Activo = true };
                        _context.Flota.Add(bus);
                        await _context.SaveChangesAsync();
                    }

                    // 2. REGISTRO DE ACTIVIDAD PRINCIPAL (Foto del momento)
                    var actividad = await _context.RegistroActividadBuses.FirstOrDefaultAsync(r => r.IdPatente == bus.IdPatente);
                    if (actividad == null)
                    {
                        actividad = new RegistroActividadBuses { IdPatente = bus.IdPatente };
                        _context.RegistroActividadBuses.Add(actividad);
                    }

                    actividad.EstadoActividadBus = request.Estado;
                    actividad.PorcentajeCarga = request.Porcentaje;
                    actividad.IdUsuario = usuario.IdUsuario;
                    actividad.FechaReg = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // 3. LOG INMUTABLE DE LA ACCIÓN (Ledger-safe)
                    string accionLog = $"Cambió patente {bus.Patente} a estado {request.Estado}";
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO LogsActividadUsuarios (IdUsuario, Accion, FechaReg) 
                        VALUES ({usuario.IdUsuario}, {accionLog}, {DateTime.UtcNow})");

                    // 4. HISTORIAL DE CARGA INMUTABLE SI APLICA (Ledger-safe)
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
                    return Ok(new { success = false, message = "Error de Base de Datos al guardar", details = ex.InnerException?.Message ?? ex.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Falla crítica en el servidor", details = ex.Message });
            }
        }

        // =====================================================================
        // MÉTODOS DE HISTORIAL GLOBAL
        // =====================================================================

        [HttpGet("HistorialCarga/Hoy")]
        public async Task<IActionResult> GetHistorialCargaHoy()
        {
            try
            {
                // Mismo truco horario para el Historial de Hoy
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
                        Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                    }).ToListAsync();

                return Ok(historial);
            }
            catch (Exception)
            {
                return Ok(new List<object>());
            }
        }

        [HttpGet("HistorialCarga/Reciente")]
        public async Task<IActionResult> GetHistorialCargaReciente()
        {
            try
            {
                // ÚLTIMOS 2 DÍAS 
                DateTime horaChile = DateTime.UtcNow.AddHours(-3);
                DateTime haceDosDiasUtc = horaChile.Date.AddDays(-2).AddHours(3);

                var historial = await _context.HistorialRegistroCarga
                    .Include(h => h.IdRegistroActividadNavigation).ThenInclude(r => r.IdPatenteNavigation)
                    .Include(h => h.IdUsuarioModifNavigation)
                    .Where(h => h.FechaModifRegistro >= haceDosDiasUtc)
                    .OrderByDescending(h => h.FechaModifRegistro)
                    .Select(h => new {
                        Patente = h.IdRegistroActividadNavigation.IdPatenteNavigation.Patente,
                        Fecha = h.FechaModifRegistro,
                        Porcentaje = h.PorcentajeCarga,
                        Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                    }).ToListAsync();

                return Ok(historial);
            }
            catch (Exception)
            {
                return Ok(new List<object>());
            }
        }

        [HttpGet("HistorialCarga/Rango")]
        public async Task<IActionResult> GetHistorialCargaRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        {
            try
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
                        Usuario = h.IdUsuarioModifNavigation.NombreCompleto
                    }).ToListAsync();

                return Ok(historial);
            }
            catch (Exception)
            {
                return Ok(new List<object>());
            }
        }
    }


    public class CambioEstadoDto { public string Patente { get; set; } public string Estado { get; set; } public int? Porcentaje { get; set; } public string RutUsuario { get; set; } }
}