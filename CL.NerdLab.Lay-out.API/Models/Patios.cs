using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class Patios
{
    public int IdPatio { get; set; }

    public string Nombre { get; set; } = null!;

    public DateTime? FechaReg { get; set; }

    public virtual ICollection<Flota> Flota { get; set; } = new List<Flota>();
}
