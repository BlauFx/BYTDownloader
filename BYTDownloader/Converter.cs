using System;
using System.IO;
using System.Reflection;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace BYTDownloader
{
    class Converter
    {
        public Converter()
        {
            Console.Title = "Converter";

            Console.Write("Input file: ");

            string x = Console.ReadLine();

            var inputFile = new MediaFile { Filename = x };

            Console.Write("Fomat: ");

            var outputFile = new MediaFile { Filename = string.Format(@"{0}\output.{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Console.ReadLine()) };

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
                Console.WriteLine("100%");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("--------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Could not create new file\n");
                Console.WriteLine("Possible errors: \ninput file does not exist / is not an audio/video file\nformat is wrong");
            }
        }

        private static void Engine_ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            int percentComplete = (int)Math.Round((double)(100 * (double)e.ProcessedDuration.Ticks) / e.TotalDuration.Ticks);

            Console.WriteLine("{0}%", percentComplete.ToString());
        }
    }
}
