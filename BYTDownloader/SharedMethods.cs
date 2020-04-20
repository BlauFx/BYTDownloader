using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BYTDownloader
{
    public static class SharedMethods
    {
        private static bool Run = false;

        private static int Files = 0;

        private static string TmpTitle = string.Empty;

        private static int PlaylistMaxLength = 0;

        private static int Current = 0;

        private static double Previous = 0;

        public static void HandleProgress(double progress)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (x == "100")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("Done");
            }
        }

        public static void HandleProgress2(double progress)
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

        private static bool CalcIfDoubleUsed(double progress)
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

public static string FileCutter(string str, int num) => str.Contains("(") ? $"{str.Substring(0, str.IndexOf("(", StringComparison.Ordinal))} ({num})" : $"{str} ({num})";

        public static string ENGAlphabet(string tit)
        {
            char[] list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ()$.-[]".ToCharArray();
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
            if (File.Exists(path + $"\\{name}.{format}"))
            {
                CheckIfAvailableNameVoid(path, name, format);
                return TmpTitle;
            }
            return name;
        }

        public static void CheckIfAvailableNameVoid(string path, string name, Format format = Format.Mp3)
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
