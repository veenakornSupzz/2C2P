using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public string TransactionId { get; set; } = null!;

    public string? AccountNumber { get; set; }

    public decimal? Amount { get; set; }

    public string? CurrencyCode { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Status { get; set; }

    public bool IsValid { get; set; }

    public DateTime? CreatedAt { get; set; }
}
