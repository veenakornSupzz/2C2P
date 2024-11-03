using System;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Transaction ID is required.")]
        [StringLength(50, ErrorMessage = "Transaction ID cannot exceed 50 characters.")]
        public string TransactionId { get; set; } = null!;

        [Required(ErrorMessage = "Account Number is required.")]
        [StringLength(30, ErrorMessage = "Account Number cannot exceed 30 characters.")]
        public string? AccountNumber { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Currency Code is required.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency Code must be a 3-letter uppercase code.")]
        public string? CurrencyCode { get; set; }

        [Required(ErrorMessage = "Transaction Date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? TransactionDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression(@"^(Approved|Failed|Finished)$", ErrorMessage = "Status must be one of the following: Approved, Failed, or Finished.")]
        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
