using API.Models;

namespace API.Strategies
{
    public interface IFileProcessingStrategy
    {
        Task<List<Transaction>> ProcessAsync(Stream fileStream);
    }
}
