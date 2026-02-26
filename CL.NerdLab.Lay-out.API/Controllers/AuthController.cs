using CL.NerdLab.Lay_out.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Lay_out_DBContext _context;

        public AuthController(Lay_out_DBContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Buscamos por RUT en vez de Email
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Rut == request.Rut && u.Activo == true);

            if (usuario == null)
                return BadRequest(new { success = false, message = "RUT no encontrado o cuenta inactiva." });

            if (!string.IsNullOrEmpty(usuario.PasswordTemp) && usuario.PasswordTemp == request.Password)
                return Ok(new { success = true, requirePasswordChange = true });

            if (usuario.PasswordHash == request.Password)
            {
                return Ok(new
                {
                    success = true,
                    requirePasswordChange = false,
                    role = usuario.IdRolNavigation.Nombre,
                    nombre = usuario.NombreCompleto,
                    rut = usuario.Rut
                });
            }

            return BadRequest(new { success = false, message = "Contraseña incorrecta." });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Rut == request.Rut && u.PasswordTemp == request.CurrentPassword);

            if (usuario == null)
                return BadRequest(new { success = false, message = "Datos inválidos o la clave temporal ya expiró." });

            usuario.PasswordHash = request.NewPassword;
            usuario.PasswordTemp = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, role = usuario.IdRolNavigation.Nombre, nombre = usuario.NombreCompleto });
        }
    }

    public class LoginRequestDto { public string Rut { get; set; } public string Password { get; set; } }
    public class ChangePasswordDto { public string Rut { get; set; } public string CurrentPassword { get; set; } public string NewPassword { get; set; } }
}