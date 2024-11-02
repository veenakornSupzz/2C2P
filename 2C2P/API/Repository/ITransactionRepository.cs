using API.Models;

namespace API.Repository
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<IEnumerable<Transaction>> GetByStatusAsync(IEnumerable<string> mappedStatuses);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<Transaction>> GetByCurrencyAsync(string currencyCode);
        Task AddTransactionAsync(Transaction transaction);
        Task SaveChangesAsync();
        Task SaveTransactionsAsync(IEnumerable<Transaction> transactions);
    }
}
