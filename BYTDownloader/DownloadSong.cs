using System;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public class DownloadSong
    {
        private readonly IProgress<double> pro;

        public DownloadSong()
        {
            pro = new Progress<double>(HandleProgress);

            DSong();
        }

        private async void DSong()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            Console.WriteLine("Download has started!");

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

                Console.Title = "BYTDownloader";

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string tit = SharedMethods.CheckIfAvailableName(path, SharedMethods.ENGAlphabet(video.Title), Format.Mp3);

                Console.WriteLine("Download has started!");

                await client.Videos.Streams.DownloadAsync(streamManifest.GetAudioOnly().Where(x => x.Container.Name == (Mp4Mode ? "mp4" : "webm")).WithHighestBitrate(), $"{path}\\{tit}.mp3", pro);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void HandleProgress(double progress) => SharedMethods.HandleProgress(progress);
    }
}
