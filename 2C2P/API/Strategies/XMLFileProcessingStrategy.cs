using API.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace API.Strategies
{
    public class XMLFileProcessingStrategy : IFileProcessingStrategy
    {
        public async Task<List<Transaction>> ProcessAsync(Stream fileStream)
        {
            var transactions = new List<Transaction>();
            var xml = await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);

            var transactionElements = xml.Descendants("Transaction");

            foreach (var element in transactionElements)
            {
                var transaction = new Transaction
                {
                    TransactionId = element.Attribute("id")?.Value,
                    AccountNumber = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "AccountNo")?.Value,
                    Amount = decimal.TryParse(
                        element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Amount")?.Value, out var amount) ? amount : 0,
                    CurrencyCode = ValidateCurrencyCode(element.Descendants().FirstOrDefault(e => e.Name.LocalName == "CurrencyCode")?.Value)
                                    ? element.Descendants().FirstOrDefault(e => e.Name.LocalName == "CurrencyCode")?.Value : null,
                    Status = MapStatus(element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Status")?.Value),
                    TransactionDate = ParseDate(element.Descendants().FirstOrDefault(e => e.Name.LocalName == "TransactionDate")?.Value)
                };
                transactions.Add(transaction);
            }
            return transactions;
        }

        private DateTime? ParseDate(string dateString)
        {
            var dateFormats = new[] { "yyyy-MM-ddTHH:mm:ss" }; 
            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }

        private string MapStatus(string status)
        {
            return status switch
            {
                "Approved" => "Approved",
                "Rejected" => "Failed",  // Mapping 'Rejected' to 'Failed'
                "Done" => "Finished",    // Mapping 'Done' to 'Finished'
                _ => null
            };
        }

        private bool ValidateCurrencyCode(string code)
        {
            return Regex.IsMatch(code, @"^[A-Z]{3}$"); // ISO4217 format (3 uppercase letters)
        }
    }
}
