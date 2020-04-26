using FFmpeg.NET;
using FFmpeg.NET.Enums;
using FFmpeg.NET.Events;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace BYTDownloader
{
    class Converter
    {
        public Converter()
        {
            Console.Title = "Converter";
            Console.Write("Input file: ");

            string x = Console.ReadLine();
            string x2 = Fixinput(x);

            var inputFile = new MediaFile(x2);
            Console.Write("Fomat: ");

            var format = Console.ReadLine();
            var outputFile = new MediaFile($"{GetFileDir(x2)}\\{GetOutputName(x2, format)}.{format}");

            var conversionOptions = new ConversionOptions
            {
                VideoAspectRatio = VideoAspectRatio.Default,
                VideoSize = VideoSize.Custom,
                AudioSampleRate = AudioSampleRate.Hz48000,
            };

            var ffmpeg = new Engine($"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\ffmpeg.exe");
            ffmpeg.Progress += Engine_ConvertProgressEvent;
            _ = ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions).Result;

            Console.ForegroundColor = ConsoleColor.Red;

            if (File.Exists(outputFile.FileInfo.FullName))
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
            if(input.StartsWith("\""))
                input = input[1..];

            if (input.EndsWith("\""))
                input = input[0..^1];

            return input;
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
            string str = inputFile.Substring(num, num2 - num);
            
            string checkEng = SharedMethods.ENGAlphabet(str);

            Format fmr = ParseFormat(format);
            
            string finalstr = SharedMethods.CheckIfAvailableName(GetFileDir(inputFile), checkEng, fmr);
            return finalstr;
        }
        
        private Format ParseFormat(string format)
        {
            if (!(string.IsNullOrEmpty(format)))
            {
                Format dir = (Format)Enum.Parse(typeof(Format), format,true);
                return dir;
            }
            throw new NullReferenceException("string can not be null");
        }

        private void Engine_ConvertProgressEvent(object sender, ConversionProgressEventArgs e)
            => Console.WriteLine("{0}%", ((int)Math.Round(100 * (double)e.ProcessedDuration.Ticks / e.TotalDuration.Ticks)).ToString());
    }
}
