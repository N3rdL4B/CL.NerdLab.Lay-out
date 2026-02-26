using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class LogsActividadUsuarios_Ledger
{
    public int IdLog { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime? FechaReg { get; set; }

    public long ledger_transaction_id { get; set; }

    public long ledger_sequence_number { get; set; }

    public int ledger_operation_type { get; set; }

    public string ledger_operation_type_desc { get; set; } = null!;
}
