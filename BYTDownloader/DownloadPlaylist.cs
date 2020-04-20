using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadPlaylist
    {

        private IProgress<double> pro;

        private IProgress<double> pro2;

        public DownloadPlaylist(bool specificPart)
        {
            pro = new Progress<double>(SharedMethods.HandleProgress);
            pro2 = new Progress<double>(SharedMethods.HandleProgress2);

            DPlaylist(specificPart);
        }

        private void DPlaylist(bool specificPart)
        {
            if (specificPart)
            {
                DPlaylist_SPECIFIC_PART_Playlist();
            }
            else
            {
                DPlaylist_WHOLE_Playlist();
            }
        }

        public async void DPlaylist_SPECIFIC_PART_Playlist()
        {
            Console.Clear();

            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            Console.WriteLine("Which part in the playlist?");
            int part = int.Parse(Console.ReadLine());

            Console.Clear();

            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string answer = Console.ReadLine();

            Console.Clear();

            Console.Title = "BYTDownloader | Loading...";

            var client = new YoutubeClient();
            var playlist = await client.Playlists.GetAsync(url);

            IReadOnlyList<YoutubeExplode.Videos.Video> playlistVideos = await client
            .Playlists
            .GetVideosAsync(playlist.Id)
            .BufferAsync();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (!Directory.Exists(path + "\\Playlist"))
                Directory.CreateDirectory(path + "\\Playlist");

            var video = await client.Videos.GetAsync(playlistVideos.ElementAt(part).Url);
            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

            string title = SharedMethods.ENGAlphabet(playlistVideos[part].Title);

            if (int.Parse(answer) == 1) //Video
            {
                Console.WriteLine("In which resolution do you want to download the video?");
                Console.WriteLine("Type the Number that is displayed before the resolution / fps");

                bool Mp4Mode = false;

                var allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "webm").GetAllVideoQualities().ToArray();

                if (allqualities.Length == 0)
                {
                    Mp4Mode = true;
                    allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "mp4").GetAllVideoQualities().ToArray();
                }

                for (int i = 0; i < allqualities.Length; i++)
                    Console.WriteLine($"{i}: {allqualities[i]}");

                Console.Title = "BYTDownloader";

                Console.Write("Your answer: ");
                int input = int.Parse(Console.ReadLine());

                IStreamInfo[] mediaStreamInfos = new IStreamInfo[]
                {
                    streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(),
                    streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray()[input]
                };

                string tit = SharedMethods.CheckIfAvailableName(path, title, Format.Mp4);

                Console.WriteLine("Download has started!");
                await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, $"{path}\\Playlist\\{tit}.mp4", "mp4", pro);
            }
            else if (int.Parse(answer) == 2) //Sound
            {
                try
                {
                    var mediaStreamInfos = new IStreamInfo[]
                    {
                        streamManifest.GetAudioOnly().WithHighestBitrate(),
                    };

                    string tit = SharedMethods.CheckIfAvailableName(path + "\\Playlist", title, Format.Mp3);

                    await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, $"{path}\\Playlist\\{tit}.mp3", "mp3", pro2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async void DPlaylist_WHOLE_Playlist()
        {
            Console.Clear();

            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string answer = Console.ReadLine();

            Console.Clear();

            var client = new YoutubeClient();
            var playlist = await client.Playlists.GetAsync(url);

            IReadOnlyList<YoutubeExplode.Videos.Video> playlistVideos = await client
            .Playlists
            .GetVideosAsync(playlist.Id)
            .BufferAsync();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (!Directory.Exists(path + "\\Playlist")) 
                Directory.CreateDirectory(path + "\\Playlist");

            int answer2AsInt = 0;

            if (int.Parse(answer) == 1)
            {
                Console.WriteLine("You have these options in which the quality of the playlist should be downloaded:\n" +
                                  "1: Best quality\n2: worst quality\n3: Set manually for each video the quality");

                Console.Write("Your answer: ");

                string answer2 = Console.ReadLine();
                answer2AsInt = int.Parse(answer2);
            }

            for (int i = 0; i < playlistVideos.Count(); i++)
            {
                var video = await client.Videos.GetAsync(playlistVideos.ElementAt(i).Url);
                var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

                string title = SharedMethods.ENGAlphabet(playlistVideos[i].Title);

                if (int.Parse(answer) == 1) //Video
                {
                    try
                    {
                        bool Mp4Mode = false;
                        var allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "webm").GetAllVideoQualities().ToArray();

                        if (allqualities.Length == 0)
                        {
                            Mp4Mode = true;
                            allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "mp4").GetAllVideoQualities().ToArray();
                        }

                        IStreamInfo[] mediaStreamInfos = new IStreamInfo[]
                        {
                            streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(),
                            streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray().First()
                        };

                        if (answer2AsInt == 1)
                            mediaStreamInfos[1] = streamManifest.GetVideoOnly().OrderByDescending(s => s.VideoQuality).ThenByDescending(s => s.Framerate).First();
                        else if (answer2AsInt == 2)
                            mediaStreamInfos[1] = streamManifest.GetVideoOnly().OrderByDescending(s => s.VideoQuality).ThenByDescending(s => s.Framerate).Last();
                        else if (answer2AsInt == 3)
                        {
                            for (int j = 0; i < allqualities.Length; j++)
                                Console.WriteLine($"{j}: {allqualities[j]}");

                            Console.Write("Your answer: ");

                            mediaStreamInfos = new IStreamInfo[]
                            {
                                streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(),
                                streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray()[int.Parse(Console.ReadLine())]
                            };
                        }

                        Console.Title = "BYTDownloader";

                        string tit = SharedMethods.CheckIfAvailableName(path + "\\Playlist", title, Format.Mp4);

                        Console.WriteLine("Download has started!");
                        await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, $"{path}\\Playlist\\{tit}.mp4", "mp4", pro2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (int.Parse(answer) == 2) //Sound
                {
                    try
                    {
                        var mediaStreamInfos = new IStreamInfo[]
                        {
                            streamManifest.GetAudioOnly().WithHighestBitrate(),
                        };

                        string tit = SharedMethods.CheckIfAvailableName(path + "\\Playlist", title, Format.Mp3);

                        await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, $"{path}\\Playlist\\{tit}.mp3", "mp3", pro2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
