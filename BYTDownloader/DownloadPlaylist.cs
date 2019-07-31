using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace BYTDownloader
{
    public class DownloadPlaylist
    {
        private bool Run = false;

        private IProgress<double> pro;
        
        private IProgress<double> pro2;

        private int PlaylistMaxLength = 0;

        private int Current = 0;

        private double Previous = 0;
        
        private string TmpTitle = string.Empty;
        
        private int Files = 0;
        
        public DownloadPlaylist(bool specificPart)
        {
            pro = new Progress<double>(HandleProgress);
            
            pro2 = new Progress<double>(HandleProgress2);
            
            DPlaylist(specificPart);
        }

        private void DPlaylist(bool specificPart)
        {
            if (specificPart)
            {
                DPlaylist_SPECIFIC_PART_Playlist();
            }
            else
            {
                DPlaylist_WHOLE_Playlist();
            }
        }
        
        private async void DPlaylist_SPECIFIC_PART_Playlist()
        {
            Console.Clear();
            
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            
            Console.Clear();
            
            Console.WriteLine("Which part in the playlist?");
            int part = int.Parse(Console.ReadLine());
            
            Console.Clear();
            
            Console.WriteLine("Do you want the download it as a video or as a song?");
            
            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string answer = Console.ReadLine();
            
            Console.Clear();
            
            Console.WriteLine("Download has started!");
            
            var id = YoutubeClient.ParsePlaylistId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var playlist = await client.GetPlaylistAsync(id);
            var video = playlist.Videos.ElementAt(part - 1).Id;

            var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(video);
            var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

            if (int.Parse(answer) == 1) //Video
            {
                try
                {
                    var title1 = await client.GetVideoAsync(video);
                    string title = ENGAlphabet(title1.Title);
                    
                    var videoStreamInfo = mediaStreamInfoSet.Video
                        .OrderByDescending(s => s.VideoQuality)
                        .ThenByDescending(s => s.Framerate)
                        .First();

                    var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string tit = CheckIfAvailableName(path, title, Format.Mp4);
                
                    try
                    {
                        await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\{tit}.mp4", "mp4", pro);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (int.Parse(answer) == 2) //Sound
            {
                try
                {
                    var title1 = await client.GetVideoAsync(video);
                    string title = ENGAlphabet(title1.Title);

                    var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };
                    
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
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async void DPlaylist_WHOLE_Playlist()
        {
            Console.Clear();
            
            Console.WriteLine("URL: ");
            var url = Console.ReadLine();
            
            Console.Clear();
            
            Console.WriteLine("Do you want the download it as a video or as a song?");
            
            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            string Answer = Console.ReadLine();
            
            Console.Clear();
            
            Console.WriteLine("Download has started!");
            
            var id = YoutubeClient.ParsePlaylistId(url);

            var client = new YoutubeClient();
            var converter = new YoutubeConverter(client);

            var playlist = await client.GetPlaylistAsync(id);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            if (!(Directory.Exists(path + "\\Playlist")))
            {
                Directory.CreateDirectory(path + "\\Playlist");
            }

            PlaylistMaxLength = playlist.Videos.Count;

            for (int i = 0; i < playlist.Videos.Count; i++)
            {
                var video = playlist.Videos.ElementAt(i).Id;

                var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync(video);

                var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

                if (int.Parse(Answer) == 1) //Video
                {
                    try
                    {
                        var title1 = await client.GetVideoAsync(video);
                        string title = ENGAlphabet(title1.Title);
                        
                        var videoStreamInfo = mediaStreamInfoSet.Video
                            .OrderByDescending(s => s.VideoQuality)
                            .ThenByDescending(s => s.Framerate)
                            .First();

                        var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

                        string tit = CheckIfAvailableName(path + "\\Playlist", title, Format.Mp4);
                
                        try
                        {
                            await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\Playlist\\{tit}.mp4", "mp4", pro2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (int.Parse(Answer) == 2)//Sound
                {
                    try
                    {
                        var title1 = await client.GetVideoAsync(video);
                        string title = ENGAlphabet(title1.Title);

                        var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };
                    
                        string tit = CheckIfAvailableName(path + "\\Playlist", title, Format.Mp3);
                
                        try
                        {
                            await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, path + $"\\Playlist\\{tit}.mp3", "mp3", pro2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
        
        private string ENGAlphabet(string tit)
        {
            List<string> tmp = new List<string>();
            string titel = string.Empty;
            
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
                titel += x;
            }
            
            return titel;
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
                Files = 0;
                TmpTitle = name;
            }
        }

        private string FileCutter(string str, int num)
        {
            if (str.Contains("("))
            {
                int i = str.IndexOf("(", StringComparison.Ordinal);
                string j = str.Substring(0, i);
                return $"{j}({num})";
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
        
        private void HandleProgress2(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            Run = CalcIfDoubleUsed(progress);

            if (Run)
            {
                Run = false;
                Current++;

                Console.WriteLine($"{Current} / {PlaylistMaxLength}");

                if (Current == PlaylistMaxLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Done");
                }
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
}