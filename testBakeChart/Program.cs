using System;
using bakeChart;

namespace testBakeChart
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadVotes = new DownloadVotes("http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37218,id,idg.html");
            var downloadVotes2 = new DownloadVotes("http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37188,id,idg.html");

            var parsed = downloadVotes.DownloadVotes_Parse();
            var parsed2 = downloadVotes2.DownloadVotes_Parse();

            Console.WriteLine(parsed);
            Console.WriteLine(parsed2);

            Console.ReadLine();
        }
    }
}
