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

            string x = Fixinput(Console.ReadLine());
            var inputFile = new MediaFile(x);

            Console.Write("Fomat: ");

            var format = Console.ReadLine();
            var outputFile = new MediaFile($"{Path.GetDirectoryName(x)}\\{GetOutputName(x, format)}.{format}");

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
                Console.WriteLine("Possible errors: \ninput file does not exist / is not an audio/video file\nformat is wrong or not supported");
            }
        }

        private string Fixinput(string input)
        {
            if (input.StartsWith("\""))
                input = input[1..];

            if (input.EndsWith("\""))
                input = input[0..^1];

            return input;
        }

        private string GetOutputName(string inputFile, string format)
        {
            int num = inputFile.LastIndexOf("\\", StringComparison.Ordinal) + 1;
            int num2 = inputFile.LastIndexOf(".", StringComparison.Ordinal);
            string name = inputFile[num..num2];

            return SharedMethods.CheckIfAvailableName(Path.GetDirectoryName(inputFile), name, ParseFormat(format));
        }

        private Format ParseFormat(string format)
            => string.IsNullOrEmpty(format) != true ? (Format)Enum.Parse(typeof(Format), format, true) : throw new NullReferenceException("string can not be null");

        private void Engine_ConvertProgressEvent(object sender, ConversionProgressEventArgs e)
            => Console.WriteLine("{0}%", ((int)Math.Round(100 * (double)e.ProcessedDuration.Ticks / e.TotalDuration.Ticks)).ToString());
    }
}
