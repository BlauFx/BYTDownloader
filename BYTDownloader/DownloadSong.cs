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

            var video = client.Videos.GetAsync(Console.ReadLine()).Result;
            Console.Title = "BYTDownloader | Loading...";

            var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;
            Console.Title = "BYTDownloader";

            var mediaStreamInfos = new IStreamInfo[] { manifest.GetAudioOnly().WithHighestBitrate() };

            Format format = Format.mp3;
            var title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), format);

            new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(
                mediaStreamInfos,
                $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{title}.{format.ToString().ToLower()}",
                format.ToString().ToLower(),
                new Progress<double>((p) => SharedMethods.HandleProgress(p))).GetAwaiter().GetResult();
        }
    }
}
