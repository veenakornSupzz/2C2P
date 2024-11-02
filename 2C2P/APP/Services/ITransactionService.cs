using APP.Models;
using APP.ResponseModels;

namespace APP.Services
{
    public interface ITransactionService
    {
        Task<ApiResponse<List<AppTransaction>>> GetTransactionsAsync();
        Task<ApiResponse<List<AppTransaction>>> GetTransactionsByCurrencyAsync(string currencyCode);
        Task<ApiResponse<List<AppTransaction>>> GetTransactionsByDateRangeAsync(string startDate, string endDate);
        Task<ApiResponse<List<AppTransaction>>> GetTransactionsByStatusAsync(string status);
        Task<ApiResponse<string>> UploadTransactionFileAsync(IFormFile file);
    }
}
