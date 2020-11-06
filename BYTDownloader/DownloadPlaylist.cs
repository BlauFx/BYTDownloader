using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadPlaylist
    {
        private readonly YoutubeClient client = new YoutubeClient();

        private IStreamInfo[] MediaStreamInfos;
        private readonly IReadOnlyList<Video> PlaylistVideos;

        private readonly Format Frmt;

        public DownloadPlaylist()
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            var url = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Do you want the download it as a video or as a song?\n" +
                "1: Video\n" +
                "2: Song");

            Frmt = Console.ReadLine() == "1" ? Format.mp4 : Format.mp3;
            Console.Clear();

            Console.Title = "BYTDownloader | Loading...";
            PlaylistVideos = client.Playlists.GetVideosAsync(client.Playlists.GetAsync(url).GetAwaiter().GetResult().Id).BufferAsync().GetAwaiter().GetResult();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Playlist"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Playlist");

            DownloadFile();
        }

        public async void DownloadFile()
        {
            int playlistLength = PlaylistVideos.Count();
            int? answer = null;

            string Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Playlist";

            if (Frmt == Format.mp4)
            {
                Console.Write("You can choose between these options:\n" +
                    "1: Best quality\n" +
                    "2: worst quality\n" +
                    "3: Set manually for each video the quality\n" +
                    "Your answer: ");

                answer = int.Parse(Console.ReadLine());
            }

            Console.Title = "BYTDownloader";

            for (int i = 0; i < playlistLength; i++)
            {
                try
                {
                    var video = await client.Videos.GetAsync(PlaylistVideos.ElementAt(i).Url);
                    var manifest = await client.Videos.Streams.GetManifestAsync(video.Id);

                    if (Frmt == Format.mp4)
                    {
                        MediaStreamInfos = new IStreamInfo[]
                        {
                            manifest.GetAudioOnly().WithHighestBitrate(),
                            manifest.GetVideoOnly().WithHighestVideoQuality()
                        };

                        switch (answer.Value)
                        {
                            case 1:
                                break;
                            case 2:
                                MediaStreamInfos[1] = manifest.GetVideoOnly().OrderByDescending(o => o.VideoQuality).ThenByDescending(o => o.Framerate).ThenByDescending(o => o.Bitrate).Last();
                                break;
                            case 3:
                                {
                                    var allqualities = manifest.GetVideoOnly().ToArray();

                                    for (int j = 0; j < allqualities.Length; j++)
                                        Console.WriteLine($"{j}: {allqualities[j].Resolution} - {allqualities[j].Framerate} - {allqualities[j].Bitrate} - {allqualities[j].Container}");

                                    Console.Write("Your answer: ");
                                    MediaStreamInfos[1] = manifest.GetVideoOnly().ToArray()[int.Parse(Console.ReadLine())];
                                    break;
                                }

                            default:
                                throw new Exception();
                        }
                    }
                    else if (Frmt == Format.mp3) //Sound
                    {
                        MediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                    }

                    string format = Frmt.ToString().ToLower();
                    var title = SharedMethods.CheckIfAvailableName(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Playlist", PlaylistVideos[i].Title, format);

                    client.Videos.DownloadAsync(MediaStreamInfos, new ConversionRequestBuilder($"{Path}\\{title}.{format}")
                        .SetFormat(format).SetPreset(ConversionPreset.VerySlow).Build(), new Progress<double>((p) =>  SharedMethods.HandleProgress(p, true))).GetAwaiter().GetResult();
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine($"{i + 1} / {playlistLength} has been skipped => ({PlaylistVideos[i].Title})");
                    Console.ResetColor();

                    continue;
                }

                Console.WriteLine($"{i + 1} / {playlistLength}");
            }

            Console.ForegroundColor = ConsoleColor.Green;

            Thread.Sleep(1000);
            Console.WriteLine("Done");
        }
    }
}
