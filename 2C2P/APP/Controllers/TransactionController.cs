using Microsoft.AspNetCore.Mvc;
using APP.Models;
using APP.Services;
using APP.ResponseModels;

namespace APP.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly int _pageSize;

        public TransactionController(ITransactionService transactionService, IConfiguration configuration)
        {
            _transactionService = transactionService;
            _pageSize = int.Parse(configuration["PaginationSettings:PageSize"]);
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var apiResponse = await _transactionService.GetTransactionsAsync();
            if (apiResponse.Success)
            {
                var transactions = apiResponse.Data;
                var pagedTransactions = transactions
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                ViewBag.TotalPages = (int)Math.Ceiling(transactions.Count / (double)_pageSize);
                ViewBag.CurrentPage = pageNumber;

                return View(pagedTransactions);
            }

            ViewBag.Message = apiResponse.Message;
            return View(new List<AppTransaction>());
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            const long maxFileSize = 1 * 1024 * 1024; // 1 MB

            if (file == null || file.Length > maxFileSize)
            {
                TempData["Message"] = file == null
                    ? "No file selected for upload."
                    : "File size exceeds the 1 MB limit.";
                TempData["Status"] = "error";
                return RedirectToAction("Index");
            }

            var apiResponse = await _transactionService.UploadTransactionFileAsync(file);
            TempData["Message"] = apiResponse.Success ? apiResponse.Message : apiResponse.Error ?? "An unexpected error occurred.";
            TempData["Status"] = apiResponse.Success ? "success" : "error";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchType, string searchInput, string startDate, string endDate)
        {
            ApiResponse<List<AppTransaction>> apiResponse = searchType switch
            {
                "currency" => await _transactionService.GetTransactionsByCurrencyAsync(searchInput),
                "status" => await _transactionService.GetTransactionsByStatusAsync(searchInput),
                "date" => await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate),
                _ => new ApiResponse<List<AppTransaction>>(false, "Invalid search type.", null)
            };

            if (apiResponse.Success)
            {
                return View("Index", apiResponse.Data);
            }

            ViewBag.Message = apiResponse.Message;
            return View("Index", new List<AppTransaction>());
        }
    }
}
