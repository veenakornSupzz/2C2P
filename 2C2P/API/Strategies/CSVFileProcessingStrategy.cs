using API.Models;
using System.Globalization;
using System.Text.RegularExpressions;

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
                    // Use Regex to split by commas that are outside of quotes
                    var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    if (values.Length >= 5)
                    {
                        var transaction = new Transaction
                        {
                            TransactionId = values[0].Trim('"'),
                            AccountNumber = values[1].Trim('"'),
                            Amount = decimal.TryParse(values[2].Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) ? amount : 0,
                            CurrencyCode = ValidateCurrencyCode(values[3].Trim('"')) ? values[3].Trim('"') : null,
                            Status = MapStatus(values[5].Trim('"'), isCsv: true),
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
            var dateFormats = new[] { "dd/MM/yyyy HH:mm:ss" }; 
            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }

        private string MapStatus(string status, bool isCsv)
        {
            if (isCsv)
            {
                return status switch
                {
                    "Approved" => "Approved",
                    "Failed" => "Failed",
                    "Finished" => "Finished",
                    _ => null
                };
            }
            return null;
        }

        private bool ValidateCurrencyCode(string code)
        {
            return Regex.IsMatch(code, @"^[A-Z]{3}$");
        }
    }
}
