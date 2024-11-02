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
using API.Services;
using API.Dto;
using API.ResponseModels;

namespace API.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IFileProcessorService _fileProcessorService;


        public TransactionsController(TransactionContext context, ITransactionService transactionService, IFileProcessorService fileProcessorService)
        {
            _context = context;
            _transactionService = transactionService;
            _fileProcessorService = fileProcessorService;
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDTO>>>> GetTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("Transactions retrieved successfully.", transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<TransactionResponseDTO>>.ErrorResponse("Failed to retrieve transactions.", ex.Message));
            }
        }

        [HttpGet("currency/{currencyCode}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDTO>>>> GetByCurrency(string currencyCode)
        {
            try
            {
                var transactions = await _transactionService.GetByCurrencyAsync(currencyCode);

                if (transactions == null || !transactions.Any())
                {
                    return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("No transactions found for the specified currency.", transactions));
                }

                return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("Transactions retrieved successfully.", transactions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TransactionResponseDTO>>.ErrorResponse("Failed to retrieve transactions.", ex.Message));
            }
        }

        /// <summary>
        /// Get transactions within a date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (e.g., 2024-11-01T00:00:00).</param>
        /// <param name="endDate">The end date of the range (e.g., 2024-11-30T23:59:59).</param>
        [HttpGet("date")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDTO>>>> GetByDateRange(DateTime? startDate, DateTime? endDate)
        {
            // ตรวจสอบว่า startDate น้อยกว่า endDate
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(ApiResponse<IEnumerable<TransactionResponseDTO>>.ErrorResponse("Start date must be less than or equal to end date.", null));
            }

            try
            {
                var transactions = await _transactionService.GetByDateRangeAsync(startDate, endDate);

                if (transactions == null || !transactions.Any())
                {
                    return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("No transactions found for the specified date range.", transactions));
                }

                return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("Transactions retrieved successfully.", transactions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TransactionResponseDTO>>.ErrorResponse("Failed to retrieve transactions.", ex.Message));
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponseDTO>>>> GetByStatus(string status)
        {
            var mappedStatuses = status switch
            {
                "A" => new List<string> { "Approved" },
                "R" => new List<string> { "Rejected", "Failed" },
                "D" => new List<string> { "Finished", "Done" },
                _ => new List<string> { status }
            };

            mappedStatuses = mappedStatuses.Select(s => s.ToUpper().Trim()).ToList();

            try
            {
                var transactions = await _transactionService.GetByStatusAsync(mappedStatuses);
                return Ok(ApiResponse<IEnumerable<TransactionResponseDTO>>.SuccessResponse("Transactions retrieved successfully.", transactions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TransactionResponseDTO>>.ErrorResponse("Failed to retrieve transactions.", ex.Message));
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTransactionFile([FromForm] IFormFile file)
        {
            try
            {
                var transactions = await _fileProcessorService.ProcessFileAsync(file);
                await _transactionService.SaveTransactionsAsync(transactions); 

                return Ok(ApiResponse<string>.SuccessResponse("File uploaded and transactions saved successfully.", null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, null));
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error.", ex.Message));
            }
        }


        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadTransactionFile([FromForm] IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded or file is empty.");
        //    }

        //    var transactions = new List<Transaction>();

        //    var extension = Path.GetExtension(file.FileName).ToLower();

        //    try
        //    {
        //        using (var stream = file.OpenReadStream())
        //        {
        //            if (extension == ".csv")
        //            {
        //                using (var reader = new StreamReader(stream))
        //                {
        //                    string headerLine = await reader.ReadLineAsync();

        //                    while (!reader.EndOfStream)
        //                    {
        //                        var line = await reader.ReadLineAsync();
        //                        var values = line.Split(',');

        //                        if (values.Length >= 5)
        //                        {
        //                            var transaction = new Transaction
        //                            {
        //                                TransactionId = values[0].Trim('"'),
        //                                AccountNumber = values[1].Trim('"'),
        //                                Amount = decimal.TryParse(values[2].Trim('"'), out var amount) ? amount : 0,
        //                                CurrencyCode = values[3].Trim('"'),
        //                                Status = values[5].Trim('"')
        //                            };

        //                            var dateFormats = new[] { "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
        //                            var dateString = values[4].Trim('"');
        //                            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        //                            {
        //                                if (date >= (DateTime)SqlDateTime.MinValue && date <= (DateTime)SqlDateTime.MaxValue)
        //                                {
        //                                    transaction.TransactionDate = date;
        //                                }
        //                                else
        //                                {
        //                                    transaction.TransactionDate = null;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                transaction.TransactionDate = null;
        //                            }

        //                            transactions.Add(transaction);
        //                        }
        //                    }
        //                }
        //            }
        //            else if (extension == ".xml")
        //            {
        //                var xml = XDocument.Load(stream);

        //                var transactionElements = xml.Descendants("Transaction");

        //                foreach (var element in transactionElements)
        //                {
        //                    var transaction = new Transaction
        //                    {
        //                        TransactionId = element.Attribute("id")?.Value,
        //                        AccountNumber = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "AccountNo")?.Value,
        //                        Amount = decimal.TryParse(
        //                            element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Amount")?.Value, out var amount) ? amount : 0,
        //                        CurrencyCode = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "CurrencyCode")?.Value,
        //                        Status = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "Status")?.Value
        //                    };


        //                    var transactionDateString = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "TransactionDate")?.Value;
        //                    if (DateTime.TryParse(transactionDateString, out var date))
        //                    {
        //                        transaction.TransactionDate = date;
        //                    }
        //                    else
        //                    {
        //                        transaction.TransactionDate = DateTime.MinValue;
        //                    }

        //                    transactions.Add(transaction);
        //                }
        //            }
        //            else
        //            {
        //                return BadRequest("Unsupported file format. Only CSV and XML are supported.");
        //            }
        //        }

        //        _context.Transactions.AddRange(transactions);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "File uploaded and transactions saved successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
        }




    }
}
