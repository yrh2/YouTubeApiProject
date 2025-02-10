using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;
        private readonly ILogger<YouTubeController> _logger; // Add ILogger to log errors


        public YouTubeController(YouTubeApiService youtubeService,ILogger<YouTubeController> logger)
        {
            _youtubeService = youtubeService;
            _logger = logger; // Assign logger
        }

        // Display Search Page
        public IActionResult Index()
        {
            return View(new List<YouTubeVideoModel>()); // Pass an empty list initially
        }

        // Handle the search query with filters
        [HttpPost]
        public async Task<IActionResult> Search(string query, string duration, string uploaddate)
        {
            if (string.IsNullOrEmpty(query))
            {
                TempData["ErrorMessage"] = "Please enter a search query."; // Display error message
                return View("Index", new List<YouTubeVideoModel>());
            }

            try 
            {
                var videos = await _youtubeService.SearchVideosAsync(query, duration, uploaddate);
                return View("Index", videos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during search: {ex.Message}"); // Log error
                TempData["ErrorMessage"] = ex.Message; // Display error message
                return View("Index", new List<YouTubeVideoModel>()); // Return an empty list
            }
        }
    }
}

