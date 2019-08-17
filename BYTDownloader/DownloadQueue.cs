using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace BYTDownloader
{
    public class DownloadQueue
    {
        private bool Run = false;

        private IProgress<double> pro2;

        private int ListMaxLength = 0;

        private int Current = 0;

        private double Previous = 0;

        private string TmpTitle = string.Empty;

        private int Files = 0;

        private string LastStr = string.Empty;

        public DownloadQueue(List<string> list)
        {
            pro2 = new Progress<double>(HandleProgress2);

            DQueue(list);
        }

        private async void DQueue(List<string> list)
        {
            Console.Clear();
            
            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string answer = Console.ReadLine();

            Console.Clear();

            int answer2AsInt = 0;
            if (int.Parse(answer) == 1)
            {
                Console.WriteLine("You have these options in which the quality of the playlist should be downloaded:\n" +
                                  "1: Best quality\n2: worst quality\n3: Set manually for each video the quality");

                Console.Write("Your answer: ");

                string answer2 = Console.ReadLine();
                answer2AsInt = int.Parse(answer2);
            }

            Console.WriteLine("Download has started!");

            ListMaxLength = list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                var id = YoutubeClient.ParseVideoId(list[i]);

                var client = new YoutubeClient();
                var converter = new YoutubeConverter(client);

                var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
                var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

                if (int.Parse(answer) == 1)
                {
                    try
                    {
                        var title1 = await client.GetVideoAsync(id);
                        string title = ENGAlphabet(title1.Title);

                        int counter = 1;

                        string x = string.Empty;
                        string y = string.Empty;

                        var videoStreamInfo = mediaStreamInfoSet.Video.OrderByDescending(s => s.VideoQuality)
                            .ThenByDescending(s => s.Framerate).First();

                        if (answer2AsInt == 2)
                        {
                            videoStreamInfo = mediaStreamInfoSet.Video.OrderByDescending(s => s.VideoQuality)
                                .ThenByDescending(s => s.Framerate).Last();
                        }
                        else if (answer2AsInt == 3)
                        {
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

                            videoStreamInfo = videoStream[int.Parse(input) - 1];
                        }

                        var mediaStreamInfos = new MediaStreamInfo[] {audioStreamInfo, videoStreamInfo};

                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                        string tit = CheckIfAvailableName(path, title, Format.Mp4);

                        try
                        {
                            await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp4",
                                "mp4", pro2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (int.Parse(answer) == 2)
                {
                    try
                    {
                        var title1 = await client.GetVideoAsync(id);
                        string title = ENGAlphabet(title1.Title);

                        var mediaStreamInfos = new MediaStreamInfo[] {audioStreamInfo};

                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                        string tit = CheckIfAvailableName(path, title, Format.Mp3);

                        try
                        {
                            await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp3",
                                "mp3", pro2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
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
            string titel = string.Empty;

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
                titel += x;
            }

            return titel;
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
                return $"{j}({num})";
            }

            return $"{str} ({num})";
        }

        private void HandleProgress2(double progress)
        {
            string x = ((int) (progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            Run = CalcIfDoubleUsed(progress);

            if (Run)
            {
                Run = false;
                Current++;

                Console.WriteLine($"{Current} / {ListMaxLength}");

                if (Current == ListMaxLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Done");
                }
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