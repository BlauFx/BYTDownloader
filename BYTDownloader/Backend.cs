using System;
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
        protected IProgress<double> pro;

        public YoutubeClient client = new YoutubeClient();

        public string Title { get; set; }

        public IStreamInfo[] mediaStreamInfos = null;

        public List<string> list;

        public Format format;

        public List<IStreamInfo[]> streamInfosList = new List<IStreamInfo[]>();

        protected Backend(bool IsVideo, List<string> list = null)
        {
            pro = new Progress<double>((p) => SharedMethods.HandleProgress(p, false));
            this.list = list;

            PrepareDownload(IsVideo).GetAwaiter().GetResult();
            DownloadFile(client, mediaStreamInfos, SharedMethods.Path, Title, IsVideo ? Format.Mp4 : Format.Mp3).GetAwaiter().GetResult();
        }

        public abstract Task PrepareDownload(bool IsVideo);

        public virtual async Task DownloadFile(YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, string tit, Format format)
        {
            Console.WriteLine("Download has started!");
            await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(readOnlyList, $"{path}\\{tit}.{format}", format.ToString(), pro);
        }
    }

    public abstract class BackendPlaylist
    {
        public IProgress<double> Pro { get; set; }

        public IProgress<double> Pro2 { get; set; }

        public YoutubeClient client = new YoutubeClient();

        public string Title { get; set; }

        public int Answer { get; set; }

        public IStreamInfo[] MediaStreamInfos { get; set; } = null;

        public IReadOnlyList<Video> PlaylistVideos { get; set; }

        public int Part { get; set; }

        public Format Format { get; set; } = Format.Mp4;

        public int CurrentPos { get; set; } = 1;

        protected BackendPlaylist()
        {
            Pro = new Progress<double>((p) => SharedMethods.HandleProgress(p, false));

            Console.WriteLine("1: Specific part?\n2: Whole playlist");

            int input = int.Parse(Console.ReadLine());
            bool WholePlaylist = input == 1 ? false : true;

            PrepareDownloadBackend(WholePlaylist);
            PrepareDownload(WholePlaylist).GetAwaiter().GetResult();

            Console.WriteLine("Download has started!");
            DownloadFile(WholePlaylist, client, MediaStreamInfos, SharedMethods.Path, Format).GetAwaiter().GetResult();
        }

        public abstract Task PrepareDownload(bool WholePlaylist);

        private void PrepareDownloadBackend(bool WholePlaylist)
        {
            Console.Clear();

            Console.WriteLine("URL: ");
            var url = Console.ReadLine();

            Console.Clear();

            if (!WholePlaylist)
            {
                Console.WriteLine("Which part in the playlist?");
                Part = int.Parse(Console.ReadLine()) - 1;
            }

            Console.Clear();
            Console.WriteLine("Do you want the download it as a video or as a song?");

            Console.WriteLine("1: Video");
            Console.WriteLine("2: Song");

            Answer = int.Parse(Console.ReadLine());
            Console.Clear();

            Console.Title = "BYTDownloader | Loading...";
            PlaylistVideos = client.Playlists.GetVideosAsync(client.Playlists.GetAsync(url).GetAwaiter().GetResult().Id).BufferAsync().GetAwaiter().GetResult();

            if (!Directory.Exists(SharedMethods.Path + "\\Playlist"))
                Directory.CreateDirectory(SharedMethods.Path + "\\Playlist");
        }

        public virtual async Task DownloadFile(bool WholePlaylist, YoutubeClient client, IReadOnlyList<IStreamInfo> readOnlyList, string path, Format format)
        {
            await new YoutubeConverter(client).DownloadAndProcessMediaStreamsAsync(readOnlyList, $"{path}\\Playlist\\{Title}.{format.ToString().ToLower()}", format.ToString().ToLower(), Pro);
        }
    }
}
