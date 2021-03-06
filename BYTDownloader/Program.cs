using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;

namespace BYTDownloader
{
    internal class Program
    {
        static void Main()
        {
            Console.Title = "BYTDownloader";

            if (Directory.Exists("temp"))
                Directory.Delete("temp", true);

            if (File.Exists("FFMPEG.zip"))
                File.Delete("FFMPEG.zip");

            new License();
            Updater updater = new Updater();

            if (!updater.IsUpdating)
            {
                var CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

                if (!File.Exists(CurrentDirectory + "\\ffmpeg.exe"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ffmpeg.exe not found\nDownloading ffmpeg.exe");

                    using (var fs = new FileStream(CurrentDirectory + "//FFMPEG.zip", FileMode.CreateNew))
                    using (HttpClient httpClient = new HttpClient())
                        httpClient.GetStreamAsync("https://github.com/ShareX/FFmpeg/releases/download/v4.3.1/ffmpeg-4.3.1-win64.zip")?.Result.CopyTo(fs);

                    Directory.CreateDirectory(CurrentDirectory + "//temp");
                    ZipFile.ExtractToDirectory(CurrentDirectory + "//FFMPEG.zip", CurrentDirectory + "//temp");
                    File.Move(CurrentDirectory + "//temp/ffmpeg.exe", CurrentDirectory + "//ffmpeg.exe");

                    Console.WriteLine("ffmpeg.exe downloaded!");
                    Console.ResetColor();
                    Console.Clear();
                }

                if (File.Exists(CurrentDirectory + "//FFMPEG.zip"))
                    File.Delete(CurrentDirectory + "//FFMPEG.zip");

                if (Directory.Exists(CurrentDirectory + "//temp"))
                    Directory.Delete(CurrentDirectory + "//temp", true);

                INI();
            }
            else
                Console.ReadLine();
        }

        private static void INI()
        {
            Console.WriteLine(
                "1: Video\n" +
                "2: Song\n" +
                "3: Playlist\n" +
                "4: Add stuff to queue\n" +
                "------------\n" +
                "5: Converter\n" +
                "------------\n" +
                "6: About\n" +
                "------------");

            var x = Console.ReadLine();
            Console.Clear();

            if (uint.Parse(x) == 1)
                new DownloadVideo();
            else if (uint.Parse(x) == 2)
                new DownloadSong();
            else if (uint.Parse(x) == 3)
                new DownloadPlaylist();
            else if (uint.Parse(x) == 4)
                new Queue();
            else if (uint.Parse(x) == 5)
                new Converter();
            else if (uint.Parse(x) == 6)
                About();

            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.ReadLine();
        }

        private static void About()
        {
            Console.Clear();
            Console.WriteLine($"BYTDownloader current version: {Assembly.GetExecutingAssembly().GetName().Version}\n" +
                $"Using YoutubeExplode: {Assembly.GetAssembly(typeof(YoutubeExplode.YoutubeClient))?.GetName().Version}\n" +
                $"Using YoutubeExplode.Converter: {Assembly.GetAssembly(typeof(YoutubeExplode.Converter.ConversionRequest))?.GetName().Version}\n" +
                $"Using FFmpeg.NET: {Assembly.GetAssembly(typeof(FFmpeg.NET.Engine))?.GetName().Version}\n" +
                $"\nPress enter to return to mainmenu");

            Console.ReadLine();
            Console.Clear();

            INI();
        }
    }

    public enum Format
    {
        mp3,
        mp4,
        wav,
    }
}
