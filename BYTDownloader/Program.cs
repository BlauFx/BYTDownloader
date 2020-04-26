using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace BYTDownloader
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "BYTDownloader";

            new License();
            CheckFFMPEG();
        }

        private static void CheckFFMPEG()
        {
            var CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (!File.Exists(CurrentDirectory + "\\ffmpeg.exe"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ffmpeg.exe not found");
                Console.WriteLine("Start downloading ffmpeg.exe");

                using var fs = new FileStream(CurrentDirectory + "//FFMPEG.zip", FileMode.CreateNew);
                using HttpClient httpClient = new HttpClient();
                httpClient.GetStreamAsync("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-20200417-889ad93-win64-static.zip").Result.CopyTo(fs);

                Directory.CreateDirectory(CurrentDirectory + "//temp");
                ZipFile.ExtractToDirectory(CurrentDirectory + "//FFMPEG.zip", CurrentDirectory + "//temp");
                File.Move(CurrentDirectory + "//temp/ffmpeg-20200417-889ad93-win64-static/bin/ffmpeg.exe", CurrentDirectory + "//ffmpeg.exe");

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

        private static void INI()
        {
            Console.WriteLine("1: Video\n" +
                "2: Song\n" +
                "3: Playlist\n" +
                "4: Add stuff to queue\n" +
                "------------\n" +
                "5: Converter\n" +
                "------------\n" +
                "");

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

            Console.ReadLine();
        }
    }

    public enum Format
    {
        Mp3,
        Mp4,
        wav,
    }
}
