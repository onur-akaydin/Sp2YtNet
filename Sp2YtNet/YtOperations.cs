using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeSearchApi.Net.Services;

namespace Sp2YtNet
{
    public static class YtOperations
    {
        public static async Task<YouTubeService> GetYoutubeServiceAsync()
        {
            // YouTube credentials
            UserCredential credential;

            if (File.Exists(Sp2YtNetAuthSecrets.Instance.YtClientSecretsPath) == false)
            {
                Console.WriteLine($"Cannot found file {Sp2YtNetAuthSecrets.Instance.YtClientSecretsPath}. Check this file and {Sp2YtNetAuthSecrets.SECRETS_FILE_PATH} file and try again.");
                Environment.Exit(1);
            }

            using (var stream = new FileStream(Sp2YtNetAuthSecrets.Instance.YtClientSecretsPath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    Sp2YtNetAuthSecrets.Instance.YtScope,
                    Sp2YtNetAuthSecrets.Instance.YtUsername,
                    CancellationToken.None,
                    new FileDataStore(Sp2YtNetAuthSecrets.Instance.YtAppName)
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Sp2YtNetAuthSecrets.Instance.YtAppName
            });

            return youtubeService;
        }


        #region PLAYLIST OPERATIONS

        public static async Task<Playlist> GetYouTubePlaylist(this YouTubeService youtubeService, string playlistTitle)
        {
            List<Playlist> playlists = await GetExistingPlaylists(youtubeService);

            // Search for the playlist with the given title
            var playlist = playlists.FirstOrDefault(p => p.Snippet.Title == playlistTitle);
            return playlist;
        }

        public static async Task<Playlist> GetOrCreateYouTubeMusicPlaylist(this YouTubeService youtubeService, string playlistTitle)
        {
            var playlist = await GetYouTubePlaylist(youtubeService, playlistTitle);

            if (playlist != null)
            {
                return playlist;
            }
            else
            {
                // Playlist not found, create a new one
                var newPlaylist = new Playlist();
                newPlaylist.Snippet = new PlaylistSnippet
                {
                    Title = playlistTitle,
                };
                var playlistInsertRequest = youtubeService.Playlists.Insert(newPlaylist, "snippet"); // 50 unit
                return await playlistInsertRequest.ExecuteAsync();
            }
        }

        public static async Task<List<Playlist>> GetExistingPlaylists(this YouTubeService youtubeService)
        {
            // Get playlists associated with the authenticated user
            var playlistsListRequest = youtubeService.Playlists.List("snippet");
            playlistsListRequest.Mine = true;
            playlistsListRequest.MaxResults = 50;  // max allowed = 50
            var allPlaylists = new List<Playlist>();

            do
            {
                var playlistsListResponse = await playlistsListRequest.ExecuteAsync();
                allPlaylists.AddRange(playlistsListResponse.Items);

                // Check if there are more playlists to retrieve
                playlistsListRequest.PageToken = playlistsListResponse.NextPageToken;

            } while (!string.IsNullOrEmpty(playlistsListRequest.PageToken));

            return allPlaylists;
        }

        public static async Task<List<PlaylistItem>> GetAllTracksInAPlaylistAsync(this YouTubeService youtubeService, string playlistId)
        {
            List<PlaylistItem> tracks = new List<PlaylistItem>();

            string nextPageToken = null;
            do
            {
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = playlistId;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;
                var playlistItemsListResponse = playlistItemsListRequest.Execute();

                nextPageToken = playlistItemsListResponse.NextPageToken;

                tracks.AddRange(playlistItemsListResponse.Items);
            } while (nextPageToken != null);

            return tracks;
        }

        public static async Task<int> AddQueriesToPlaylist(this YouTubeService youtubeService, string youtubePlaylistTitle, List<string> trackQueries)
        {
            //Create YouTube playlist
            var youtubePlaylist = await GetOrCreateYouTubeMusicPlaylist(youtubeService, youtubePlaylistTitle);
            var cachedTracks = await GetAllTracksInAPlaylistAsync(youtubeService, youtubePlaylist.Id);

            // Transfer tracks to YouTube playlist
            foreach (var query in trackQueries)
            {
                // Search for the track on YouTube Music
                string videoIdResult = null;
                // FIRST MAKE MUSIC SEARCH:
                videoIdResult = await SearchYoutubeMusicWithoutApiKey(query, videoIdResult);

                // IF MUSIC IS NOT FOUND, SEARCH FOR VIDEO
                if (videoIdResult == null)
                {
                    videoIdResult = await SearchYoutubeWithoutApiKey(query, videoIdResult);
                }

                if (videoIdResult != null)
                {
                    var videoId = videoIdResult;

                    // Check if the video / track already exists in the YouTube Music playlist
                    bool isTrackInPlaylist = cachedTracks.Any(pli => pli.Snippet.ResourceId.VideoId == videoId);

                    if (!isTrackInPlaylist)
                    {
                        // Add the track to the YouTube playlist
                        PlaylistItem playlistItem = CreatePlaylistItem(youtubePlaylist.Id, videoId);
                        bool success = await AddPlaylistItemToPlaylist(youtubeService, playlistItem);
                        if (success)
                        {
                            cachedTracks.Add(playlistItem);
                            Console.WriteLine($"Added {query} to YouTube Music playlist.");
                        }
                        else
                        {
                            Console.WriteLine($"ERROR during insertion of the song to YouTube Music playlist: {query}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{query} already exists in the YouTube Music playlist. Skipping.");
                    }
                }
                else
                {
                    Console.WriteLine($"No music found for track: {query}");
                }
            }

            return 1;
        }

        private static PlaylistItem CreatePlaylistItem(string playlistId, string videoId)
        {
            var playlistItem = new PlaylistItem();
            playlistItem.Snippet = new PlaylistItemSnippet
            {
                PlaylistId = playlistId,
                ResourceId = new ResourceId
                {
                    Kind = "youtube#video",
                    VideoId = videoId,
                },
            };
            return playlistItem;
        }

        private static async Task<bool> AddPlaylistItemToPlaylist(YouTubeService youtubeService, PlaylistItem playlistItem)
        {
            var playlistItemInsertRequest = youtubeService.PlaylistItems.Insert(playlistItem, "snippet");
            try
            {
                await playlistItemInsertRequest.ExecuteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region SEARCH YOUTUBE WITHOUT AN API KEY

        private static async Task<string> SearchYoutubeMusicWithoutApiKey(string query, string videoIdResult)
        {
            using (var httpClient = new HttpClient())
            {
                YoutubeMusicSearchClient client = new YoutubeMusicSearchClient(httpClient);

                var responseObject = await client.SearchAsync(query);
                if (responseObject.Results.Count > 0)
                {
                    videoIdResult = responseObject.Results.First().Id;
                }
            }

            return videoIdResult;
        }

        private static async Task<string> SearchYoutubeWithoutApiKey(string query, string videoIdResult)
        {
            using (var httpClient = new HttpClient())
            {
                YoutubeSearchClient client = new YoutubeSearchClient(httpClient);

                var responseObject = await client.SearchAsync(query);
                if (responseObject.Results.Count > 0)
                {
                    videoIdResult = responseObject.Results.First().Id;
                }
            }

            return videoIdResult;
        }

        #endregion

        #region SEARCH YOUTUBE WITH API KEY


        /// <summary>
        /// Searches YouTube for a query. A call for this method will cost 100 units of API quota.
        /// That's why searching WITHOUT and API key is preferred, instead of this method.
        /// </summary>
        /// <param name="youtubeService"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static async Task<string> SearchYoutubeWithApiKey(YouTubeService youtubeService, string query)
        {
            string videoIdResult = null;

            var searchListRequest = youtubeService.Search.List("snippet"); // API COST: 100 units!!!
            searchListRequest.Q = query;
            searchListRequest.Type = "video";
            searchListRequest.VideoCategoryId = "10";
            searchListRequest.MaxResults = 1;
            var searchListResponse = await searchListRequest.ExecuteAsync();

            if (searchListResponse.Items.Count > 0)
                videoIdResult = searchListResponse.Items[0].Id.VideoId;
            return videoIdResult;
        }

        #endregion
    }
}