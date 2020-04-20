using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace BYTDownloader
{
    public class License
    {
        private readonly string[] license = new string[4];
        private readonly string[] license_in_folder;
        private readonly string[] missing_licenses;
        private int number;

        public License()
        {
            license_in_folder = new string[license.Length];
            missing_licenses = new string[license.Length];

            license[0] = "BYTDownloader";
            license[1] = "YoutubeExplode";
            license[2] = "YoutubeExplode.Converter";
            license[3] = "FFmpeg.NET";

            number = Count_Licenses();

            if (license.Length.ToString() != number.ToString())
            {
                Get_Missing_Licenses();
                Download_Licenses();
            }
        }
        private int Count_Licenses()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Licenses");
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Exists)
            {
                int i = 0;
                if (dir.GetFiles("*.txt").Length > 0)
                {
                    foreach (var obj in dir.GetFiles("*.txt"))
                    {
                        for (int j = 0; j < license.Length; j++)
                        {
                            if (obj.ToString().Contains(license[j] + ".txt"))
                            {
                                license_in_folder[j] = license[j];
                            }
                            else
                            {
                                license_in_folder[j] = string.Empty;
                            }
                        }
                        i++;
                    }
                }
                else
                {
                    for (int i2 = 0; i2 < license.Length; i2++)
                    {
                        license_in_folder[i2] = string.Empty;
                    }
                }
                return i;
            }
            else
            {
                Directory.CreateDirectory(path);
                Count_Licenses();
            }
            return 0;
        }

        private void Get_Missing_Licenses()
        {
            try
            {
                for (int i = 0; i < license.Length; i++)
                {
                    if (!license_in_folder[i].Contains(license[i]))
                    {
                        missing_licenses[i] = license[i];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void Download_Licenses()
        {
            string download_path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Licenses");
            Directory.CreateDirectory(download_path);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("Start downloading missing licenses");

            for (int i = 0; i < missing_licenses.Length; i++)
            {
                try
                {
                    using var fs = new FileStream(string.Format(Path.Combine(download_path, "{0}.txt"), missing_licenses[i]), FileMode.CreateNew);
                    using HttpClient httpClient = new HttpClient();
                    httpClient.GetStreamAsync($"https://raw.githubusercontent.com/BlauFx/BYTDownloader/master/Licenses/{missing_licenses[i]}.txt").Result.CopyTo(fs);
                }
                catch { }
            }

            Console.Clear();
        }
    }
}