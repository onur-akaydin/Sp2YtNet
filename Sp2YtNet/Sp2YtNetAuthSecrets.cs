using Google.Apis.YouTube.v3;
using SpotifyAPI.Web;
using System.Text.Json;

namespace Sp2YtNet;
public class Sp2YtNetAuthSecrets
{
    private const string SECRETS_FILE_PATH = "authSecrets.json";

    public static Sp2YtNetAuthSecrets Instance;
    public static async Task ReadSettingsIntoStaticInstance()
    {
        try
        {
            var json = await File.ReadAllTextAsync(SECRETS_FILE_PATH);
            var secrets = JsonSerializer.Deserialize<Sp2YtNetAuthSecrets>(json);
            Instance = secrets;
        }
        catch
        {
            Console.WriteLine($"Could not read {SECRETS_FILE_PATH} properly");
            Environment.Exit(1);
        }
    }

    public static async Task SaveSettings(Sp2YtNetAuthSecrets settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        await File.WriteAllTextAsync(SECRETS_FILE_PATH, json);
    }

    // Spotify credentials
    public string SpotifyClientId { get; set; }
    public string SpotifyClientSecret { get; set; }
    public int SpotifyAuthorizationTimeout { get; set; } = 30000;
    public string SpotifyRedirectUri { get; set; } = "http://localhost:5543/callback";

    public List<string> SpotifyScope { get; set; } = new List<string> { Scopes.UserReadEmail, Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative };

    public string SpotifyAccessTokenFilePath { get; set; } = "SpAccessToken.txt";

    // Youtube Credentials

    // The path to the client_secrets.json file
    public string YtClientSecretsPath { get; set; } = "desktop_client_secret.json";
    public string YtUsername { get; set; }
    public string YtAppName { get; set; } = "MyAppName";
    public string[] YtScope { get; set; } = new[] {
                                            YouTubeService.Scope.Youtube,
                                            YouTubeService.Scope.YoutubeForceSsl,
                                            YouTubeService.Scope.YoutubeUpload,
                                            YouTubeService.Scope.Youtubepartner,
                                            YouTubeService.Scope.YoutubepartnerChannelAudit
                                            };

}
