using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace BYTDownloader
{
    public class DownloadSong
    {
        private bool Run = false;
        
        private IProgress<double> pro;
        
        private double Previous = 0;

        private int Files = 0;

        private string TmpTitle = string.Empty;
        
        public DownloadSong()
        {
            pro = new Progress<double>(HandleProgress);
            
            DSong();
        }

        private async void DSong()
        {
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            var id = YoutubeClient.ParseVideoId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

            try
            {
                var title1 = await client.GetVideoAsync(id);
                string title = ENGAlphabet(title1.Title);

                var mediaStreamInfos = new MediaStreamInfo[] {audioStreamInfo};

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string tit = CheckIfAvailableName(path, title, Format.Mp3);
                
                try
                {
                    await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp3", "mp3", pro);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private string ENGAlphabet(string tit)
        {
            List<string> tmp = new List<string>();
            string Titel = string.Empty;
            
            foreach (var x in tit)
            {
                bool result = x.ToString().Any(x => char.IsLetterOrDigit(x));
                bool result2 = x.ToString().Any(x => char.IsWhiteSpace(x));

                if (result || result2)
                {
                    tmp.Add(x.ToString());
                }
                else
                {
                    tmp.Add("#");
                }
            }
            
            foreach (var x in tmp)
            {
                Titel += x;
            }
            
            return Titel;
        }

        private string CheckIfAvailableName(string path, string name, Format format = Format.Mp3)
        {
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                CheckIfAvailableNameVoid(path, name, format);
                return TmpTitle;
            }
            return name;
        }

        private void CheckIfAvailableNameVoid(string path, string name, Format format = Format.Mp3)
        {
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                Files++;
                CheckIfAvailableNameVoid(path, FileCutter(name,Files), format);
            }
            else
            {
                TmpTitle = name;
            }
        }

        private string FileCutter(string str, int num)
        {
            if (str.Contains("("))
            {
                int i = str.IndexOf("(", StringComparison.Ordinal);
                string j = str.Substring(0, i);
                return $"{j} ({num})";
            }
            
            return $"{str} ({num})";
        }

        private void HandleProgress(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (x == "100")
            {
                Run = CalcIfDoubleUsed(progress);
            }

            if (Run)
            {
                Run = false;
                Console.ForegroundColor = ConsoleColor.Red;
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("Done");
            }
        }
        
        private bool CalcIfDoubleUsed(double progress)
        {
            if (progress == 1)
            {
                if (progress == Previous)
                {
                    return false;
                }
                Previous = progress;
                return true;
            }

            Previous = progress;

            return false;
        }
    }

    public enum Format
    {
        Mp3,
        Mp4,
        
    }
}
