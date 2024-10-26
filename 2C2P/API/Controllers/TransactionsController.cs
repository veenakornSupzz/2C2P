using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
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

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                var content = await stream.ReadToEndAsync();
                // ตรวจสอบและประมวลผลข้อมูลในไฟล์ CSV/XML ที่นี่
            }

            return Ok(new { message = "File uploaded successfully." });
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
