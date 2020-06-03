using System;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadVideo : Backend
    {
        public override void PrepareDownload()
        {
            IsVideo = true;

            Console.Clear();
            Console.WriteLine("URL: ");

            Console.Title = "BYTDownloader | Loading...";

            var video = client.Videos.GetAsync(Console.ReadLine()).Result;
            var manifest = client.Videos.Streams.GetManifestAsync(video.Id).Result;

            Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), Format.mp4);
            Console.WriteLine("In which resolution do you want to download the video?\n" + "Type the Number that is displayed before the resolution / fps");

            var allqualities = manifest.GetVideoOnly().ToArray();

            for (int i = 0; i < allqualities.Length; i++)
                Console.WriteLine($"{i}: {allqualities[i].Resolution} - {allqualities[i].Framerate} - {allqualities[i].Bitrate} - {allqualities[i].Container}");

            Console.Title = "BYTDownloader";
            Console.WriteLine("Your answer: ");

            mediaStreamInfos = new IStreamInfo[]
            {
                manifest.GetAudioOnly().WithHighestBitrate(),
                manifest.GetVideoOnly().ToArray()[int.Parse(Console.ReadLine())]
            };
        }
    }
}
