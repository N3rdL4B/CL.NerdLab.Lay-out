using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class TipoVehiculo
{
    public int IdTipoVehiculo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public int? LargoPx { get; set; }

    public virtual ICollection<Flota> Flota { get; set; } = new List<Flota>();
}
