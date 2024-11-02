using API.Models;

namespace API.Services
{
    public interface IFileProcessorService
    {
        Task<List<Transaction>> ProcessFileAsync(IFormFile file);
    }
}