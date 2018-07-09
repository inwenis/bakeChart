using System;
using System.IO;
using System.Threading;

namespace bakeChart
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = new Timer(new TimerCallback(DownloadAndSaveData), null, 0, 10 * 60 * 1000);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        private static void DownloadAndSaveData(object state)
        {
            try
            {
                var x = new DownloadVotes("http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37218,id,idg.html");
                var parsed = x.DownloadVotes_Parse();
                var fullFileName = SaveToFile(parsed);
                Console.WriteLine(DateTimeOffset.UtcNow + ": data written to: " + fullFileName);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string SaveToFile(string resultParsed)
        {
            var now = DateTimeOffset.UtcNow;
            var fileName = now.ToString("yyyy-MM-ddTHH-mm-ss") + ".txt";
            var outputDirecotry = Path.Combine("outs",
                now.Year.ToString("0000"),
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"));
            Directory.CreateDirectory(outputDirecotry);
            var fullFileName = Path.Combine(outputDirecotry, fileName);
            File.WriteAllText(fullFileName, resultParsed);
            return fullFileName;
        }
    }
}
