using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionContext _context;

        public TransactionRepository(TransactionContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByCurrencyAsync(string currencyCode)
        {
            return await _context.Transactions
                .Where(t => t.CurrencyCode == currencyCode)
                .ToListAsync();
        }


        public async Task<IEnumerable<Transaction>> GetByStatusAsync(IEnumerable<string> mappedStatuses)
        {
            // ดึงข้อมูลจากฐานข้อมูลที่ status ไม่เป็น null ก่อน
            var transactions = await _context.Transactions
                .Where(t => t.Status != null) // กรองข้อมูลในระดับฐานข้อมูลที่ status ไม่เป็น null
                .ToListAsync();

            // ใช้ Contains ในระดับ memory โดยทำการเปรียบเทียบแบบไม่สนใจตัวพิมพ์ใหญ่-เล็ก
            var filteredTransactions = transactions
                .Where(t => mappedStatuses.Any(ms => string.Equals(ms, t.Status, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredTransactions;
        }
        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            return await query.ToListAsync();
        }
        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }
    }
}
