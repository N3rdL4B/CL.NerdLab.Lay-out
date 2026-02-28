using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class PatioSlots
{
    public int IdSlot { get; set; }

    public int IdZona { get; set; }

    public int Fila { get; set; }

    public int Columna { get; set; }

    public int? IdPatente { get; set; }

    public virtual Flota? IdPatenteNavigation { get; set; }

    public virtual PatioZonas IdZonaNavigation { get; set; } = null!;
}
