using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class Usuarios
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Rut { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordTemp { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaReg { get; set; }

    public virtual ICollection<HistorialRegistroCarga> HistorialRegistroCarga { get; set; } = new List<HistorialRegistroCarga>();

    public virtual Roles IdRolNavigation { get; set; } = null!;

    public virtual ICollection<LogsActividadUsuarios> LogsActividadUsuarios { get; set; } = new List<LogsActividadUsuarios>();

    public virtual ICollection<RegistroActividadBuses> RegistroActividadBuses { get; set; } = new List<RegistroActividadBuses>();
}
