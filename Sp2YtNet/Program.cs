using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;


namespace Sp2YtNet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayUsage();
                return;
            }

            await Sp2YtNetAuthSecrets.ReadSettingsIntoStaticInstance();
            
            string command = args[0].ToLower();
            switch (command)
            {
                case "list-spotify-playlists":
                    await ListSpotifyPlaylists();
                    break;
                case "list-youtube-playlists":
                    await ListYoutubePlaylists();
                    break;
                case "list-spotify-tracks":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Please provide the name of the Spotify playlist.");
                    }
                    else
                    {
                        string spotifyPlaylistName = args[1];
                        await ListSpotifyTracks(spotifyPlaylistName);
                    }
                    break;
                case "list-youtube-tracks":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Please provide the name of the Youtube playlist.");
                    }
                    else
                    {
                        string youtubePlaylistName = args[1];
                        await ListYoutubeTracks(youtubePlaylistName);
                    }
                    break;
                case "transfer-to-youtube":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Please provide the names of both the Spotify and Youtube playlists.");
                    }
                    else
                    {
                        string spotifyPlaylistName = args[1];
                        string youtubePlaylistName = args[2];
                        await TransferToYoutube(spotifyPlaylistName, youtubePlaylistName);
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown command '{command}'.");
                    DisplayUsage();
                    break;
            }
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage: Sp2Yt <command>");
            Console.WriteLine("Commands:");
            Console.WriteLine("  list-spotify-playlists");
            Console.WriteLine("  list-youtube-playlists");
            Console.WriteLine("  list-spotify-tracks <Spotify playlist name>");
            Console.WriteLine("  list-youtube-tracks <Youtube playlist name>");
            Console.WriteLine("  transfer-to-youtube <Spotify playlist name> <Youtube playlist name>");
        }

        static async Task ListSpotifyPlaylists()
        {
            var spotify = await SpotifyOperations.LoginAndGetSpotifyClientAsync();

            // Get Spotify playlists
            var spotifyPlaylists = await spotify.GetSpotifyPlaylistsAsync();

            if (spotifyPlaylists == null || spotifyPlaylists.Count == 0)
            {
                Console.WriteLine("No playlists found in Spotify.");
                return;
            }

            // List existing Spotify playlists
            Console.WriteLine("Existing Spotify playlists:");
            for (int i = 0; i < spotifyPlaylists.Count; i++)
            {
                Console.WriteLine($"- {spotifyPlaylists[i].Name}");
            }
        }

        static async Task ListYoutubePlaylists()
        {
            var youtube = await YtOperations.GetYoutubeServiceAsync();

            // Get Youtube playlists
            var youtubePlaylists = await youtube.GetExistingPlaylists();

            if (youtubePlaylists == null || youtubePlaylists.Count == 0)
            {
                Console.WriteLine("No playlists found in YouTube.");
                return;
            }

            // List existing Youtube playlists
            Console.WriteLine("Existing YouTube playlists:");
            for (int i = 0; i < youtubePlaylists.Count; i++)
            {
                Console.WriteLine($"- {youtubePlaylists[i].Snippet.Title}");
            }
        }

        static async Task ListSpotifyTracks(string playlistName)
        {
            var spotify = await SpotifyOperations.LoginAndGetSpotifyClientAsync();
            var trackNames = await spotify.GetTrackQueriesWithPlaylistNameAsync(playlistName);

            if (trackNames is null)
            {
                Console.WriteLine($"No playlist found with name: {playlistName}");
            }
            else if (trackNames.Count == 0)
            {
                Console.WriteLine($"Playlist contains no tracks: {playlistName}");
                return;
            }
            // List existing Spotify playlists
            Console.WriteLine($"Tracks in playlist {playlistName}:");
            for (int i = 0; i < trackNames.Count; i++)
            {
                Console.WriteLine($"- {trackNames[i]}");
            }
        }

        static async Task ListYoutubeTracks(string playlistName)
        {
            var youtube = await YtOperations.GetYoutubeServiceAsync();
            var playlist = await youtube.GetYouTubePlaylist(playlistName);
            if (playlist is null)
            {
                Console.WriteLine($"No playlist found with name: {playlistName}");
            }

            var tracks = await youtube.GetAllTracksInAPlaylistAsync(playlist.Id);
            if (tracks.Count == 0)
            {
                Console.WriteLine($"Playlist contains no tracks: {playlistName}");
                return;
            }

            // List existing Youtube playlists
            Console.WriteLine($"Tracks in playlist {playlistName}:");
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"- {tracks[i].Snippet.Title} - {tracks[i].Snippet.VideoOwnerChannelTitle.TrimEndWith(" - Topic")}");
            }
        }

        static async Task TransferToYoutube(string spotifyPlaylistName, string youtubePlaylistName)
        {
            var spotify = await SpotifyOperations.LoginAndGetSpotifyClientAsync();


            // Get the selected Spotify playlist for transferring tracks
            SimplePlaylist selectedSpotifyPlaylist = await spotify.GetPlaylistAsync(spotifyPlaylistName);
            var queries = await spotify.GetTrackQueriesAsync(selectedSpotifyPlaylist.Id);

            var youtubeService = await YtOperations.GetYoutubeServiceAsync();
            // Transfer tracks to YouTube Music playlist
            await youtubeService.AddQueriesToPlaylist(youtubePlaylistName, queries);

            Console.WriteLine("Transfer completed.");
        }
    }
}
