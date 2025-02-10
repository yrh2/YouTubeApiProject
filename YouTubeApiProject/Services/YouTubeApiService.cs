using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;
        private readonly ILogger<YouTubeApiService> _logger; // Add ILogger to log errors

        public YouTubeApiService(IConfiguration configuration, ILogger<YouTubeApiService> logger)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
            _logger = logger;
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query, string duration, string uploaddate)
        {
            try // Add try-catch block to handle exceptions
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _apiKey,
                    ApplicationName = "YouTubeProject"
                });

                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = query;
                searchRequest.MaxResults = 12;
                searchRequest.Type = "video"; // return videos only

                //video duration filter
                if (!string.IsNullOrEmpty(duration))
                {
                    searchRequest.VideoDuration = duration switch
                    {
                        "short" => SearchResource.ListRequest.VideoDurationEnum.Short__,
                        "medium" => SearchResource.ListRequest.VideoDurationEnum.Medium,
                        "long" => SearchResource.ListRequest.VideoDurationEnum.Long__,
                        _ => null
                    };
                }

                //video date filter
                if (!string.IsNullOrEmpty(uploaddate))
                {
                    DateTime publishedAfter = DateTime.UtcNow;
                    if (uploaddate == "today")
                    {
                        publishedAfter = DateTime.UtcNow.AddDays(-1);
                    }
                    else if (uploaddate == "this_week")
                    {
                        publishedAfter = DateTime.UtcNow.AddDays(-7);
                    }
                    else if (uploaddate == "this_month")
                    {
                        publishedAfter = DateTime.UtcNow.AddMonths(-1);
                    }

                    searchRequest.PublishedAfter = publishedAfter;
                }

                var searchResponse = await searchRequest.ExecuteAsync();

                if (searchResponse.Items == null || !searchResponse.Items.Any())
                {
                    _logger.LogWarning("No results found for the query: {Query}", query); // Log warning
                    throw new Exception("No videos found for this search."); // Throw exception
                }

                var videos = searchResponse.Items
                .Where(item => item.Id.Kind == "youtube#video")
                .Select(item => new YouTubeVideoModel
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    VideoUrl = "https://www.youtube.com/watch?v=" + item.Id.VideoId
                }).ToList();
                return videos; // Return the list of videos
            }
            catch (Google.GoogleApiException ex) // Catch Google API exceptions
            {
                _logger.LogError($"YouTube API error: {ex.Message}");
                throw new Exception("Invalid API Key or exceeded API quota. Please check your API credentials.");
            }
            catch (Exception ex) // Catch other exceptions
            {
                _logger.LogError($"Error fetching YouTube data: {ex.Message}");
                throw new Exception("An error occurred while fetching videos. Please try again later.");
            }

        }
    }
}
