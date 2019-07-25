using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace BYTDownloader
{
    class Program
    {
        private static bool Run = false;

        private static IProgress<double> pro = new Progress<double>(HandleProgress);

        private static IProgress<double> pro2 = new Progress<double>(HandleProgress2);

        private static int PlaylistMaxLength = 0;

        private static int Current = 0;

        private static double Previous = 0;

        static void Main()
        {
            Console.Title = "BYTDownloader";
            new License();
            CheckFFMPGEG();
        }

        private static void CheckFFMPGEG()
        {
            var CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (!File.Exists(CurrentDirectory + "\\ffmpeg.exe"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ffmpeg.exe not found");
                Console.WriteLine("Start downloading ffmpeg.exe");

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(new Uri("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-20190530-9c35285-win64-static.zip"), CurrentDirectory + "//FFMPEG.zip");

                    Directory.CreateDirectory(CurrentDirectory + "//temp");
                    ZipFile.ExtractToDirectory(CurrentDirectory + "//FFMPEG.zip", CurrentDirectory + "//temp");
                    File.Move(CurrentDirectory + "//temp/ffmpeg-20190530-9c35285-win64-static/bin/ffmpeg.exe", CurrentDirectory + "//ffmpeg.exe");
                }

                Console.WriteLine("ffmpeg.exe downloaded!");
                Console.ResetColor();
                Console.Clear();
            }

            if (File.Exists(CurrentDirectory + "//FFMPEG.zip"))
            {
                File.Delete(CurrentDirectory + "//FFMPEG.zip");
            }

            if (Directory.Exists(CurrentDirectory + "//temp"))
            {
                DeleteDirectory(CurrentDirectory + "//temp");
            }
            INI();
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private static void INI()
        {
            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");
            Console.WriteLine("3: Playlist");
            Console.WriteLine("-----------");
            Console.WriteLine("4: Converter");

            var x = Console.ReadLine();

            if (uint.Parse(x) == 1)
            {
                DownloadVideo();
            }
            else if (uint.Parse(x) == 2)
            {
                DownloadSong();
            }
            else if (uint.Parse(x) == 3)
            {
                Console.WriteLine("1: Specific part?");
                Console.WriteLine("2: Whole playlist");
                var y = Console.ReadLine();

                if (int.Parse(y) == 1)
                {
                    Console.WriteLine("Which part in the playlist?");
                    var z = Console.ReadLine();
                    Download_SPECIFIC_PART_Playlist(int.Parse(z));
                }
                else if (int.Parse(y) == 2)
                {
                    Download_WHOLE_Playlist();
                }
            }
            else if (uint.Parse(x) == 4)
            {
                new Converter();
            }
            Console.ReadLine();
        }

        private static async void DownloadVideo()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            var id = YoutubeClient.ParseVideoId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

            try
            {
                //var videoStreamInfo = mediaStreamInfoSet.Video.FirstOrDefault(s => s.Framerate == 60);

                var videoStreamInfo = mediaStreamInfoSet.Video
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Framerate)
                    .First();

                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + "\\video.mp4", "mp4", pro);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async void DownloadSong()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            var id = YoutubeClient.ParseVideoId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

            try
            {
                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + "\\sound.mp3", "mp3", pro);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async void Download_SPECIFIC_PART_Playlist(int y)
        {
            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string Answer = Console.ReadLine();

            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            var id = YoutubeClient.ParsePlaylistId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var playlist = await client.GetPlaylistAsync(id);

            var video = playlist.Videos.ElementAt(y - 1).Id;

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(video);

            var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

            if (int.Parse(Answer) == 1) //Video
            {
                try
                {
                    var videoStreamInfo = mediaStreamInfoSet.Video
                        .OrderByDescending(s => s.VideoQuality)
                        .ThenByDescending(s => s.Framerate)
                        .First();

                    var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + "\\video.mp4", "mp4", pro);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (int.Parse(Answer) == 2)//Sound
            {
                try
                {
                    var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + "\\sound.mp3", "mp3", pro);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static async void Download_WHOLE_Playlist()
        {
            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string Answer = Console.ReadLine();

            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            var id = YoutubeClient.ParsePlaylistId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var playlist = await client.GetPlaylistAsync(id);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!(Directory.Exists(path + "\\Playlist")))
            {
                Directory.CreateDirectory(path + "\\Playlist");
            }

            PlaylistMaxLength = playlist.Videos.Count;

            for (int i = 0; i < playlist.Videos.Count; i++)
            {
                var video = playlist.Videos.ElementAt(i).Id;

                var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(video);

                var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

                if (int.Parse(Answer) == 1) //Video
                {
                    try
                    {
                        var videoStreamInfo = mediaStreamInfoSet.Video
                            .OrderByDescending(s => s.VideoQuality)
                            .ThenByDescending(s => s.Framerate)
                            .First();

                        var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                        await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, Path.Combine(path + "\\Playlist", string.Format("video{0}.mp4", (i + 1).ToString())), "mp4", pro2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (int.Parse(Answer) == 2)//Sound
                {
                    try
                    {
                        var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };

                        await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, Path.Combine(path + "\\Playlist", string.Format("song{0}.mp4", (i + 1).ToString())), "mp4", pro2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private static void HandleProgress(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (x == "100")
            {
                Run = CalcIfDoubleUsed(progress);
            }

            if (Run)
            {
                Run = false;
                Console.ForegroundColor = ConsoleColor.Red;
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("Done");
            }
        }

        private static void HandleProgress2(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            Run = CalcIfDoubleUsed(progress);

            if (Run)
            {
                Run = false;
                Current++;

                Console.WriteLine($"{Current} / {PlaylistMaxLength}");

                if (Current == PlaylistMaxLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Done");
                }
            }
        }

        private static bool CalcIfDoubleUsed(double progress)
        {
            if (progress == 1)
            {
                if (progress == Previous)
                {
                    return false;
                }
                Previous = progress;
                return true;
            }

            Previous = progress;

            return false;
        }
    }
}
