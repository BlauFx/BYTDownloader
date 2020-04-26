using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BYTDownloader
{
    public static class SharedMethods
    {
        private static int Files = 0;

        private static string TmpTitle = string.Empty;

        public static string Path { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static int Current = 0;

        public static void HandleProgress(double progress, bool Playlist, int PlaylistLength = 0)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (Playlist && Current == PlaylistLength && x == "100")
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Thread.Sleep(1000);
                Console.WriteLine("Done");
            }
            else if (!Playlist && x == "100")
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Thread.Sleep(1000);
                Console.WriteLine("Done");
            }

            Current++;
        }

        public static string FileCutter(string str, int num) => str.Contains("(") ? $"{str.Substring(0, str.IndexOf("(", StringComparison.Ordinal)-1)} ({num})" : $"{str} ({num})";

        public static string ENGAlphabet(string tit)
        {
            char[] list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ()$.-[]&'".ToCharArray();
            List<string> tmp = new List<string>();

            string Titel = string.Empty;
            foreach (var x in tit)
            {
                bool result = x.ToString().Any(x => char.IsLetterOrDigit(x));
                bool result2 = x.ToString().Any(x => char.IsWhiteSpace(x));
                bool result3 = list.Any(s => s.ToString().Contains(x));

                if (result || result2 || result3)
                    tmp.Add(x.ToString());
                else
                    tmp.Add("#");
            }

            foreach (var x in tmp)
            {
                Titel += x;
            }

            return Titel;
        }

        public static string CheckIfAvailableName(string path, string name, Format format = Format.Mp3)
        {
            if (File.Exists(path + $"\\{name}.{format.ToString().ToLower()}"))
            {
                CheckIfAvailableNameVoid(path, name, format.ToString().ToLower());
                return TmpTitle;
            }
            return name;
        }

        public static void CheckIfAvailableNameVoid(string path, string name, string format)
        {
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                Files++;
                CheckIfAvailableNameVoid(path, FileCutter(name, Files), format);
            }
            else
            {
                Files = 0;
                TmpTitle = name;
            }
        }
    }
}
