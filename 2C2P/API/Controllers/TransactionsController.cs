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
            var transactions = await _context.Transactions.ToListAsync();

            var mappedTransactions = transactions.Select(t => new
            {
                Id = t.Id,
                TransactionId = t.TransactionId,
                AccountNumber = t.AccountNumber,
                Amount = t.Amount,
                CurrencyCode = t.CurrencyCode,
                TransactionDate = t.TransactionDate,
                Status = MapStatus(t.Status),
                IsValid = t.IsValid,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(mappedTransactions);
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
        public async Task<ActionResult<IEnumerable<Transaction>>> GetByDateRange(DateTime? startDate, DateTime? endDate)
        {
            // ตรวจสอบว่า startDate น้อยกว่า endDate
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Start date must be less than or equal to end date.");
            }

            var query = _context.Transactions.AsQueryable();

            // ถ้ามี startDate ให้ค้นหาตั้งแต่ startDate ขึ้นไป
            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            // ถ้ามี endDate ให้ค้นหาถึง endDate
            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            var transactions = await query.ToListAsync();
            return transactions;
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetByStatus(string status)
        {
            // Mapping status ที่ส่งเข้ามาให้เป็นกลุ่มของสถานะจริงในฐานข้อมูล
            List<string> mappedStatuses = status switch
            {
                "A" => new List<string> { "Approved" },
                "R" => new List<string> { "Rejected", "Failed" },
                "D" => new List<string> { "Finished", "Done" },
                _ => new List<string> { status } // กรณีที่ status ไม่ได้ map ไว้ ใช้ค่านั้นเลย
            };

            // แปลงค่าใน mappedStatuses เป็นตัวพิมพ์ใหญ่และลบช่องว่างทั้งหมด
            mappedStatuses = mappedStatuses.Select(s => s.ToUpper().Trim()).ToList();

            // ดึงข้อมูลจากฐานข้อมูลแล้วใช้ Contains ในหน่วยความจำ
            var transactions = _context.Transactions
                .AsEnumerable() // โหลดข้อมูลทั้งหมดมาที่ memory
                .Where(t => mappedStatuses.Contains(t.Status?.ToUpper().Trim() ?? string.Empty)) // ใช้ Contains กับ ToUpper และ Trim
                .ToList(); // ใช้ ToList() แทน ToListAsync()

            return Ok(transactions);
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
                                        TransactionId = values[0].Trim('"'),
                                        AccountNumber = values[1].Trim('"'),
                                        Amount = decimal.TryParse(values[2].Trim('"'), out var amount) ? amount : 0,
                                        CurrencyCode = values[3].Trim('"'),
                                        Status = values[5].Trim('"')
                                    };

                                    var dateFormats = new[] { "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
                                    var dateString = values[4].Trim('"');
                                    if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
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


        private string MapStatus(string status)
        {
            return status switch
            {
                "Approved" => "A",
                "Failed" => "R",
                "Finished" => "D",
                "Rejected" => "R",
                "Done" => "D",
                _ => status
            };
        }


    }
}
