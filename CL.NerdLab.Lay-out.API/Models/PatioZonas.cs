using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class PatioZonas
{
    public int IdZona { get; set; }

    public int IdPatio { get; set; }

    public string NombreZona { get; set; } = null!;

    public string? ColorHex { get; set; }

    public int Filas { get; set; }

    public int Columnas { get; set; }

    public string Orientacion { get; set; } = null!;

    public virtual Patios IdPatioNavigation { get; set; } = null!;

    public virtual ICollection<PatioSlots> PatioSlots { get; set; } = new List<PatioSlots>();
}
