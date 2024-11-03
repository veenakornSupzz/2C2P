using System.Net.Http.Headers;
using System.Text.Json;
using APP.Models;
using APP.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace APP.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly HttpClient _httpClient;

        public TransactionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            var timeoutInSeconds = int.Parse(configuration["ApiSettings:TimeoutInSeconds"]);

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
        }

        private async Task<ApiResponse<T>> ProcessApiResponse<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // ให้รองรับ JSON key แบบ case-insensitive
            };


            if (!response.IsSuccessStatusCode)
            {
                // Deserialize jsonString เพื่อดึงข้อความ error จากฝั่ง API
                var apiErrorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(jsonString, options);
                var errorMessage = apiErrorResponse?.Error ?? response.ReasonPhrase;

                return new ApiResponse<T>(false, "Failed to process API response.", default, errorMessage);
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(jsonString, options);
        }

        public async Task<ApiResponse<List<AppTransaction>>> GetTransactionsAsync()
        {
            var response = await _httpClient.GetAsync("/api/transaction");
            return await ProcessApiResponse<List<AppTransaction>>(response);
        }

        public async Task<ApiResponse<List<AppTransaction>>> GetTransactionsByCurrencyAsync(string currencyCode)
        {
            var response = await _httpClient.GetAsync($"/api/transaction/currency/{currencyCode}");
            return await ProcessApiResponse<List<AppTransaction>>(response);
        }

        public async Task<ApiResponse<List<AppTransaction>>> GetTransactionsByDateRangeAsync(string startDate, string endDate)
        {
            var url = $"/api/transaction/date?startDate={startDate}&endDate={endDate}";
            var response = await _httpClient.GetAsync(url);
            return await ProcessApiResponse<List<AppTransaction>>(response);
        }

        public async Task<ApiResponse<List<AppTransaction>>> GetTransactionsByStatusAsync(string status)
        {
            var response = await _httpClient.GetAsync($"/api/transaction/status/{status}");
            return await ProcessApiResponse<List<AppTransaction>>(response);
        }

        public async Task<ApiResponse<string>> UploadTransactionFileAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream())
            {
                Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
            };
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/transaction/upload", content);
            return await ProcessApiResponse<string>(response);
        }
    }
}
