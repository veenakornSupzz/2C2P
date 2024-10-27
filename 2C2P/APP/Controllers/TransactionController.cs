using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;

namespace APP.Controllers
{
    public class TransactionController : Controller
    {
        private readonly HttpClient _httpClient;

        public TransactionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5000"); // เปลี่ยนเป็น API ของคุณ
        }

        // GET: /Transaction/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Transaction/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a valid file.";
                return View("Index");
            }

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/transaction/upload", content);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "File uploaded successfully.";
            }
            else
            {
                ViewBag.Message = "Failed to upload the file.";
            }

            return View("Index");
        }

        // GET: /Transaction/Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchType, string searchInput)
        {
            string url = "/api/transaction";
            if (searchType == "currency")
                url += $"/currency/{searchInput}";
            else if (searchType == "status")
                url += $"/status/{searchInput}";
            else if (searchType == "date")
            {
                var dates = searchInput.Split("to");
                url += $"/date?startDate={dates[0].Trim()}&endDate={dates[1].Trim()}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(jsonString);
                return View("Index", transactions);
            }

            ViewBag.Message = "No transactions found.";
            return View("Index");
        }
    }
}
