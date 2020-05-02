using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadPlaylist : BackendPlaylist
    {
        public override async Task PrepareDownload(bool WholePlaylist)
        {
            if (!WholePlaylist)
            {
                var manifest = await client.Videos.Streams.GetManifestAsync(client.Videos.GetAsync(PlaylistVideos.ElementAt(Part).Url).GetAwaiter().GetResult().Id);

                if (Answer == 1) //Video
                {
                    Console.WriteLine("In which resolution do you want to download the video?");
                    Console.WriteLine("Type the Number that is displayed before the resolution / fps");

                    VideoQuality[] allqualities = manifest.GetVideoOnly().Where(x => x.Container.Name == "webm").GetAllVideoQualities().ToArray()
                        ?? manifest.GetVideoOnly().Where(x => x.Container.Name == "mp4").GetAllVideoQualities().ToArray();

                    for (int i = 0; i < allqualities.Length; i++)
                        Console.WriteLine($"{i}: {allqualities[i]}");

                    Console.Title = "BYTDownloader";
                    Console.Write("Your answer: ");

                    MediaStreamInfos = new IStreamInfo[]
                    {
                        manifest.GetAudioOnly().WithHighestBitrate(),
                        manifest.GetVideoOnly().Where(x => x.Container.Name == "webm" ? x.Container.Name == "webm" : x.Container.Name == "mp4").ToArray()[int.Parse(Console.ReadLine())]
                    };
                }
                else if (Answer == 2) //Sound
                {
                    Format = Format.Mp3;
                    MediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                }

                Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path + "\\Playlist", SharedMethods.ENGAlphabet(PlaylistVideos[Part].Title), Format);
            }
        }

        public override async Task DownloadFile(bool WholePlaylist, YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, Format format)
        {
            if (!WholePlaylist)
            {
                await base.DownloadFile(WholePlaylist, client, readOnlyList, path, format);
                return;
            }

            int? answer2 = null;

            if (Answer == 1)
            {
                Console.WriteLine("You have these options in which the quality of the playlist should be downloaded:\n" +
                                  "1: Best quality\n2: worst quality\n3: Set manually for each video the quality");

                Console.Write("Your answer: ");

                answer2 = int.Parse(Console.ReadLine());
            }

            int playlistLength = PlaylistVideos.Count();
            Pro2 = new Progress<double>((p) => SharedMethods.HandleProgress(p, true, playlistLength));

            YoutubeConverter converter = new YoutubeConverter(client);
            Console.WriteLine("Download has started!");

            for (int i = 0; i < playlistLength; i++)
            {
                try
                {
                    var video = await client.Videos.GetAsync(PlaylistVideos.ElementAt(i).Url);
                    var manifest = await client.Videos.Streams.GetManifestAsync(video.Id);

                    if (Answer == 1) //Video
                    {
                        MediaStreamInfos = new IStreamInfo[]
                        {
                            manifest.GetAudioOnly().WithHighestBitrate(),
                            manifest.GetVideoOnly().WithHighestVideoQuality()
                        };

                        switch (answer2.Value)
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

                        Console.Title = "BYTDownloader";
                        Format = Format.Mp4;
                    }
                    else if (Answer == 2) //Sound
                    {
                        Format = Format.Mp3;
                        MediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                    }

                    Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path + "\\Playlist", SharedMethods.ENGAlphabet(PlaylistVideos[i].Title), Format);
                    await converter.DownloadAndProcessMediaStreamsAsync(MediaStreamInfos, $"{path}\\Playlist\\{Title}.{format.ToString().ToLower()}", format.ToString().ToLower(), Pro2);
                }
                catch
                {
                    Console.Title = "BYTDownloader";
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine($"{CurrentPos} / {playlistLength} has been skipped => ({PlaylistVideos[i].Title})");
                    Console.ResetColor();

                    CurrentPos++;
                    continue;
                }

                Console.WriteLine($"{CurrentPos} / {playlistLength}");
                CurrentPos++;
            }

            Console.ForegroundColor = ConsoleColor.Red;

            Thread.Sleep(1000);
            Console.WriteLine("Done");
        }
    }
}
