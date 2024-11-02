using System.Text.Json.Serialization;

namespace APP.Models
{
    public class AppTransaction
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
