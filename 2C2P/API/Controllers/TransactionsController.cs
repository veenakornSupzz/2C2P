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
using System.ComponentModel.DataAnnotations;

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
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file.", "File cannot be empty."));
            }

            try
            {
                // Log file upload start
                var logEntry = new TransactionLog { TransactionId = Guid.NewGuid().ToString(), FileName = file.FileName, LogMessage = "File upload started.", CreatedAt = DateTime.UtcNow };
                _context.TransactionLogs.Add(logEntry);
                await _context.SaveChangesAsync();

                // Process the file
                var transactions = await _fileProcessorService.ProcessFileAsync(file);

                // Validate each transaction
                var invalidTransactions = new List<(Transaction transaction, List<string> errors)>();
                foreach (var transaction in transactions)
                {
                    var context = new ValidationContext(transaction, serviceProvider: null, items: null);
                    var validationResults = new List<ValidationResult>();

                    if (!Validator.TryValidateObject(transaction, context, validationResults, validateAllProperties: true))
                    {
                        var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                        invalidTransactions.Add((transaction, errors));
                    }
                }

                if (invalidTransactions.Any())
                {
                    var errorDetails = string.Join("; ", invalidTransactions.Select(it =>
                        $"TransactionId: {it.transaction.TransactionId}, Errors: {string.Join(", ", it.errors)}"));

                    // Log validation failure
                    logEntry.LogMessage = $"Validation failed: {errorDetails}";
                    await _context.SaveChangesAsync();

                    return BadRequest(ApiResponse<string>.ErrorResponse("Validation failed for one or more transactions.", errorDetails));
                }

                // Save valid transactions
                await _transactionService.SaveTransactionsAsync(transactions);

                // Log successful processing
                logEntry.LogMessage = "File uploaded and transactions saved successfully.";
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<string>.SuccessResponse("File uploaded and transactions saved successfully.", null));
            }
            catch (ArgumentException ex)
            {
                // Log argument exception
                var logEntry = new TransactionLog { TransactionId = Guid.NewGuid().ToString(), FileName = file.FileName, LogMessage = $"Argument error: {ex.Message}", CreatedAt = DateTime.UtcNow };
                
                _context.TransactionLogs.Add(logEntry);
                await _context.SaveChangesAsync();

                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, ex.Message));
            }
            catch (NotSupportedException ex)
            {
                // Log not supported exception
                var logEntry = new TransactionLog { TransactionId = Guid.NewGuid().ToString(), FileName = file.FileName, LogMessage = $"Unsupported file format: {ex.Message}", CreatedAt = DateTime.UtcNow };
                _context.TransactionLogs.Add(logEntry);
                await _context.SaveChangesAsync();

                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, ex.Message));
            }
            catch (Exception ex)
            {
                // Log generic exception
                var logEntry = new TransactionLog { TransactionId = Guid.NewGuid().ToString(), FileName = file.FileName, LogMessage = $"Internal server error: {ex.Message}", CreatedAt = DateTime.UtcNow };
                _context.TransactionLogs.Add(logEntry);
                await _context.SaveChangesAsync();

                return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error.", ex.Message));
            }
        }
    }
}
