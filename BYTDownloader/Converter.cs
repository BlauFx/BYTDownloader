using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace BYTDownloader
{
    class Converter
    {
        private string TmpTitle = string.Empty;

        private int Files = 0;
        
        public Converter()
        {
            Console.Title = "Converter";

            Console.Write("Input file: ");

            string x = Console.ReadLine();

            string x2 = Fixinput(x);

            var inputFile = new MediaFile { Filename = x2 };

            Console.Write("Fomat: ");

            var format = Console.ReadLine();

            var outputFile = new MediaFile { Filename = string.Format(@"{0}\{1}.{2}", GetFileDir(x2), GetOutputName(x2,format), format) };

            var conversionOptions = new ConversionOptions
            {
                VideoAspectRatio = VideoAspectRatio.Default,
                VideoSize = VideoSize.Custom,
                AudioSampleRate = AudioSampleRate.Hz48000,
            };

            using (var engine = new Engine(string.Format("{0}\\ffmpeg.exe", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))))
            {
                engine.ConvertProgressEvent += Engine_ConvertProgressEvent;
                engine.Convert(inputFile, outputFile, conversionOptions);
            }

            Console.ForegroundColor = ConsoleColor.Red;

            if (File.Exists(outputFile.Filename))
            {
                Thread.Sleep(100);
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("Could not create new file\n");
                Console.WriteLine("Possible errors: \ninput file does not exist / is not an audio/video file\nformat is wrong");
            }
        }

        private string Fixinput(string input)
        {
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                string str = input.Substring(1, input.Length - 2);
                return str;
            }
            else if (input.StartsWith("\"") && !(input.EndsWith("\"")))
            {
                string str = input.Substring(1, input.Length - 1);
                return str;
            }
            else if (input.EndsWith("\"") && !(input.StartsWith("\"")))
            {
                string str = input.Substring(0, input.Length - 1);
                return str;
            }
            else
            {
                return input;
            }
        }

        private string GetFileDir(string inputFile)
        {
            int num = inputFile.LastIndexOf("\\", StringComparison.Ordinal);
            string str = inputFile.Substring(0, num);

            return str;
        }

        private string GetOutputName(string inputFile, string format)
        {
            int num = inputFile.LastIndexOf("\\", StringComparison.Ordinal)+1;
            int num2 = inputFile.LastIndexOf(".", StringComparison.Ordinal);
            string str = inputFile.Substring(num,  num2 - num);
            
            string checkEng = EngAlphabet(str);

            Format fmr = parseFormat(format);
            
            string finalstr = CheckIfAvailableName(GetFileDir(inputFile), checkEng, fmr);
            return finalstr;
        }
        
        private string EngAlphabet(string tit)
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
                return $"{j} ({num})";
            }
            
            return $"{str} ({num})";
        }
        
        private Format parseFormat(string format)
        {
            if (!(String.IsNullOrEmpty(format)))
            {
                Format dir = (Format)Enum.Parse(typeof(Format), format,true);
                return dir;
            }
            throw new NullReferenceException("string can not be null");
        }

        private void Engine_ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            int percentComplete = (int)Math.Round((double)(100 * (double)e.ProcessedDuration.Ticks) / e.TotalDuration.Ticks);

            Console.WriteLine("{0}%", percentComplete.ToString());
        }
    }
}
