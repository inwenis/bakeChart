using System;
using System.IO;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;

namespace bakeChart
{
    public class DownloadVotes
    {
        private string _url;

        public DownloadVotes(string url)
        {
            _url = url;
        }

        public string DownloadVotes_Parse()
        {
            Console.WriteLine(DateTimeOffset.UtcNow + ": will now download data");
            var result = Download();
            return Parse(result);
        }

        private string Download()
        {
            var httpClient = new HttpClient();
            var result = httpClient.GetStringAsync(_url).Result;
            return result;
        }

        public static string Parse(string result)
        {
            var xPathToAreaName = "//*[@id=\"gtsms_sms-wyniki\"]/h2";
            var xPathToCompetitorListItem = "//*[@id=\"gtsms_sms-wyniki\"]/ul/li";
            StringReader reader = new StringReader(result);
            var doc = new HtmlDocument();
            doc.Load(reader);

            var areaName = doc.DocumentNode.SelectNodes(xPathToAreaName)[0].InnerText;

            var nodes = doc.DocumentNode.SelectNodes(xPathToCompetitorListItem);
            var stringBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                var name = node.SelectNodes("./span[2]/span/span[1]/strong")[0].InnerText;
                var count = node.SelectNodes("./span[2]/span/span[2]/text()")[0].InnerText;
                count = count.Trim();
                stringBuilder.AppendLine(name + "\t" + areaName + "\t" + count);
            }

            return stringBuilder.ToString();
        }
    }
}