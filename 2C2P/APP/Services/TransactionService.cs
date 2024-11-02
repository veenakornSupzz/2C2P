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

        public TransactionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7250");
            _httpClient.Timeout = TimeSpan.FromSeconds(300);
        }

        private async Task<ApiResponse<T>> ProcessApiResponse<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            // Optional: Log jsonString for debugging
            Console.WriteLine(jsonString);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // ให้รองรับ JSON key แบบ case-insensitive
            };

            // ใช้การตรวจสอบ response ก่อน Deserialize
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>(false, "Failed to process API response.", default, response.ReasonPhrase);
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
