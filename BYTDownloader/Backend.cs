﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BYTDownloader
{
    public abstract class Backend
    {
        protected IProgress<double> pro { get; set; }

        public YoutubeClient client { get; set; } = new YoutubeClient();

        public string Title { get; set; }

        public IStreamInfo[] mediaStreamInfos { get; set; } = null;

        public Format format { get; set; }

        public List<IStreamInfo[]> StreamInfosList { get; set; } = new List<IStreamInfo[]>();

        public List<string> list { get; set; }

        protected Backend(bool IsVideo)
        {
            pro = new Progress<double>((p) => SharedMethods.HandleProgress(p, false));

            PrepareDownload(IsVideo);
            DownloadFile(client, mediaStreamInfos, SharedMethods.Path, Title, IsVideo ? Format.mp4 : Format.mp3, false);
        }

        protected Backend(List<string> list, bool IsVideo)
        {
            pro = new Progress<double>((p) => SharedMethods.HandleProgress(p, true));

            this.list = list;
            SharedMethods.ListMaxLength = list.Count;

            PrepareDownload(IsVideo);
            DownloadFile(client, mediaStreamInfos, SharedMethods.Path, Title, IsVideo ? Format.mp4 : Format.mp3, true);
        }

        public abstract void PrepareDownload(bool IsVideo);

        private async void DownloadFile(YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, string title, Format format, bool IsQueue)
        {
            Console.SetIn(TextReader.Null);
            Console.WriteLine("Download has started!");

            if (IsQueue)
            {
                YoutubeConverter converter = new YoutubeConverter(client);

                for (int i = 0; i < SharedMethods.ListMaxLength; i++)
                    await converter.DownloadAndProcessMediaStreamsAsync(StreamInfosList[i],
                        $"{path}\\{SharedMethods.CheckIfAvailableName(path, SharedMethods.ENGAlphabet(title), format)}.{format.ToString().ToLower()}", format.ToString().ToLower(), pro);
            }
            else
                await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(readOnlyList, $"{path}\\{title}.{format}", format.ToString().ToLower(), pro);
        }
    }

    public abstract class BackendPlaylist
    {
        public IProgress<double> Pro { get; set; }

        public YoutubeClient client = new YoutubeClient();

        public string Title { get; set; }

        public int Answer { get; set; }

        public IStreamInfo[] MediaStreamInfos { get; set; } = null;

        public IReadOnlyList<Video> PlaylistVideos { get; set; }

        public int Part { get; set; }

        public Format Format { get; set; } = Format.mp4;

        public int CurrentPos { get; set; } = 1;

        protected BackendPlaylist()
        {
            Pro = new Progress<double>((p) => SharedMethods.HandleProgress(p, false));

            Console.WriteLine("1: Specific part?\n2: Whole playlist");

            bool WholePlaylist = int.Parse(Console.ReadLine()) != 1;

            PrepareDownload(WholePlaylist).GetAwaiter().GetResult();
            DownloadFile(WholePlaylist, client, MediaStreamInfos, SharedMethods.Path, Format).GetAwaiter().GetResult();
        }

        public abstract Task PrepareDownload(bool WholePlaylist);

        public virtual async Task DownloadFile(bool WholePlaylist, YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, Format format) 
            => await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(readOnlyList, $"{path}\\Playlist\\{Title}.{format.ToString().ToLower()}", format.ToString().ToLower(), Pro);
    }
}
