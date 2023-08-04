# Sp2YtNet
This is a lightweight command line tool for one-way transfer of Spotify playlists to Youtube Music playlists, written in C# .NET, using below libraries:

- [JohnnyCrazy / SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET)
- [madeyoga / YoutubeSearchApi.Net](https://github.com/madeyoga/YoutubeSearchApi.Net)
- [Google.Apis.YouTube.v3](https://www.nuget.org/packages/Google.Apis.youtube.v3/)
- [SpotifyAPI.Web](https://www.nuget.org/packages/SpotifyAPI.Web)

## What it does
- List Spotify playlists of the user
- List YouTube playlists of the user
- List all the tracks in a Spotify playlist of the user
- List all the tracks in a Youtube playlist of the user
- Transfer all tracks in a Spotify playlist into another Youtube playlist

## Requirements
- .NET 7.0 Runtime

## Basic Setup

- Edit the example `authSecrets.json` file and see the required inputs

### Setup Spotify API
- Go to [Developer Dashboard](https://developer.spotify.com/dashboard) of Spotify and add an application for Web API access. Make sure the `SpotifyRedirectUri` in `authSecrets.json` is the same for your applications Redirect URI in the Spotify's Dashboard.
- Enter your Client ID and Client Secret to the `SpotifyClientId` and `SpotifyClientSecret` fields, in `authSecrets.json`.

### Setup YouTube API
- Go to [Google Cloud Console](https://console.cloud.google.com/apis/api/youtube.googleapis.com/) and enable YouTube API
- Register your app from **Enabled APIs & services** >> **Credentials** >> **+ CREATE CREDENTIALS** >> **OAuth client ID** >> **Desktop app**
- Download OAuth client definition file in JSON format, by clicking Download OAuth client button in Credentials page
- Move/copy the downloaded OAuth client JSON file to the same folder of the executable. Enter the path of this file in the `YtClientSecretsPath` field in `authSecrets.json`.
- Move/copy the downloaded OAuth client JSON file to the folder of the executable. Put path of this file into `YtClientSecretsPath` field, in `authSecrets.json`.
- Enter your registered application name into `YtAppName` field, in `authSecrets.json`.
- Enter your YouTube username into `YtUsername` field, in `authSecrets.json`. Please note that there can be more than one user associated with the same e-mail address. Choose the related one.

## Usage:
        Sp2Yt <command>

## Commands:
        -list-spotify-playlists
        -list-youtube-playlists
        -list-spotify-tracks <Spotify playlist name>
        -list-youtube-tracks <Youtube playlist name>
        -transfer-to-youtube <Spotify playlist name> <Youtube playlist name>
