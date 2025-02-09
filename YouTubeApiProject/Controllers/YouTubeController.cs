using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Display Search Page
        public IActionResult Index()
        {
            return View(new List<YouTubeVideoModel>()); // Pass an empty list initially
        }

        // Handle the search query with filters
        [HttpPost]
        public async Task<IActionResult> Search(string query, string duration, string uploadDate)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View("Index", new List<YouTubeVideoModel>());
            }

            var videos = await _youtubeService.SearchVideosAsync(query, duration, uploadDate);
            return View("Index", videos);
        }
    }
}

