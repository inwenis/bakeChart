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
            var timer = new Timer(new TimerCallback(RefreshChart), null, 0, 10 * 60 * 1000);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        static void RefreshChart(object state)
        {
            Console.WriteLine(DateTimeOffset.UtcNow + " will now refresh chart");
            var allPoints = ReadDataFromFiles(@"c:\git\bakeChart\bakeChart\bin\Debug\outs");

            Dictionary<string, List<Point>> dictionary = allPoints
                .GroupBy(x => x.CompetitorName)
                .ToDictionary(x => x.Key, x => x.ToList());

            RemoveEntreisWhereAllValuesAreZero(dictionary);

            var allPointsWithOutLoosers = dictionary.SelectMany(x => x.Value);

            var areaName_Competitor_Points = new Dictionary<string, Dictionary<string, List<Point>>>();

            var grouppedByArea = allPointsWithOutLoosers
                .GroupBy(x => x.AreaName);

            foreach (var areaGroup in grouppedByArea)
            {
                var dic = areaGroup
                    .GroupBy(x => x.CompetitorName)
                    .ToDictionary(y => y.Key, y => y.ToList());
                areaName_Competitor_Points.Add(areaGroup.Key, dic);
            }

            var powWejherowskiDic = areaName_Competitor_Points["Mistrzowie Smaku - Cukiernia/Kawiarnia Roku 2018 (powiat wejherowski)"];
            var tortowyZascianekKey = "Tortowy Zaścianek Bogna Nadolska, Bojano, ul. Czynu 1000-lecia 8";
            AddMissingPoints(powWejherowskiDic, tortowyZascianekKey);
            OrderPointsByDateTime(powWejherowskiDic);
            Only50PointsShallRemainForEachCompetitor(powWejherowskiDic, tortowyZascianekKey);
            DoChart(powWejherowskiDic, tortowyZascianekKey, "wejherowski");

            if (File.Exists(@"C:\inetpub\wwwroot\out.html"))
            {
                File.Copy("wejherowski.html", @"C:\inetpub\wwwroot\out.html", true);
            }

            var bestCompetitorsFromAreas = allPoints
                .GroupBy(x => x.AreaName)
                .Select(areaGroup =>
                {
                    var groupByComp = areaGroup.GroupBy(x => x.CompetitorName);
                    var bestCompetitorFromThisArea = groupByComp
                        .Select(x => new {Competitor = x.Key, MaxVotes = x.Max(y => y.Value)})
                        .OrderBy(x => x.MaxVotes)
                        .Last();
                    return bestCompetitorFromThisArea.Competitor;
                });

            var bestOfAllDic = allPoints
                .GroupBy(x => x.CompetitorName)
                .Where(x => bestCompetitorsFromAreas.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.ToList());

            UseOnlyPointsNewerThan(bestOfAllDic, new DateTimeOffset(2018, 07, 09, 21, 40, 0, TimeSpan.Zero));
            AddMissingPoints(bestOfAllDic, tortowyZascianekKey);
            OrderPointsByDateTime(bestOfAllDic);
            Only50PointsShallRemainForEachCompetitor(bestOfAllDic, tortowyZascianekKey);
            DoChart(bestOfAllDic, tortowyZascianekKey, "best");

            if (File.Exists(@"C:\inetpub\wwwroot\out.html"))
            {
                File.Copy("best.html", @"C:\inetpub\wwwroot\best.html", true);
            }
        }

        private static void UseOnlyPointsNewerThan(Dictionary<string, List<Point>> bestOfAllDic, DateTimeOffset threshold)
        {
            foreach (var key in bestOfAllDic.Keys.ToArray())
            {
                bestOfAllDic[key] = bestOfAllDic[key].Where(x => x.DateTime > threshold).ToList();
            }
        }

        private static void Only50PointsShallRemainForEachCompetitor(Dictionary<string, List<Point>> dictionary, string tortowyZascianekKey)
        {
            var max = dictionary[tortowyZascianekKey].Count;
            var howManyPointShouldStay = 50;
            var takeEveryNthPoint = max / howManyPointShouldStay;
            takeEveryNthPoint = takeEveryNthPoint == 0
                ? 1
                : takeEveryNthPoint;
            Console.WriteLine(DateTimeOffset.UtcNow + " there are " + max + " point, I will take every " + takeEveryNthPoint + "nth for the chart");

            foreach (var key in dictionary.Keys.ToArray())
            {
                int c = 0;
                var allPointsForKey = dictionary[key];
                var pointsToPlotOnChart = allPointsForKey.Where(x => c++ % takeEveryNthPoint == 0).ToList();
                //add last point
                if (!pointsToPlotOnChart.Contains(allPointsForKey.Last()))
                {
                    pointsToPlotOnChart.Add(allPointsForKey.Last());
                }

                dictionary[key] = pointsToPlotOnChart;
            }
        }

        private static void DoChart(Dictionary<string, List<Point>> dictionary, string tortowyZascianekKey, string s)
        {
            var labels = dictionary[tortowyZascianekKey]
                .Select(x =>"'" + x.DateTime.ToOffset(TimeSpan.FromHours(2)).ToString("dd MMM  HH:mm", new CultureInfo("pl-PL")) + "'")
                .Aggregate((a, b) => a + "," + b);

            var allDataSetsJoined = dictionary
                .Select(x => DatasetForCompetitor(x.Key, x.Value))
                .Aggregate((a, b) => a + "\n" + b);
            var allText = File.ReadAllText("chartTemplate.html");
            var replace = allText
                .Replace("XXXLabelsXXX", labels)
                .Replace("XXXDataSetsXXX", allDataSetsJoined);
            File.WriteAllText(s + ".html", replace, Encoding.UTF8);
            Console.WriteLine(DateTimeOffset.UtcNow + " done refreshing chart");
        }

        private static void OrderPointsByDateTime(Dictionary<string, List<Point>> dictionary)
        {
            var keys = dictionary.Keys.ToArray();
            
            foreach (var key in keys)
            {
                dictionary[key] = dictionary[key].OrderBy(x => x.DateTime).ToList();
            }
        }

        private static void RemoveEntreisWhereAllValuesAreZero(Dictionary<string, List<Point>> dictionary)
        {
            var dictionaryKeys = dictionary.Keys.ToArray();
            var keysToBeRemoved = dictionaryKeys.Where(x => dictionary[x].All(p => p.Value == 0));
            foreach (var key in keysToBeRemoved)
            {
                dictionary.Remove(key);
            }
        }

        private static void AddMissingPoints(Dictionary<string, List<Point>> dictionary, string tortowyZascianekKey)
        {
            var goodPoints = dictionary[tortowyZascianekKey];
            foreach (var keyValuePair in dictionary)
            {
                var points = keyValuePair.Value;
                foreach (var goodPoint in goodPoints)
                {
                    if (points.Any(p => p.DateTime == goodPoint.DateTime))
                    {
                        continue;
                    }
                    else
                    {
                        points.Add(new Point(){DateTime = goodPoint.DateTime, Value = 0});
                    }
                }
            }
        }

        private static string DatasetForCompetitor(string name, List<Point> points)
        {
            var data = points
                .OrderBy(x => x.DateTime)
                .Select(p => p.Value.ToString())
                .Aggregate((a, b) => a + "," + b);

            var colorNames = new [] {"red","orange","yellow","green","blue","grey"};
            var colorName = colorNames[random.Next(colorNames.Length)];

            if (name.Contains("Tortowy"))
            {
                colorName = "purple";
            }

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("label: '" + name.Substring(0, name.IndexOf(',')) + "',");
            sb.AppendLine("backgroundColor: window.chartColors." + colorName + ",");
            sb.AppendLine("borderColor: window.chartColors." + colorName + ",");
            sb.AppendLine("data: [");
            sb.AppendLine(data);
            sb.AppendLine("    ],");
            sb.AppendLine("fill: false,");
            sb.AppendLine("},");

            return sb.ToString();
        }

        private static List<Point> ReadDataFromFiles(string pathToDirectoryWithFiles)
        {
            var allPoints = new List<Point>();
            var files = Directory.GetFiles(pathToDirectoryWithFiles, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var dateTimeFromFileName = DateTimeOffset.ParseExact(
                    Path.GetFileNameWithoutExtension(file),
                    "yyyy-MM-ddTHH-mm-ss",
                    new DateTimeFormatInfo(),
                    DateTimeStyles.AssumeUniversal);
                var lines = File.ReadAllLines(file, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var splitted = line.Split('\t');
                    var competitorName = splitted[0];
                    var areaName = splitted[1];
                    var value = int.Parse(splitted[2]);
                    allPoints.Add(new Point
                    {
                        DateTime = dateTimeFromFileName,
                        AreaName = areaName,
                        CompetitorName = competitorName,
                        Value = value
                    });
                }
            }

            return allPoints;
        }
    }

    internal class Point
    {
        public DateTimeOffset DateTime { get; set; }
        public string AreaName { get; set; }
        public string CompetitorName { get; set; }
        public int Value { get; set; }
    }
}
