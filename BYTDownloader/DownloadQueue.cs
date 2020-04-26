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

            Console.Clear();

            Console.WriteLine("Do you want to download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            int input = int.Parse(Console.ReadLine());
            new DownloadQueue(tmp, input == 1 ? true : (input == 2 ? false : new bool?()));
        }
    }

    internal class DownloadQueue : Backend
    {
        private int ListMaxLength = 0;

        private int Current = 0;

        private double Previous = 0;

        public DownloadQueue(List<string> list, bool? IsVideo) : base(IsVideo.Value, list) { }

        public override async Task PrepareDownload(bool IsVideo)
        {
            pro = new Progress<double>(HandleProgress2);
            Console.Clear();

            int? answer2 = null;

            if (IsVideo)
            {
                Console.WriteLine("You have these options in which the quality of the playlist should be downloaded:\n" +
                                  "1: Best quality\n2: worst quality\n3: Set manually for each video the quality");

                Console.Write("Your answer: ");
                answer2 = int.Parse(Console.ReadLine());
            }

            ListMaxLength = list.Count;

            for (int i = 0; i < ListMaxLength; i++)
            {
                var client = new YoutubeClient();
                var video = await client.Videos.GetAsync(list[i]);

                Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);
                var manifest = await client.Videos.Streams.GetManifestAsync(video.Id);
               
                if (IsVideo)
                {
                    mediaStreamInfos = new IStreamInfo[]
                    {
                        manifest.GetAudioOnly().WithHighestBitrate(),
                        manifest.GetVideoOnly().WithHighestVideoQuality()
                    };

                    switch (answer2.Value)
                    {
                        case 1:
                            break;
                        case 2:
                            mediaStreamInfos[1] = manifest.GetVideoOnly().OrderByDescending(o => o.VideoQuality).ThenByDescending(o => o.Framerate).ThenByDescending(o => o.Bitrate).Last();
                            break;
                        case 3:
                            {
                                var allqualities = manifest.GetVideoOnly().ToArray();

                                for (int j = 0; j < allqualities.Length; j++)
                                    Console.WriteLine($"{j}: {allqualities[j].Resolution} - {allqualities[j].Framerate} - {allqualities[j].Bitrate} - {allqualities[j].Container}");

                                Console.Write("Your answer: ");
                                mediaStreamInfos[1] = manifest.GetVideoOnly().ToArray()[int.Parse(Console.ReadLine())];
                                break;
                            }

                        default:
                            throw new Exception();
                    }

                    Console.Title = "BYTDownloader";
                    format = Format.Mp4;
                }
                else if (!IsVideo)
                {
                    format = Format.Mp3;
                    mediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                }

                streamInfosList.Add(mediaStreamInfos);
            }
        }

        public override async Task DownloadFile(YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, string title, Format format)
        {
            Console.WriteLine("Download has started!");
            YoutubeConverter converter = new YoutubeConverter(client);

            for (int i = 0; i < ListMaxLength; i++)
            {
                title = SharedMethods.CheckIfAvailableName(path, SharedMethods.ENGAlphabet(title), format);
                await converter.DownloadAndProcessMediaStreamsAsync(streamInfosList[i], $"{path}\\{title}.{format.ToString().ToLower()}", format.ToString().ToLower(), pro);
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
