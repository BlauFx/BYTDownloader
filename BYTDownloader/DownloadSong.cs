using System;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadSong : Backend
    {
        public DownloadSong() : base (false) { }

        public override void PrepareDownload(bool IsVideo)
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            var client = new YoutubeClient();
            var video = client.Videos.GetAsync(Console.ReadLine()).Result;
           
            var streamManifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;
            Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), Format.mp3);

            mediaStreamInfos = new IStreamInfo[] { streamManifest.GetAudioOnly().WithHighestBitrate() };
        }
    }
}
