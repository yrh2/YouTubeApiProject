using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query, string duration, string uploaddate)
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

            var videos = searchResponse.Items
                .Where(item => item.Id.Kind == "youtube#video")
                .Select(item => new YouTubeVideoModel
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    VideoUrl = "https://www.youtube.com/watch?v=" + item.Id.VideoId
                }).ToList();

            return videos;
        }
    }
}