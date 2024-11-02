using API.Factory;
using API.Models;

namespace API.Services
{
    public class FileProcessorService : IFileProcessorService
    {
        private readonly FileProcessingStrategyFactory _strategyFactory;

        public FileProcessorService(FileProcessingStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public async Task<List<Transaction>> ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded or file is empty.");

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var strategy = _strategyFactory.GetStrategy(fileExtension);

            using (var stream = file.OpenReadStream())
            {
                return await strategy.ProcessAsync(stream);
            }
        }
    }
}
