using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Text.Json;

namespace Sp2YtNet;
public static class SpotifyOperations
{
    #region AUTHORIZATION 

    private static SpotifyClient spotifyClient = null;
    private static EmbedIOAuthServer _server;
    private static TaskCompletionSource<bool> authorizationCompleted = new TaskCompletionSource<bool>();
    private static bool hasError = false;

    public static async Task<SpotifyClient> LoginAndGetSpotifyClientAsync()
    {
        var existingToken = await ReadToken();

        if (existingToken is not null && existingToken.IsExpired == false)
        {
            spotifyClient = new SpotifyClient(existingToken);
            Console.WriteLine(value: "Spotify authorization completed using existing access token.");
            return spotifyClient;
        }

        Helpers.ActivateOrDeactivateConsoleOutputs(false);

        // Make sure "redirectUri" is in your spotify application as redirect uri!
        _server = new EmbedIOAuthServer(new Uri(Sp2YtNetAuthSecrets.Instance.SpotifyRedirectUri), Helpers.GetPortFromUri(Sp2YtNetAuthSecrets.Instance.SpotifyRedirectUri));
        await _server.Start();

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        // Start the authorization process
        var request = new LoginRequest(_server.BaseUri, Sp2YtNetAuthSecrets.Instance.SpotifyClientId, LoginRequest.ResponseType.Code)
        {
            Scope = Sp2YtNetAuthSecrets.Instance.SpotifyScope
        };
        BrowserUtil.Open(request.ToUri());

        // Wait for the authorization code or error before proceeding
        var completedTask = await Task.WhenAny(authorizationCompleted.Task, Task.Delay(Sp2YtNetAuthSecrets.Instance.SpotifyAuthorizationTimeout));
        if (completedTask == authorizationCompleted.Task)
        {
            if (hasError)
            {
                Helpers.ActivateOrDeactivateConsoleOutputs(true);
                return null;
            }
            else
            {
                Helpers.ActivateOrDeactivateConsoleOutputs(true);
                Console.WriteLine(value: "Spotify authorization completed successfully.");
            }
        }
        else
        {
            Helpers.ActivateOrDeactivateConsoleOutputs(true);
            Console.WriteLine("Spotify authorization timeout. Aborting.");
            return null;
        }

        return spotifyClient;
    }

    private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        await _server.Stop();

        try
        {
            var config = SpotifyClientConfig.CreateDefault();
            AuthorizationCodeTokenResponse tokenResponse = await new OAuthClient(config).RequestToken(
                new AuthorizationCodeTokenRequest(
                    Sp2YtNetAuthSecrets.Instance.SpotifyClientId, Sp2YtNetAuthSecrets.Instance.SpotifyClientSecret, response.Code, new Uri("http://localhost:5543/callback")
                )
            );

            spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
            await SaveTokenForFurtherUse(tokenResponse);

            // Set the task completion to indicate success
            authorizationCompleted.TrySetResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Spotify authorization error: {ex.Message}");
            // Set the task completion to indicate failure
            authorizationCompleted.TrySetResult(false);
        }
    }

    private static async Task SaveTokenForFurtherUse(AuthorizationCodeTokenResponse tokenResponse)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(new
        {
            tokenResponse.AccessToken,
            tokenResponse.TokenType,
            tokenResponse.ExpiresIn,
            tokenResponse.Scope,
            tokenResponse.RefreshToken,
            tokenResponse.CreatedAt
        }, options);
        await File.WriteAllTextAsync(Sp2YtNetAuthSecrets.Instance.SpotifyAccessTokenFilePath, json);
    }

    private static async Task<AuthorizationCodeTokenResponse> ReadToken()
    {
        try
        {
            // Deserialize the JSON back to an anonymous type or dictionary
            var json = await File.ReadAllTextAsync(Sp2YtNetAuthSecrets.Instance.SpotifyAccessTokenFilePath);
            var deserializedResponse = JsonSerializer.Deserialize<AuthorizationCodeTokenResponse>(json);
            return deserializedResponse;
        }
        catch
        {
            return null;
        }
    }

    private static async Task OnErrorReceived(object sender, string error, string state)
    {
        Console.WriteLine($"Aborting Spotify authorization, error received: {error}");
        await _server.Stop();
        // Set the task completion to indicate failure
        hasError = true;
        authorizationCompleted.TrySetResult(false);
    }

    #endregion

    #region PLAYLIST OPERATIONS
    public static async Task<List<SimplePlaylist>> GetSpotifyPlaylistsAsync(this SpotifyClient spotifyClient)
    {
        var req = new PlaylistCurrentUsersRequest()
        { Limit = 50, Offset = 0 };
        var page = await spotifyClient.Playlists.CurrentUsers(req);
        var allPages = await spotifyClient.PaginateAll(page);

        return allPages.ToList();
    }

    public static async Task<SimplePlaylist> GetPlaylistAsync(this SpotifyClient spotifyClient, string playlistName)
    {
        var allPlaylists = await GetSpotifyPlaylistsAsync(spotifyClient);
        return allPlaylists.FirstOrDefault(pl => pl.Name.Equals(playlistName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static async Task<FullPlaylist> GetFullPlaylistAsync(this SpotifyClient spotifyClient, string playlistId)
    {
        return await spotifyClient.Playlists.Get(playlistId);
    }

    public static async Task<List<FullTrack>> GetFullTracksAsync(this SpotifyClient spotifyClient, string playlistId)
    {
        var fullPlaylist = await spotifyClient.GetFullPlaylistAsync(playlistId);

        return fullPlaylist.Tracks.Items.Select(playableItem => playableItem.Track as FullTrack).ToList();
    }

    public static async Task<List<string>> GetTrackQueriesAsync(this SpotifyClient spotifyClient, string playlistId)
    {
        var fullTracks = await GetFullTracksAsync(spotifyClient, playlistId);
        List<string> searchQueries = GetQueries(fullTracks);
        return searchQueries;
    }

    public static async Task<List<string>> GetTrackQueriesWithPlaylistNameAsync(this SpotifyClient spotifyClient, string playlistName)
    {
        var playlists = await GetSpotifyPlaylistsAsync(spotifyClient);
        var playlist = playlists.FirstOrDefault(p => p.Name.Equals(playlistName, StringComparison.InvariantCultureIgnoreCase));

        if (playlist is null)
        {
            return null;
        }
        return await GetTrackQueriesAsync(spotifyClient, playlist.Id);
    }

    public static List<string> GetQueries(List<FullTrack> fullTracks)
    {
        char seperator = ',';
        return fullTracks.Select(track => $"{track.Name} - {string.Join(seperator, track.Artists.Select(a => a.Name))}").ToList();
    }

    #endregion
}
