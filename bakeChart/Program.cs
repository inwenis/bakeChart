using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace bakeChart
{
    class Program
    {
        static void Main(string[] args)
        {
//            var timer = new Timer(new TimerCallback(DownloadAndSaveData), null, 0, 10 * 60 * 1000);
            var timer = new Timer(new TimerCallback(DownloadAndSaveData), null, 0, 10 * 1000);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        private static void DownloadAndSaveData(object state)
        {
            try
            {
                var urls = new List<string>(){
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37192,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37194,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37196,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37198,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37200,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37202,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37204,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37206,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37208,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37210,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37212,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37214,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37216,id,idg.html",
                    "http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37218,id,idg.html"
                };

                var results = urls
                    .Select(x => new DownloadVotes(x))
                    .Select(x => x.DownloadVotes_Parse());
                var allResults = string.Join("", results);
                var fullFileName = SaveToFile(allResults);
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
