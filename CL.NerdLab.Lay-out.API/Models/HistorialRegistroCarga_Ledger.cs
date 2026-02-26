using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class HistorialRegistroCarga_Ledger
{
    public int IdRegistroCarga { get; set; }

    public int IdRegistroActividad { get; set; }

    public int PorcentajeCarga { get; set; }

    public int IdUsuarioModif { get; set; }

    public DateTime? FechaModifRegistro { get; set; }

    public long ledger_transaction_id { get; set; }

    public long ledger_sequence_number { get; set; }

    public int ledger_operation_type { get; set; }

    public string ledger_operation_type_desc { get; set; } = null!;
}
