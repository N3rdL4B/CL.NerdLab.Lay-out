using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class Flota
{
    public int IdPatente { get; set; }

    public string Patente { get; set; } = null!;

    public bool? Activo { get; set; }

    public int IdPatio { get; set; }

    public DateTime? FechaReg { get; set; }

    public virtual Patios IdPatioNavigation { get; set; } = null!;

    public virtual ICollection<RegistroActividadBuses> RegistroActividadBuses { get; set; } = new List<RegistroActividadBuses>();
}
