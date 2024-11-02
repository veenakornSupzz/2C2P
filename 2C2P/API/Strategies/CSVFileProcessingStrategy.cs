using API.Models;
using System.Globalization;

namespace API.Strategies
{
    public class CSVFileProcessingStrategy : IFileProcessingStrategy
    {
        public async Task<List<Transaction>> ProcessAsync(Stream fileStream)
        {
            var transactions = new List<Transaction>();

            using (var reader = new StreamReader(fileStream))
            {
                string headerLine = await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line.Split(',');

                    if (values.Length >= 5)
                    {
                        var transaction = new Transaction
                        {
                            TransactionId = values[0].Trim('"'),
                            AccountNumber = values[1].Trim('"'),
                            Amount = decimal.TryParse(values[2].Trim('"'), out var amount) ? amount : 0,
                            CurrencyCode = values[3].Trim('"'),
                            Status = values[5].Trim('"'),
                            TransactionDate = ParseDate(values[4].Trim('"'))
                        };
                        transactions.Add(transaction);
                    }
                }
            }
            return transactions;
        }

        private DateTime? ParseDate(string dateString)
        {
            var dateFormats = new[] { "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }
    }
}
