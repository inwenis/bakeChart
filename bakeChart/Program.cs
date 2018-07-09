using System;
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
                x.DownloadVotes_Parse_SaveToFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
