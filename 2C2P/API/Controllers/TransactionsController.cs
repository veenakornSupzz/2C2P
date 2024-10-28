using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;
using System.Xml.Serialization;
using System.Data.SqlTypes;
using System.Globalization;
using System.Xml.Linq;

namespace API.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionContext _context;

        public TransactionsController(TransactionContext context)
        {
            _context = context;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        [HttpGet("currency/{currencyCode}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetByCurrency(string currencyCode)
        {
            var transactions = await _context.Transactions
                .Where(t => t.CurrencyCode == currencyCode)
                .ToListAsync();
            return transactions;
        }

        [HttpGet("date")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToListAsync();
            return transactions;
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetByStatus(string status)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Status == status)
                .ToListAsync();
            return transactions;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTransactionFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            var transactions = new List<Transaction>();

            var extension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    if (extension == ".csv")
                    {
                        using (var reader = new StreamReader(stream))
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
                                        TransactionId = values[0],
                                        AccountNumber = values[1],
                                        Amount = decimal.TryParse(values[2], out var amount) ? amount : 0,
                                        CurrencyCode = values[3],
                                        Status = values[5]
                                    };

                                    var dateFormats = new[] { "yyyy-MM-dd HH:mm:ss" };
                                    if (DateTime.TryParseExact(values[4], dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                                    {
                                        if (date >= (DateTime)SqlDateTime.MinValue && date <= (DateTime)SqlDateTime.MaxValue)
                                        {
                                            transaction.TransactionDate = date;
                                        }
                                        else
                                        {
                                            transaction.TransactionDate = null;
                                        }
                                    }
                                    else
                                    {
                                        transaction.TransactionDate = null;
                                    }

                                    transactions.Add(transaction);
                                }
                            }
                        }
                    }
                    else if (extension == ".xml")
                    {
                        var xml = XDocument.Load(stream);

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
                                Status = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Status")?.Value
                            };


                            var transactionDateString = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "TransactionDate")?.Value;
                            if (DateTime.TryParse(transactionDateString, out var date))
                            {
                                transaction.TransactionDate = date;
                            }
                            else
                            {
                                transaction.TransactionDate = DateTime.MinValue;
                            }

                            transactions.Add(transaction);
                        }
                    }
                    else
                    {
                        return BadRequest("Unsupported file format. Only CSV and XML are supported.");
                    }
                }

                _context.Transactions.AddRange(transactions);
                await _context.SaveChangesAsync();

                return Ok(new { message = "File uploaded and transactions saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
        }


    }
}
