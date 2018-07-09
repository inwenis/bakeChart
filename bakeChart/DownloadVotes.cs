using System;
using System.IO;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;

namespace bakeChart
{
    class DownloadVotes
    {
        public static void DownloadVotes_Parse_SaveToFile()
        {
            Console.WriteLine(DateTimeOffset.UtcNow + ": will now download data");
            var result = Download();
            var resultParsed = Parse(result);
            var fullFileName = SaveToFile(resultParsed);
            Console.WriteLine(DateTimeOffset.UtcNow + ": data written to: " + fullFileName);
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

        private static string Parse(string result)
        {
            var xPath = "//*[@id=\"gtsms_sms-wyniki\"]/ul/li";
            StringReader reader = new StringReader(result);
            var doc = new HtmlDocument();
            doc.Load(reader);
            var nodes = doc.DocumentNode.SelectNodes(xPath);
            var stringBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                var name = node.SelectNodes("./span[2]/span/span[1]/strong")[0].InnerText;
                var count = node.SelectNodes("./span[2]/span/span[2]/text()")[0].InnerText;
                count = count.Trim();
                stringBuilder.AppendLine(name + "\t" + count);
            }

            return stringBuilder.ToString();
        }

        private static string Download()
        {
            var httpClient = new HttpClient();
            var result = httpClient
                .GetStringAsync("http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37218,id,idg.html").Result;
            return result;
        }
    }
}