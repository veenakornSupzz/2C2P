using API.Dto;
using API.Models;
using API.Repository;
using AutoMapper;

namespace API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionResponseDTO>> GetAllTransactionsAsync()
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync();
            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<IEnumerable<TransactionResponseDTO>> GetByCurrencyAsync(string currencyCode)
        {
            var transactions = await _transactionRepository.GetByCurrencyAsync(currencyCode);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<IEnumerable<TransactionResponseDTO>> GetByDateRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<IEnumerable<TransactionResponseDTO>> GetByStatusAsync(IEnumerable<string> mappedStatuses)
        {
            var transactions = await _transactionRepository.GetByStatusAsync(mappedStatuses);
            return _mapper.Map<IEnumerable<TransactionResponseDTO>>(transactions);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _transactionRepository.AddTransactionAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
        }

        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            await _transactionRepository.SaveTransactionsAsync(transactions);
        }
    }
}
