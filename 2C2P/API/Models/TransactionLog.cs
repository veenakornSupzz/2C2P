using System;
using System.Collections.Generic;

namespace API.Models;

public partial class TransactionLog
{
    public int LogId { get; set; }

    public string? TransactionId { get; set; }

    public string FileName { get; set; } = null!;

    public string LogMessage { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
