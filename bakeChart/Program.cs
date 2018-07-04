using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using HtmlAgilityPack;

namespace bakeChart
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("outs");
            var timer = new Timer(new TimerCallback(DoIt), null, 0, 10 * 60 * 1000);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        private static void DoIt(object state)
        {
            try
            {
                Console.WriteLine(DateTimeOffset.UtcNow);
                var httpClient = new HttpClient();
                var result = httpClient
                    .GetStringAsync("http://hermes.gratka-technologie.pl/glosowanie/wyniki/66482,37218,id,idg.html").Result;
                //Console.WriteLine(result);
                //var path = "//*[@id=\"gtsms_sms-wyniki\"]/ul/li[1]/span[2]/span/span[2]";
                var path = "//*[@id=\"gtsms_sms-wyniki\"]/ul/li";

                File.WriteAllText("out.txt", result);
                var doc = new HtmlDocument();
                doc.Load("out.txt");
                var nodes = doc.DocumentNode.SelectNodes(path);
                var sb = new StringBuilder();
                foreach (var node in nodes)
                {
                    var name = node.SelectNodes("./span[2]/span/span[1]/strong")[0].InnerText;
                    var count = node.SelectNodes("./span[2]/span/span[2]/text()")[0].InnerText;
                    count = count.Trim();
                    sb.AppendLine(name + "\t" + count);
                }

                var fileName = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss") + ".txt";
                File.WriteAllText(Path.Combine("outs", fileName), sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
