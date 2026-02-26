using System;
using System.Collections.Generic;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class LogsActividadUsuarios
{
    public int IdLog { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime? FechaReg { get; set; }

    public long ledger_start_transaction_id { get; set; }

    public long ledger_start_sequence_number { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
