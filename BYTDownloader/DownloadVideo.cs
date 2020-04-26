using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadVideo : Backend
    {
        public DownloadVideo() : base(true) { }

        public override async Task PrepareDownload(bool IsVideo)
        {
            Console.Clear();
            Console.WriteLine("URL: ");

            Console.Title = "BYTDownloader | Loading...";

            var video = await client.Videos.GetAsync(Console.ReadLine());
            var manifest = await client.Videos.Streams.GetManifestAsync(video.Id);

            Title = SharedMethods.CheckIfAvailableName(SharedMethods.Path, SharedMethods.ENGAlphabet(video.Title), Format.Mp4);
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
