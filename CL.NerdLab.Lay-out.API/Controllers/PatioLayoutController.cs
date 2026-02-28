using CL.NerdLab.Lay_out.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatioLayoutController : ControllerBase
    {
        private readonly Lay_out_DBContext _context;
        public PatioLayoutController(Lay_out_DBContext context) => _context = context;

        [HttpGet("{idPatio}")]
        public async Task<IActionResult> GetLayout(int idPatio)
        {
            var zonas = await _context.PatioZonas.Where(z => z.IdPatio == idPatio)
                .Select(z => new {
                    z.IdZona,
                    z.NombreZona,
                    z.ColorHex,
                    z.Filas,
                    z.Columnas,
                    z.Orientacion, // <--- NUEVO
                    Slots = _context.PatioSlots.Where(s => s.IdZona == z.IdZona).Select(s => new {
                        s.IdSlot,
                        s.Fila,
                        s.Columna,
                        s.IdPatente,
                        Patente = s.IdPatente != null ? s.IdPatenteNavigation.Patente : null,
                        IdTipoVehiculo = s.IdPatente != null ? s.IdPatenteNavigation.IdTipoVehiculo : null,
                        TipoBus = s.IdPatente != null ? s.IdPatenteNavigation.IdTipoVehiculoNavigation.Nombre : null,
                        LargoPx = s.IdPatente != null ? s.IdPatenteNavigation.IdTipoVehiculoNavigation.LargoPx : null
                    }).ToList()
                }).ToListAsync();
            return Ok(zonas);
        }

        [HttpPost("GuardarZona")]
        public async Task<IActionResult> GuardarZona([FromBody] DtoGuardarZona request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                PatioZonas zona;
                if (request.IdZona == 0)
                {
                    zona = new PatioZonas { IdPatio = request.IdPatio, NombreZona = request.NombreZona, ColorHex = request.ColorHex, Filas = request.Filas, Columnas = request.Columnas, 
                        Orientacion = request.Orientacion 
                    };

                    _context.PatioZonas.Add(zona);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    zona = await _context.PatioZonas.FindAsync(request.IdZona);
                    if (zona == null) return NotFound();
                    zona.NombreZona = request.NombreZona; zona.ColorHex = request.ColorHex; zona.Filas = request.Filas; zona.Columnas = request.Columnas; 
                    zona.Orientacion = request.Orientacion;
                }

                var slotsExistentes = await _context.PatioSlots.Where(s => s.IdZona == zona.IdZona).ToListAsync();
                for (int f = 1; f <= zona.Filas; f++)
                    for (int c = 1; c <= zona.Columnas; c++)
                        if (!slotsExistentes.Any(s => s.Fila == f && s.Columna == c))
                            _context.PatioSlots.Add(new PatioSlots { IdZona = zona.IdZona, Fila = f, Columna = c });

                var slotsFuera = slotsExistentes.Where(s => s.Fila > zona.Filas || s.Columna > zona.Columnas).ToList();
                _context.PatioSlots.RemoveRange(slotsFuera);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(ex.Message); }
        }

        [HttpPost("RotarZona/{idZona}")]
        public async Task<IActionResult> RotarZona(int idZona)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var zona = await _context.PatioZonas.FindAsync(idZona);
                if (zona == null) return NotFound();

                // Invertir dimensiones y orientación
                int viejasFilas = zona.Filas;
                zona.Filas = zona.Columnas;
                zona.Columnas = viejasFilas;
                zona.Orientacion = zona.Orientacion == "H" ? "V" : "H";

                // Transponer las posiciones de las micros
                var slots = await _context.PatioSlots.Where(s => s.IdZona == idZona).ToListAsync();
                foreach (var slot in slots)
                {
                    int viejaFila = slot.Fila;
                    slot.Fila = slot.Columna;
                    slot.Columna = viejaFila;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(ex.Message); }
        }

        [HttpPost("AsignarBus")]
        public async Task<IActionResult> AsignarBus([FromBody] DtoAsignarBus request)
        {
            var slot = await _context.PatioSlots.FindAsync(request.IdSlot);
            if (slot == null) return NotFound("Slot no encontrado");

            var bus = await _context.Flota.FirstOrDefaultAsync(f => f.Patente == request.Patente);
            if (bus == null) return NotFound("Bus no encontrado");

            var slotAnterior = await _context.PatioSlots.FirstOrDefaultAsync(s => s.IdPatente == bus.IdPatente);
            if (slotAnterior != null) slotAnterior.IdPatente = null;

            slot.IdPatente = bus.IdPatente;
            bus.IdTipoVehiculo = request.IdTipoVehiculo;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("QuitarBus")]
        public async Task<IActionResult> QuitarBus([FromBody] int idSlot)
        {
            var slot = await _context.PatioSlots.FindAsync(idSlot);
            if (slot != null) { slot.IdPatente = null; await _context.SaveChangesAsync(); }
            return Ok(new { success = true });
        }

        // Agrega este endpoint en tu PatioLayoutController
        [HttpPost("InvertirZona")]
        public async Task<IActionResult> InvertirZona([FromBody] DtoInvertirZona request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var zona = await _context.PatioZonas.FindAsync(request.IdZona);
                if (zona == null) return NotFound();

                var slots = await _context.PatioSlots.Where(s => s.IdZona == request.IdZona).ToListAsync();

                foreach (var slot in slots)
                {
                    if (request.Eje == "V") // Espejo Vertical (Invierte las Filas)
                    {
                        slot.Fila = (zona.Filas - slot.Fila) + 1;
                    }
                    else if (request.Eje == "H") // Espejo Horizontal (Invierte las Columnas)
                    {
                        slot.Columna = (zona.Columnas - slot.Columna) + 1;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return BadRequest(ex.Message); }
        }

        // Agrega esta clase al final del archivo junto a los otros DTOs
        public class DtoInvertirZona { public int IdZona { get; set; } public string Eje { get; set; } }
    }
    public class DtoGuardarZona { public int IdZona { get; set; } public int IdPatio { get; set; } public string NombreZona { get; set; } public string ColorHex { get; set; } public int Filas { get; set; } public int Columnas { get; set; } public string Orientacion { get; set; } }
    public class DtoAsignarBus { public int IdSlot { get; set; } public string Patente { get; set; } public int IdTipoVehiculo { get; set; } }
}