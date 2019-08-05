using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace BYTDownloader
{
    public class DownloadVideo
    {
        private bool Run = false;

        private IProgress<double> pro;

        private double Previous = 0;

        private int Files = 0;

        private string TmpTitle = string.Empty;

        private string LastStr = string.Empty;

        public DownloadVideo()
        {
            pro = new Progress<double>(HandleProgress);

            DVideo();
        }

        private async void DVideo()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            Console.Title = "BYTDownloader | Loading...";

            Console.WriteLine("In which resolution do you want to download the video?");
            Console.WriteLine("Type the Number that is displayed before the resolution / fps");

            var id = YoutubeClient.ParseVideoId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            try
            {
                MediaStreamInfoSet mediaStreamInfoSet = client.GetVideoMediaStreamInfosAsync(id).Result;

                AudioStreamInfo audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

                var title1 = client.GetVideoAsync(id).Result;
                string title = ENGAlphabet(title1.Title);

                int counter = 1;

                string x = string.Empty;
                string y = string.Empty;

                VideoStreamInfo[] videoStream = new VideoStreamInfo[mediaStreamInfoSet.GetAll().Count()];

                foreach (var info in mediaStreamInfoSet.Video.OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Framerate))
                {
                    x = QualityCut(info.VideoQuality.ToString());
                    y = info.Framerate.ToString();
                    string z = x + "p" + y;

                    if (LastStr != z)
                    {
                        LastStr = z;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(counter.ToString() + ": ");
                        Console.ResetColor();
                        Console.WriteLine(z);

                        counter++;
                        videoStream[counter - 2] = info;
                    }
                }

                Console.Write("Your answer: ");
                var input = Console.ReadLine();

                var videoStreamInfo = videoStream[int.Parse(input) - 1];

                Console.Title = "BYTDownloader";

                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string tit = CheckIfAvailableName(path, title, Format.Mp4);

                try
                {
                    Console.WriteLine("Download has started!");
                    await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp4", "mp4", pro);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Console.ReadLine();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private string QualityCut(string str)
        {
            List<string> tmp = new List<string>();

            string y = string.Empty;
            foreach (var x in str)
            {
                bool isNum = int.TryParse(x.ToString(), out int n);
                if (isNum)
                {
                    tmp.Add(x.ToString());
                }
            }

            for (int i = 0;
                i < tmp.Count;
                i++)
            {
                y += tmp[i];
            }

            return y;
        }

        private string ENGAlphabet(string tit)
        {
            char[] list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ()$.-".ToCharArray();
            List<string> tmp = new List<string>();

            string Titel = string.Empty;
            foreach (var x in tit)
            {
                bool result = x.ToString().Any(x => char.IsLetterOrDigit(x));
                bool result2 = x.ToString().Any(x => char.IsWhiteSpace(x));
                bool result3 = list.Any(s => s.ToString().Contains(x));

                if (result || result2 || result3)
                {
                    tmp.Add(x.ToString());
                }
                else
                {
                    tmp.Add("#");
                }
            }

            foreach (var x in tmp)
            {
                Titel += x;
            }

            return Titel;
        }

        private string CheckIfAvailableName(string path, string name, Format format = Format.Mp3)
        {
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                CheckIfAvailableNameVoid(path, name, format);
                return TmpTitle;
            }

            return name;
        }

        private void CheckIfAvailableNameVoid(string path, string name, Format format = Format.Mp3)
        {
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                Files++;
                CheckIfAvailableNameVoid(path, FileCutter(name, Files), format);
            }
            else
            {
                Files = 0;
                TmpTitle = name;
            }
        }

        private string FileCutter(string str, int num)
        {
            if (str.Contains("("))
            {
                int i = str.IndexOf("(", StringComparison.Ordinal);
                string j = str.Substring(0, i);
                return $"{j} ({num})";
            }

            return $"{str} ({num})";
        }

        private void HandleProgress(double progress)
        {
            string x = ((int) (progress * 100)).ToString();
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

        private bool CalcIfDoubleUsed(double progress)
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