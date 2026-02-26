using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class HistorialRegistroCarga
{
    public int IdRegistroCarga { get; set; }

    public int IdRegistroActividad { get; set; }

    public int PorcentajeCarga { get; set; }

    public int IdUsuarioModif { get; set; }

    public DateTime? FechaModifRegistro { get; set; }

    public long ledger_start_transaction_id { get; set; }

    public long ledger_start_sequence_number { get; set; }

    public virtual RegistroActividadBuses IdRegistroActividadNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioModifNavigation { get; set; } = null!;
}
