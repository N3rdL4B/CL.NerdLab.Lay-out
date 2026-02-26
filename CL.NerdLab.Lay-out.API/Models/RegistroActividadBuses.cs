using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class RegistroActividadBuses
{
    public int IdRegistroActividad { get; set; }

    public int IdPatente { get; set; }

    public string EstadoActividadBus { get; set; } = null!;

    public int? PorcentajeCarga { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaReg { get; set; }

    public virtual ICollection<HistorialRegistroCarga> HistorialRegistroCarga { get; set; } = new List<HistorialRegistroCarga>();

    public virtual Flota IdPatenteNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
