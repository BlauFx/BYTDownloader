using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class Queue
    {
        private readonly List<string> tmp = new List<string>();

        public Queue()
        {
            Console.WriteLine("Add links to the queue!\nIf you're done than type: Done");

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

            Console.WriteLine("Do you want to download it as a video or as a song?\n" +
                "1: Video\n" +
                "2: Song");

            int input = int.Parse(Console.ReadLine());
            HandleRequest(input == 1 || input == 2 ? false : throw new Exception());
        }

        private void HandleRequest(bool IsVideo)
        {
            List<IStreamInfo[]> StreamInfosList = new List<IStreamInfo[]>();
            List<string> titles = new List<string>();

            YoutubeClient client = new YoutubeClient();
            Format format = IsVideo ? Format.mp4 : Format.mp3;

            if (IsVideo)
            {
                Console.Write("You can choose between these options:\n" +
                    "1: Best quality\n" +
                    "2: worst quality\n" +
                    "3: Set manually for each video the quality\n" +
                    "Your answer: ");

                int answer = int.Parse(Console.ReadLine());

                for (int i = 0; i < tmp.Count; i++)
                {
                    var video = client.Videos.GetAsync(tmp[i]).Result;
                    var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

                    var mediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate(), manifest.GetVideoOnly().WithHighestVideoQuality() };
                    var title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);

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

                    StreamInfosList.Add(mediaStreamInfos);
                    titles.Add(title);
                }
            }
            else
            {
                for (int i = 0; i < tmp.Count; i++)
                {
                    var video = client.Videos.GetAsync(tmp[i]).Result;
                    var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

                    var mediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };
                    var title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);

                    StreamInfosList.Add(mediaStreamInfos);
                    titles.Add(title);
                }
            }

            for (int i = 0; i < StreamInfosList.Count; i++)
            {
                new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(StreamInfosList[i],
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{titles[i]}.{format.ToString().ToLower()}", format.ToString().ToLower(),
                    new Progress<double>((p) => SharedMethods.HandleProgress(p))).GetAwaiter().GetResult();
            }
        }
    }
}
