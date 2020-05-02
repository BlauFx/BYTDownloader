using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadSong : Backend
    {
        public DownloadSong() : base (false) { }

        public override async Task PrepareDownload(bool IsVideo)
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            var client = new YoutubeClient();
            var video = await client.Videos.GetAsync(Console.ReadLine());

            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);
            Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), Format.Mp3);

            mediaStreamInfos = new IStreamInfo[] { streamManifest.GetAudioOnly().WithHighestBitrate() };
        }
    }
}
