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

        private static int Current = 0;

        public static double Previous { get; set; } = 0;

        public static int ListMaxLength { get; set; } = 0;

        public static void HandleProgress(double progress, bool IfQueueOrPlaylist = false)
        {
            string x = ((int)(progress * 100)).ToString();
            Console.Title = string.Format("BYTDownloader | {0}%", x);

            if (x == "100" && !IfQueueOrPlaylist)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Thread.Sleep(1000);
                Console.WriteLine("Done");
            }
        }

        private static string FilterForbiddenChars(string title)
        {
            var ForbiddenChars = "\\/:*?\"<>|".ToCharArray();
            List<char> tmp = new List<char>();

            foreach (var x in title)
            {
                bool result = ForbiddenChars.Any(s => s.ToString().Contains(x));

                if (!result)
                    tmp.Add(x);
                else
                    tmp.Add('#');
            }

            return new string(tmp.ToArray());
        }

        public static string CheckIfAvailableName(string path, string name, string format)
        {
            //Format x = !string.IsNullOrEmpty(format) ? (Format)Enum.Parse(typeof(Format), format, true) : throw new NullReferenceException("string can not be null");

            //return File.Exists($"{path}\\{name}.{format.ToString().ToLower()}") ? name.Contains("(") ? CheckIfAvailableName(path, $"{name[..name.IndexOf("(", StringComparison.Ordinal - 1)]} ({Files++})", format) : $"{name} ({Files++})" : name;

            name = FilterForbiddenChars(name);
            format = format.ToLower();

            if (File.Exists($"{path}\\{name}.{format}"))
                return name.Contains("(") ? CheckIfAvailableName(path, $"{name[..name.IndexOf("(", StringComparison.Ordinal - 1)]} ({Files++})", format) : $"{name} ({Files++})";
            else
                return name;
        }
    }
}
