using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace bakeChart.Charting
{
    class Program
    {
        static void Main(string[] args)
        {
            var dic = ReadDataFromFiles(@"c:\git\bakeChart\bakeChart\bin\Debug\outs");
            var polylines = ToPolylines(dic);
            var allPolylinesJoined = string.Join("\n", polylines);
            var chartTemplate = File.ReadAllText("chartTemplate.html");
            var chart = chartTemplate.Replace("XXXPolylinesXXX", allPolylinesJoined);
            File.WriteAllText("out.html", chart);
        }

        private static Dictionary<string, List<Point>> ReadDataFromFiles(string pathToDirectoryWithFiles)
        {
            var dic = new Dictionary<string, List<Point>>();
            var files = Directory.GetFiles(pathToDirectoryWithFiles);
            foreach (var file in files)
            {
                var dateTimeFromFileName = DateTimeOffset.ParseExact(Path.GetFileNameWithoutExtension(file),
                    "yyyy-MM-ddThh-mm-ss", new DateTimeFormatInfo());
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var splitted = line.Split('\t');
                    var competitorName = splitted[0];
                    var value = int.Parse(splitted[1]);
                    if (!dic.ContainsKey(competitorName))
                    {
                        dic.Add(competitorName, new List<Point>());
                    }

                    dic[competitorName].Add(new Point {DateTime = dateTimeFromFileName, Value = value});
                }
            }

            return dic;
        }

        private static IEnumerable<string> ToPolylines(Dictionary<string, List<Point>> dic)
        {
            var polylineWithPlaceholders = "<polyline fill=\"none\" stroke=\"#XXXXColorXXX\" stroke-width=\"1\" points=\"XXXPointsXXX\"/>";
            var random = new Random(DateTimeOffset.UtcNow.Millisecond);

            foreach (var keyValuePair in dic)
            {
                var points = keyValuePair.Value;
                
                Color randomColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                string randomColorAsHex = randomColor.R.ToString("X2") + randomColor.G.ToString("X2") + randomColor.B.ToString("X2");
                
                var polylinePoints = points
                    .OrderBy(point => point.DateTime)
                    .Select(point =>
                    {
                        var x = ((point.DateTime.UtcTicks - 636659278810000000) / 100000000).ToString();
                        var y = point.Value;
                        return x + "," + y;
                    })
                    .Aggregate((a, b) => a + " " + b);

                var polylineFilled = polylineWithPlaceholders
                    .Replace("XXXXColorXXX", randomColorAsHex)
                    .Replace("XXXPointsXXX", polylinePoints);
                yield return polylineFilled;
            }
        }
    }

    internal class Point
    {
        public DateTimeOffset DateTime { get; set; }
        public int Value { get; set; }
    }
}
