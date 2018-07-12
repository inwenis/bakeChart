using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace migrateOldOutputToNew
{
    class Program
    {
        static void Main(string[] args)
        {
            var outsPath = args[0];
            var files = Directory.GetFiles(outsPath, "*", SearchOption.AllDirectories);


            var groupsByDay = files.GroupBy(x =>
            {
                var datetimeFromFileName = DateTimeOffset.ParseExact(Path.GetFileNameWithoutExtension(x),
                    "yyyy-MM-ddTHH-mm-ss", new DateTimeFormatInfo());
                return datetimeFromFileName.Year.ToString() + datetimeFromFileName.Month.ToString() +
                       datetimeFromFileName.Day.ToString();
            });

            foreach (var groupByDay in groupsByDay.OrderBy(x => x.Key).Skip(1).Reverse().Skip(1).Where(x => x.Count() > 144))
            {
                Console.WriteLine(groupByDay.Key);
                Console.WriteLine("groupByDay.Count() = " + groupByDay.Count());
                var takeEventNth = groupByDay.Count() / (24 * 6);
                var index = 0;
                var enumerable = groupByDay.Select(x =>
                    {
                        return new
                        {
                            file = x,
                            dt = DateTimeOffset.ParseExact(Path.GetFileNameWithoutExtension(x), "yyyy-MM-ddTHH-mm-ss",
                                new DateTimeFormatInfo())
                        };
                    })
                    .OrderBy(x => x.dt)
                    .Where(x => x.dt.Day != DateTimeOffset.Now.Day);

                enumerable
                    .Where(x => index++ % takeEventNth != 0)
                    .Select(x => {
                        Console.WriteLine("would remove " + x.file);
                        File.Delete(x.file);
                        return 1;
                    })
                    .ToArray();

                index = 0;
                enumerable
                    .Where(x => index++ % takeEventNth == 0)
                    .Select(x => { Console.WriteLine("would stay " + x.file);
                        return 1;
                    })
                    .ToArray();
            }
        }
    }
}
