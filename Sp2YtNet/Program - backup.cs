using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyToYoutube;
using YoutubeSearchApi.Net.Models.Youtube;
using YoutubeSearchApi.Net.Services;

namespace Sp2YtNet
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var spotify = await SpotifyOperations.LoginAndGetSpotifyClientAsync();

            // Get Spotify playlists of the authenticated user
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
                Console.WriteLine($"{i + 1}. {spotifyPlaylists[i].Name}");
            }

            // Ask user to select a playlist
            Console.Write("Select a playlist number to transfer to YouTube Music: ");
            string input = Console.ReadLine();
            if (!int.TryParse(input, out int selectedPlaylistIndex) || selectedPlaylistIndex < 1 || selectedPlaylistIndex > spotifyPlaylists.Count)
            {
                Console.WriteLine("Invalid playlist selection. Aborting.");
                return;
            }

            // Get the selected Spotify playlist for transferring tracks
            SimplePlaylist selectedSpotifyPlaylist = spotifyPlaylists[selectedPlaylistIndex - 1];
            string ytPlaylistTitle = $"{selectedSpotifyPlaylist.Name} (from Spotify)";

            var youtubeService = await YtOperations.GetYoutubeServiceAsync();
            // Transfer tracks to YouTube Music playlist
            await youtubeService.AddQueriesToPlaylist(ytPlaylistTitle, await spotify.GetTrackQueriesAsync(selectedSpotifyPlaylist.Id));

            Console.WriteLine("Transfer completed.");
        }        
    }
}
