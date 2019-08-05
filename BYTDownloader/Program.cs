using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;

namespace BYTDownloader
{
    class Program
    {
        static void Main()
        {
            Console.Title = "BYTDownloader";
            new License();
            CheckFFMPGEG();
        }

        private static void CheckFFMPGEG()
        {
            var CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (!File.Exists(CurrentDirectory + "\\ffmpeg.exe"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ffmpeg.exe not found");
                Console.WriteLine("Start downloading ffmpeg.exe");

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(new Uri("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-20190530-9c35285-win64-static.zip"), CurrentDirectory + "//FFMPEG.zip");

                    Directory.CreateDirectory(CurrentDirectory + "//temp");
                    ZipFile.ExtractToDirectory(CurrentDirectory + "//FFMPEG.zip", CurrentDirectory + "//temp");
                    File.Move(CurrentDirectory + "//temp/ffmpeg-20190530-9c35285-win64-static/bin/ffmpeg.exe", CurrentDirectory + "//ffmpeg.exe");
                }

                Console.WriteLine("ffmpeg.exe downloaded!");
                Console.ResetColor();
                Console.Clear();
            }

            if (File.Exists(CurrentDirectory + "//FFMPEG.zip"))
            {
                File.Delete(CurrentDirectory + "//FFMPEG.zip");
            }

            if (Directory.Exists(CurrentDirectory + "//temp"))
            {
                DeleteDirectory(CurrentDirectory + "//temp");
            }
            INI();
        }

      

        private static void INI()
        {
            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");
            Console.WriteLine("3: Playlist");
            Console.WriteLine("4: Add stuff to queue");
            Console.WriteLine("------------");
            Console.WriteLine("5: Converter");
            Console.WriteLine("------------");

            var x = Console.ReadLine();
            Console.Clear();

            if (uint.Parse(x) == 1)
            {
                new DownloadVideo();
            }
            else if (uint.Parse(x) == 2)
            {
               new DownloadSong();
            }
            else if (uint.Parse(x) == 3)
            {
                Console.WriteLine("1: Specific part?");
                Console.WriteLine("2: Whole playlist");
                var y = Console.ReadLine();
                Console.Clear();
                if (int.Parse(y) == 1)
                {
                    new DownloadPlaylist(true);
                }
                else if (int.Parse(y) == 2)
                {
                    new DownloadPlaylist(false);
                }
            }
            else if (uint.Parse(x) == 4)
            {
                new Queue();
            }
            else if (uint.Parse(x) == 5)
            {
                new Converter();
            }
            Console.ReadLine();
        }
        
        private static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }

    public enum Format
    {
        Mp3,
        Mp4,
        wav,
    }
}
