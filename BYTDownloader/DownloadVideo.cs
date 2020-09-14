using System;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadVideo
    {
        public DownloadVideo()
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            YoutubeClient client = new YoutubeClient();

            var video = client.Videos.GetAsync(Console.ReadLine()).Result;
            Console.Title = "BYTDownloader | Loading...";

            var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;
            var allqualities = manifest.GetVideoOnly().ToArray();

            for (int i = 0; i < allqualities.Length; i++)
                Console.WriteLine($"{i}: {allqualities[i].Resolution} - {allqualities[i].Framerate} - {allqualities[i].Bitrate} - {allqualities[i].Container}");

            Console.Title = "BYTDownloader";
            Console.Write("Your answer: ");

            var mediaStreamInfos = new IStreamInfo[]
            {
                manifest.GetAudioOnly().WithHighestBitrate(),
                manifest.GetVideoOnly().ToArray()[int.Parse(Console.ReadLine()!)]
            };

            Format format = Format.mp4;
            var title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);

            new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(
                mediaStreamInfos,
                $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{title}.{format.ToString().ToLower()}",
                format.ToString().ToLower(),
                new Progress<double>((p) => SharedMethods.HandleProgress(p))).GetAwaiter().GetResult();
        }
    }
}
