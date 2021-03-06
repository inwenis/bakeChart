﻿using System.IO;
using System.Text;

namespace fixEncodingInOutputs
{
    class Program
    {
        static void Main(string[] args)
        {
            FixFiles(args);
        }

        private static void FixFiles(string[] args)
        {
            var outsPath = args[0];
            var files = Directory.GetFiles(outsPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                var fixedContant = FixConect(content);
                File.WriteAllText(file, fixedContant, Encoding.UTF8);
            }
        }

        private static string FixConect(string content)
        {
            return content.Replace("\t", "\tMistrzowie Smaku - Cukiernia/Kawiarnia Roku 2018 (powiat wejherowski)\t");
        }
    }
}
