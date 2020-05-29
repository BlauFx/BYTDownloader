using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BYTDownloader
{
    public static class SharedMethods
    {
        public static string Path { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static int Files = 0;

        private static int Current = 0;

        public static double Previous { get; set; } = 0;

        public static int ListMaxLength { get; set; } = 0;

        public static void HandleProgress(double progress, bool IfQueueOrPlaylist = false)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (IfQueueOrPlaylist)
            {
                static bool CalcIfDoubleUsed(double progress)
                {
                    if (progress == 1)
                    {
                        if (progress == Previous)
                            return false;

                        Previous = progress;
                        return true;
                    }

                    Previous = progress;
                    return false;
                }

                string x2 = ((int)(progress * 100)).ToString();
                Console.Title = string.Format("BYTDownloader | {0}%", x2);

                if (CalcIfDoubleUsed(progress))
                {
                    Current++;
                    Console.WriteLine($"{Current} / {ListMaxLength}");

                    if (Current == ListMaxLength)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Thread.Sleep(1000);
                        Console.WriteLine("Done");
                    }
                }

                return;
            }

            Current++;
        }

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

        public static string CheckIfAvailableName(string path, string name, Format format = Format.mp3)
        {
            if (File.Exists($"{path}\\{name}.{format.ToString().ToLower()}"))
            {
                Files++;
                return CheckIfAvailableName(path, FileCutter(name, Files), format);
            }
            else
                return name;
        }

        public static string FileCutter(string str, int num) 
            => str.Contains("(") ? $"{str.Substring(0, str.IndexOf("(", StringComparison.Ordinal) - 1)} ({num})" : $"{str} ({num})";
    }
}
