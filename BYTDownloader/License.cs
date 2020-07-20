using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace BYTDownloader
{
    public class License
    {
        private readonly List<string> license = new List<string>();

        public License()
        {
            license.Add("BYTDownloader");
            license.Add("YoutubeExplode");
            license.Add("YoutubeExplode.Converter");
            license.Add("FFmpeg.NET");
            license.Add("Updater");

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Licenses");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using HttpClient httpClient = new HttpClient();

            for (int i = 0; i < license.Count; i++)
            {
                if (File.Exists($"{path}\\{license[i]}.txt"))
                    continue;

                using var fs = new FileStream($"{path}\\{license[i]}.txt", FileMode.CreateNew);
                httpClient.GetStreamAsync($"https://raw.githubusercontent.com/BlauFx/BYTDownloader/master/Licenses/{license[i]}.txt").Result.CopyTo(fs);
            }
        }
    }
}
