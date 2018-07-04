using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace bakeChart.Charting
{
    class Program
    {
        static Random random = new Random(((int) DateTimeOffset.UtcNow.Ticks));


        static void Main(string[] args)
        {
            var timer = new Timer(new TimerCallback(DoIt), null, 0, 10 * 60 * 1000);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        static void DoIt(object state)
        {
            var dictionary = ReadDataFromFiles(@"c:\git\bakeChart\bakeChart\bin\Debug\outs");
            var max = dictionary.Values.Select(x => x.Count).Max();
            var competitorWithMaxEntires = dictionary.Keys.First(key => dictionary[key].Count == max);
            var howManyPointShouldStay = 50;
            var takeEveryNthPoint = max / howManyPointShouldStay;
            Console.WriteLine(max);
            Console.WriteLine(takeEveryNthPoint);
            var dictionaryKeys = dictionary.Keys.ToArray();
            foreach (var key in dictionaryKeys)
            {
                //take every 4th entry
                int c = 0;
                dictionary[key] = dictionary[key].Where(x => c++ % takeEveryNthPoint == 0).ToList().ToList();
            }

            var labels = dictionary[dictionaryKeys.First(k => k.Contains("Tortowy"))]
                .Select(x => "'" + x.DateTime.ToLocalTime().ToString("dd MMM  HH:mm") + "'")
                .Aggregate((a,b) => a + "," + b);

            var allDataSetsJoined = dictionary.Select(x => DatasetForCompetitor(x.Key, x.Value)).Aggregate((a, b) => a + "\n" + b);
            var allText = File.ReadAllText("chartTemplate.html");
            var replace = allText
                .Replace("XXXLabelsXXX", labels)
                .Replace("XXXDataSetsXXX", allDataSetsJoined);

            try
            {
                File.WriteAllText(@"out.html", replace, Encoding.UTF8);
                File.WriteAllText(@"C:\inetpub\wwwroot\out.html", replace, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string DatasetForCompetitor(string name, List<Point> points)
        {
            var data = points
                .OrderBy(x => x.DateTime)
                .Select(p => p.Value.ToString())
                .Aggregate((a, b) => a + "," + b);

            var colorNames = new [] {"red","orange","yellow","green","blue","purple","grey"};
            var colorName = colorNames[random.Next(colorNames.Length)];

            if (name.Contains("Tortowy"))
            {
                colorName = "purple";
            }

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("label: '" + name.Substring(0,17) + "',");
            sb.AppendLine("backgroundColor: window.chartColors." + colorName + ",");
            sb.AppendLine("borderColor: window.chartColors." + colorName + ",");
            sb.AppendLine("data: [");
            sb.AppendLine(data);
            sb.AppendLine("    ],");
            sb.AppendLine("fill: false,");
            sb.AppendLine("},");

            return sb.ToString();
        }

        private static Dictionary<string, List<Point>> ReadDataFromFiles(string pathToDirectoryWithFiles)
        {
            var dictionary = new Dictionary<string, List<Point>>();
            var files = Directory.GetFiles(pathToDirectoryWithFiles);
            foreach (var file in files)
            {
                var dateTimeFromFileName = DateTimeOffset.ParseExact(Path.GetFileNameWithoutExtension(file),
                    "yyyy-MM-ddTHH-mm-ss", new DateTimeFormatInfo());
                var lines = File.ReadAllLines(file, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var splitted = line.Split('\t');
                    var competitorName = splitted[0];
                    var value = int.Parse(splitted[1]);
                    if (!dictionary.ContainsKey(competitorName))
                    {
                        dictionary.Add(competitorName, new List<Point>());
                    }

                    dictionary[competitorName].Add(new Point {DateTime = dateTimeFromFileName, Value = value});
                }
            }

            return dictionary;
        }
    }

    internal class Point
    {
        public DateTimeOffset DateTime { get; set; }
        public int Value { get; set; }
    }
}
