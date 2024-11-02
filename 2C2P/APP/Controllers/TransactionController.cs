using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;
using APP.Models;

namespace APP.Controllers
{
    public class TransactionController : Controller
    {
        private readonly HttpClient _httpClient;

        public TransactionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7250");
            _httpClient.Timeout = TimeSpan.FromSeconds(300);
        }

        // GET: /Transaction/Index
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 4)
        {
            var response = await _httpClient.GetAsync("/api/transaction");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<AppTransaction>>(jsonString);

                // คำนวณการแบ่งหน้า
                var pagedTransactions = transactions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // ส่งข้อมูลจำนวนหน้าทั้งหมดและหน้าปัจจุบันไปที่ View
                ViewBag.TotalPages = (int)Math.Ceiling(transactions.Count / (double)pageSize);
                ViewBag.CurrentPage = pageNumber;

                return View(pagedTransactions);
            }

            ViewBag.Message = "ไม่พบข้อมูลธุรกรรม";
            return View(new List<AppTransaction>());
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
                TempData["Message"] = "File uploaded successfully.";
                TempData["Status"] = "success";
            }
            else
            {
                TempData["Message"] = "Failed to upload the file.";
                TempData["Status"] = "error";
            }

            return RedirectToAction("Index");
        }

        // GET: /Transaction/Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchType, string searchInput, string startDate, string endDate)
        {
            string url = "/api/transaction";
            if (searchType == "currency")
                url += $"/currency/{searchInput}";
            else if (searchType == "status")
                url += $"/status/{searchInput}";
            else if (searchType == "date")
            {
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate) && DateTime.Parse(startDate) > DateTime.Parse(endDate))
                {
                    TempData["ErrorMessage"] = "Start date must be less than or equal to end date.";
                    return RedirectToAction("Index");
                }
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    // กรณีที่มีทั้ง startDate และ endDate
                    url += $"/date?startDate={startDate.Trim()}&endDate={endDate.Trim()}";
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    // กรณีที่มีแค่ startDate (ตั้งแต่ startDate ขึ้นไป)
                    url += $"/date?startDate={startDate.Trim()}";
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    // กรณีที่มีแค่ endDate (จนถึง endDate)
                    url += $"/date?endDate={endDate.Trim()}";
                }
                else
                {
                    // ถ้าไม่มีค่าใดเลย
                    ViewBag.Message = "กรุณาเลือกช่วงวันที่ที่ต้องการค้นหา";
                    return View("Index");
                }
            }
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<AppTransaction>>(jsonString);
                return View("Index", transactions);
            }

            ViewBag.Message = "No transactions found.";
            return View("Index");
        }
    }
}
