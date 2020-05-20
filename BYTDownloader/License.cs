using System.IO;
using System.Net.Http;
using System.Reflection;

namespace BYTDownloader
{
    public class License
    {
        private readonly string[] license = new string[6];
        
        public License()
        {
            license[0] = "BYTDownloader";
            license[1] = "YoutubeExplode";
            license[2] = "YoutubeExplode.Converter";
            license[3] = "Newtonsoft.Json";
            license[4] = "FFmpeg.NET";
            license[5] = "Updater";

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Licenses");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using HttpClient httpClient = new HttpClient();

            for (int i = 0; i < license.Length; i++)
            {
                if (!File.Exists($"{path}\\{license[i]}.txt"))
                {
                    try 
                    {
                        using var fs = new FileStream($"{path}\\{license[i]}.txt", FileMode.CreateNew);
                        httpClient.GetStreamAsync($"https://raw.githubusercontent.com/BlauFx/BYTDownloader/master/Licenses/{license[i]}.txt").Result.CopyTo(fs);
                    } catch { }
                }
            }
        }
    }
}
