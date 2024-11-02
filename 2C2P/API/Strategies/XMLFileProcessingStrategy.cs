using API.Models;
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
                    CurrencyCode = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "CurrencyCode")?.Value,
                    Status = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Status")?.Value,
                    TransactionDate = ParseDate(element.Descendants().FirstOrDefault(e => e.Name.LocalName == "TransactionDate")?.Value)
                };
                transactions.Add(transaction);
            }
            return transactions;
        }

        private DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                return date;
            }
            return null;
        }
    }
}
