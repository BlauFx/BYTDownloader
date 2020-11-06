using System;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadSong
    {
        public DownloadSong()
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            YoutubeClient client = new YoutubeClient();
            var video = client.Videos.GetAsync( Console.ReadLine()).Result;

            Console.Title = "BYTDownloader | Loading...";
            var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

            Console.Title = "BYTDownloader";
            var mediaStreamInfos = new[] { manifest.GetAudioOnly().WithHighestBitrate() };

            string format = Format.mp3.ToString().ToLower();
            var title = SharedMethods.CheckIfAvailableName(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), video.Title, format);

            client.Videos.DownloadAsync(mediaStreamInfos, new ConversionRequestBuilder($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{title}.{format}").SetPreset(ConversionPreset.VerySlow).Build(), new Progress<double>((p) => SharedMethods.HandleProgress(p))).GetAwaiter().GetResult();
        }
    }
}
