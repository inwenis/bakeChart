using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bakeChart;

namespace testBakeChart
{
    class Program
    {
        static void Main(string[] args)
        {
            var contant = File.ReadAllText(@"c:\stuff\Hermes.html");
            var parsed = DownloadVotes.Parse(contant);
            Console.WriteLine(parsed);
            Console.ReadLine();
        }
    }
}
