using API.Strategies;

namespace API.Factory
{
    public class FileProcessingStrategyFactory
    {
        public IFileProcessingStrategy GetStrategy(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".csv" => new CSVFileProcessingStrategy(),
                ".xml" => new XMLFileProcessingStrategy(),
                _ => throw new NotSupportedException($"Unsupported file format: {fileExtension}")
            };
        }
    }
}
