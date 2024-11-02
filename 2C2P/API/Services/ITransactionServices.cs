using API.Dto;
using API.Models;

namespace API.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponseDTO>> GetAllTransactionsAsync();
        Task<IEnumerable<TransactionResponseDTO>> GetByCurrencyAsync(string currencyCode);

        Task<IEnumerable<TransactionResponseDTO>> GetByStatusAsync(IEnumerable<string> mappedStatuses);
        Task<IEnumerable<TransactionResponseDTO>> GetByDateRangeAsync(DateTime? startDate, DateTime? endDate);
        Task AddTransactionAsync(Transaction transaction);
        Task SaveTransactionsAsync(IEnumerable<Transaction> transactions);
    }
}
