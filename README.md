# Sp2YtNet
This is a command line tool for one-way transfer of Spotify playlists to Youtube Music playlists, written in C# .NET, using below libraries:

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

## Basic setup: 
TODO: This section will be updated later.

## Usage:
        Sp2Yt <command>

## Commands:
        list-spotify-playlists
        list-youtube-playlists
        list-spotify-tracks <Spotify playlist name>
        list-youtube-tracks <Youtube playlist name>
        transfer-to-youtube <Spotify playlist name> <Youtube playlist name>
