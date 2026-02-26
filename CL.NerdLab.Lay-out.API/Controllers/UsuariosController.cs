using CL.NerdLab.Lay_out.API.Controllers.Base;
using CL.NerdLab.Lay_out.API.Models;
using CL.NerdLab.Lay_out.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : BaseController<Usuarios>
    {
        private readonly Lay_out_DBContext _context;

        public UsuariosController(IGenericRepository<Usuarios> repository, Lay_out_DBContext context) : base(repository)
        {
            _context = context;
        }

        [HttpGet("Detalle")]
        public async Task<IActionResult> GetUsuariosDetalle()
        {
            var users = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Select(u => new {
                    u.IdUsuario,
                    u.NombreCompleto,
                    u.Email,
                    u.Rut,
                    u.Activo,
                    Rol = u.IdRolNavigation.Nombre,
                    u.IdRol
                }).ToListAsync();
            return Ok(users);
        }

        [HttpPost("Editar/{id}")]
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] EditarUsuarioDto dto)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();

            user.NombreCompleto = dto.NombreCompleto;
            user.Rut = dto.Rut;
            user.Email = dto.Email;
            user.IdRol = dto.IdRol;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("ToggleActivo/{id}")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();
            user.Activo = !user.Activo;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPut("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();
            user.PasswordTemp = "1234";
            user.PasswordHash = "-";
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }

    public class EditarUsuarioDto { public string NombreCompleto { get; set; } public string Rut { get; set; } public string Email { get; set; } public int IdRol { get; set; } }
}