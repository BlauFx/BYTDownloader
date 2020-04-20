using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class Queue
    {
        public Queue()
        {
            Console.WriteLine("Add links to the queue!");
            Console.WriteLine("If you're done than type: Done");

            List<string> tmp = new List<string>();

            while (true)
            {
                string y = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(y))
                {
                    if (y.Contains("Done"))
                        break;
                    else
                        tmp.Add(y);
                }
            }

            new DownloadQueue(tmp);
        }
    }

    internal class DownloadQueue
    {
        private IProgress<double> pro2;

        private int ListMaxLength = 0;

        private int Current = 0;

        private double Previous = 0;

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
                answer2AsInt = int.Parse(Console.ReadLine());
            }

            ListMaxLength = list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                var client = new YoutubeClient();

                var video = await client.Videos.GetAsync(list[i]);
                var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

                bool Mp4Mode = false;

                var allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "webm").GetAllVideoQualities().ToArray();

                if (allqualities.Length == 0)
                {
                    Mp4Mode = true;
                    allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "mp4").GetAllVideoQualities().ToArray();
                }

                if (int.Parse(answer) == 1)
                {
                    try
                    {
                        string title = SharedMethods.ENGAlphabet(video.Title);

                        IStreamInfo[] mediaStreamInfos = new IStreamInfo[]
                        {
                            streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(),
                            streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray().OrderByDescending(c => c.Resolution).ThenByDescending(s => s.Framerate).First()
                        };

                        if (answer2AsInt == 2)
                            mediaStreamInfos[1] = streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray().OrderByDescending(c => c.Resolution).ThenByDescending(s => s.Framerate).Last();
                        else if (answer2AsInt == 3)
                        {
                            for (int j = 0; j < allqualities.Length; j++)
                                Console.WriteLine($"{j}: {allqualities[j]}");

                            Console.Write("Your answer: ");
                            mediaStreamInfos[1] = streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray()[int.Parse(Console.ReadLine())];
                        }

                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string tit = SharedMethods.CheckIfAvailableName(path, title, Format.Mp4);

                        await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp4", "mp4", pro2);
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
                        Console.Title = "BYTDownloader";

                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string tit = SharedMethods.CheckIfAvailableName(path, SharedMethods.ENGAlphabet(video.Title), Format.Mp3);

                        await client.Videos.Streams.DownloadAsync(streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(), $"{path}\\{tit}.mp3", pro2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private void HandleProgress2(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (CalcIfDoubleUsed(progress))
            {
                Current++;

                Console.WriteLine($"{Current} / {ListMaxLength}");

                if (Current == ListMaxLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Thread.Sleep(1000);
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
