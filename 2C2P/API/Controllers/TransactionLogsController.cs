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

        // POST: api/TransactionLogs
        [HttpPost]
        public async Task<ActionResult<TransactionLog>> PostTransactionLog(TransactionLog transactionLog)
        {
            _context.TransactionLogs.Add(transactionLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransactionLog", new { id = transactionLog.LogId }, transactionLog);
        }

    }
}
