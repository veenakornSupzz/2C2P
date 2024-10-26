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
    public class TransactionLogsController : ControllerBase
    {
        private readonly TransactionContext _context;

        public TransactionLogsController(TransactionContext context)
        {
            _context = context;
        }

        // GET: api/TransactionLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionLog>>> GetTransactionLogs()
        {
            return await _context.TransactionLogs.ToListAsync();
        }

        // GET: api/TransactionLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionLog>> GetTransactionLog(int id)
        {
            var transactionLog = await _context.TransactionLogs.FindAsync(id);

            if (transactionLog == null)
            {
                return NotFound();
            }

            return transactionLog;
        }

        // PUT: api/TransactionLogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransactionLog(int id, TransactionLog transactionLog)
        {
            if (id != transactionLog.LogId)
            {
                return BadRequest();
            }

            _context.Entry(transactionLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TransactionLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TransactionLog>> PostTransactionLog(TransactionLog transactionLog)
        {
            _context.TransactionLogs.Add(transactionLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransactionLog", new { id = transactionLog.LogId }, transactionLog);
        }

        // DELETE: api/TransactionLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransactionLog(int id)
        {
            var transactionLog = await _context.TransactionLogs.FindAsync(id);
            if (transactionLog == null)
            {
                return NotFound();
            }

            _context.TransactionLogs.Remove(transactionLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionLogExists(int id)
        {
            return _context.TransactionLogs.Any(e => e.LogId == id);
        }
    }
}
