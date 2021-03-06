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

            var id = Console.ReadLine();

            YoutubeClient client = new YoutubeClient();
            var video = client.Videos.GetAsync(id).Result;

            Console.Title = "BYTDownloader | Loading...";
            var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

            var allQualities = manifest.GetVideoOnly().ToArray();

            for (int i = 0; i < allQualities.Length; i++)
                Console.WriteLine($"{i}: {allQualities[i].Resolution} - {allQualities[i].Framerate} - {allQualities[i].Bitrate} - {allQualities[i].Container}");

            Console.Title = "BYTDownloader";
            Console.Write("Your answer: ");

            var mediaStreamInfos = new[] { manifest.GetAudioOnly().WithHighestBitrate(), manifest.GetVideoOnly().ToArray()[int.Parse(Console.ReadLine()!)] };

            string format = Format.mp4.ToString().ToLower();
            var title = SharedMethods.CheckIfAvailableName(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), video.Title, format);

            client.Videos.DownloadAsync(mediaStreamInfos, new ConversionRequestBuilder($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{title}.{format}")
                .SetFormat(format).SetPreset(ConversionPreset.VerySlow).Build(), new Progress<double>((p) =>  SharedMethods.HandleProgress(p))).GetAwaiter().GetResult();
        }
    }
}
