using System;
using System.Globalization;
using System.IO;

namespace migrateOldOutputToNew
{
    class Program
    {
        static void Main(string[] args)
        {
            var outsPath = args[0];
            var files = Directory.GetFiles(outsPath);
            foreach (var file in files)
            {
                var dateTimeFromFileName = DateTimeOffset.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyy-MM-ddTHH-mm-ss", new DateTimeFormatInfo());
                var outputDirecotry = Path.Combine(
                    dateTimeFromFileName.Year.ToString("0000"),
                    dateTimeFromFileName.Month.ToString("00"),
                    dateTimeFromFileName.Day.ToString("00"),
                    dateTimeFromFileName.Hour.ToString("00"));
                Directory.CreateDirectory(outputDirecotry);
                File.Copy(file, Path.Combine(outputDirecotry, Path.GetFileName(file)));
                File.Delete(file);
            }
        }
    }
}
