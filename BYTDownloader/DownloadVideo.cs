using System;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadVideo
    {
        private readonly IProgress<double> pro;

        public DownloadVideo()
        {
            pro = new Progress<double>(SharedMethods.HandleProgress);

            DVideo();
            Console.ReadLine();
        }

        private async void DVideo()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            Console.Title = "BYTDownloader | Loading...";
            Console.WriteLine("In which resolution do you want to download the video?\n" + "Type the Number that is displayed before the resolution / fps");

            var client = new YoutubeClient();

            var video = await client.Videos.GetAsync(url);
            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);

            try
            {
                bool Mp4Mode = false;

                var allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "webm").GetAllVideoQualities().ToArray();

                if (allqualities.Length == 0)
                {
                    Mp4Mode = true;
                    allqualities = streamManifest.GetVideoOnly().Where(x => x.Container.Name == "mp4").GetAllVideoQualities().ToArray();
                }

                for (int i = 0; i < allqualities.Length; i++)
                    Console.WriteLine($"{i}: {allqualities[i]}");

                Console.Title = "BYTDownloader";

                Console.Write("Your answer: ");
                int input = int.Parse(Console.ReadLine());

                IStreamInfo[] mediaStreamInfos = new IStreamInfo[]
                {
                    streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(),
                    streamManifest.GetVideoOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).ToArray()[input]
                };

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string tit = SharedMethods.CheckIfAvailableName(path, SharedMethods.ENGAlphabet(video.Title), Format.Mp4);

                Console.WriteLine("Download has started!");
                await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, $"{path}\\{tit}.mp4", "mp4", pro);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
