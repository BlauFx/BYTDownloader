using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.IO.Directory;

namespace BYTDownloader
{
    class Updater
    {
        private const string APIURL = "https://api.github.com/repos/BlauFx/BYTDownloader/releases";
        private const string APIURLUpdater = "https://api.github.com/repos/BlauFx/Updater/releases";

        private readonly string ExePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private List<Github_Releases> Github_Releases = new List<Github_Releases>();
        private List<Github_Releases> Github_ReleasesUpdater = new List<Github_Releases>();

        public bool IsUpdating = false;

        public Updater()
        {
            if (SearchAsyncForUpdates())
            {
                Console.Write("A new version is available");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.Write("\nDo you want to download and apply the update? [y/n]: ");
                    if (Console.ReadLine() == "y")
                    {
                        IsUpdating = true;

                        DownloadUpdateUpdater();
                        DownloadUpdate();
                        ApplyUpdate();
                    }
                }
                else
                {
                    Console.WriteLine("Please download the update manually");
                    Console.WriteLine("The Auto update feature is not supported on your platform!");
                }
            }
        }

        private bool SearchAsyncForUpdates()
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response;

                httpClient.DefaultRequestHeaders.Add("user-agent", ".");

                response = httpClient.GetAsync(APIURL).Result;
                response.EnsureSuccessStatusCode();
                Github_Releases = JsonConvert.DeserializeObject<List<Github_Releases>>(response?.Content.ReadAsStringAsync().Result);

                response = httpClient.GetAsync(APIURLUpdater).Result;
                response.EnsureSuccessStatusCode();
                Github_ReleasesUpdater = JsonConvert.DeserializeObject<List<Github_Releases>>(response?.Content.ReadAsStringAsync().Result);
            }
            catch { /*It can fail due to no internet connection or being rate-limited*/ }

            return CheckNewVersionAvailable();
        }

        private bool CheckNewVersionAvailable()
        {
            var assets = Github_Releases?.FirstOrDefault(x => x.assets == x.assets);
            return !GetCurrentVersion().Equals(assets?.tag_name ?? GetCurrentVersion());
        }

        private string GetCurrentVersion() => Program.Version;

        public void DownloadUpdate()
        {
            var x = Github_Releases.First(x => x.assets == x.assets);
            Assets win_x64 = x.assets.FirstOrDefault(y => y.name.Equals("win-x64.zip"));

            if (!Exists(@$"{ExePath}/temp"))
                CreateDirectory(@$"{ExePath}/temp");

            try
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("user-agent", ".");

                using var fs = new FileStream(@$"{ExePath}/temp/win-x64.zip", FileMode.CreateNew);
                httpClient.GetStreamAsync(win_x64.browser_download_url).Result.CopyTo(fs);
            }
            catch (Exception e) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to download win-x64 ({x.tag_name})\nError msg: {e.Message}");

                Console.ReadLine();
                Environment.Exit(0);
            }

            if (File.Exists(@$"{ExePath}/temp/win-x64.zip"))
            {
                using StreamWriter strmWriter = new StreamWriter(@$"{ExePath}/temp/win-x64.txt");
                strmWriter.WriteLine("Done");
            }
        }

        public void DownloadUpdateUpdater()
        {
            var x = Github_ReleasesUpdater?.FirstOrDefault(x => x.assets == x.assets);
            Assets Updater = x?.assets?.FirstOrDefault(y => y.name.Equals("Updater.exe"));

            if (File.Exists("Updater.exe"))
                File.Delete("Updater.exe");

            try
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("user-agent", ".");

                using var fs = new FileStream(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "//Updater.exe", FileMode.CreateNew);
                httpClient.GetStreamAsync(Updater?.browser_download_url).Result.CopyTo(fs);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to download updater ({x?.tag_name})\nError msg: {e.Message}");

                Console.ReadLine();

                Console.ResetColor();
                Environment.Exit(0);
            }
        }

        public async void ApplyUpdate()
        {
            if (File.Exists($"{ExePath}\\temp\\win-x64.txt"))
            {
                using StreamReader reader = new StreamReader($"{ExePath}\\temp\\win-x64.txt");
                if (!(reader.ReadLine() == "Done"))
                {
                    return;
                }
            }

            static void CreateDir(string path)
            {
                if (!Exists(path))
                    CreateDirectory(path);
                else
                {
                    Delete(path, true);
                    CreateDirectory(path);
                }
            }

            CreateDir($"{ExePath}\\temp\\files");
            CreateDir($"{ExePath}\\temp\\old");

            int PID = Process.GetCurrentProcess().Id;
            ZipFile.ExtractToDirectory($"{ExePath}\\temp\\win-x64.zip", $"{ExePath}\\temp\\files");

            File.Move($"{ExePath}\\Updater.exe", $"{ExePath}\\temp\\Updater.exe");
            new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/C start {ExePath}\\temp\\Updater.exe {PID} {ExePath}",
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                },
            }.Start();

            await Task.Delay(5000); //Just wait a lil bit until the Process has started
            Environment.Exit(0);
        }
    }

    internal class Github_Releases
    {
        public string tag_name { get; set; }
        public List<Assets> assets { get; set; }
    }

    internal class Assets
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }

        public List<Assets> assets { get; set; }
    }
}
