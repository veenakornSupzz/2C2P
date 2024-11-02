using System.Text.Json.Serialization;

namespace APP.Models
{
    public class AppTransaction
    {
        public int Id { get; set; }

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("transactionDate")]
        public DateTime? TransactionDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        public string Payment => $"{Amount} {CurrencyCode}";
    }
}
