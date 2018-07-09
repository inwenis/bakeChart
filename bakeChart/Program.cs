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
                DownloadVotes.DownloadVotes_Parse_SaveToFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
