using System;
using System.Collections.Generic;
using System.Linq;
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
            new DownloadQueue(tmp) { IsVideo = (bool)(input == 1 ? true : (input == 2 ? false : new bool?())) };
        }
    }

    internal class DownloadQueue : Backend
    {
        public DownloadQueue(List<string> list) : base(list) { }

        public override void PrepareDownload()
        {
            Console.Clear();
            int answer = 0;

            if (IsVideo)
            {
                Console.WriteLine("You have these options in which the quality of the playlist should be downloaded:\n" +
                                  "1: Best quality\n2: worst quality\n3: Set manually for each video the quality");

                Console.Write("Your answer: ");
                answer = int.Parse(Console.ReadLine());
            }

            for (int i = 0; i < SharedMethods.ListMaxLength; i++)
            {
                var video = client.Videos.GetAsync(list[i]).Result;

                Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);
                var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

                if (IsVideo)
                {
                    mediaStreamInfos = new IStreamInfo[]
                    {
                        manifest.GetAudioOnly().WithHighestBitrate(),
                        manifest.GetVideoOnly().WithHighestVideoQuality()
                    };

                    switch (answer)
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
                    format = Format.mp4;
                }
                else if (!IsVideo)
                {
                    format = Format.mp3;
                    mediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                }

                StreamInfosList.Add(mediaStreamInfos);
            }
        }
    }
}
