using StatReporter.Reporting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StatReporter
{
    internal class Program
    {
        private static readonly string HtmlFileNamePattern = @"^[\w\-. \[\]]+.html$";

        public static void Main(string[] args)
        {
            if (!VerfiyArguments(args))
                return;

            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            var repManager = new ReportManager(args, new StatReporter.Core.LogFactory());
            repManager.Start();
            var report = repManager.Results[0];

            foreach (var section in report.Sections)
            {
                foreach (var item in section.Content)
                    Console.WriteLine($"{item[0]}\t: {item[1]}");
            }

            Console.WriteLine();
            Console.Write("Done!");

            Console.CursorVisible = true;
            Console.ReadKey();
        }

        private static bool VerfiyArguments(string[] args)
        {
            Debug.Assert(args.Length > 0, "args.Length > 0 - at least one argument should be passed to the program.");
            if (args.Length == 0)
            {
                Console.WriteLine("File path should be provided to the program.");
                return false;
            }

            if (!VerifyHtmlFiles(args))
                return false;

            return true;
        }

        private static bool VerifyHtmlFiles(string[] filePaths)
        {
            bool doesMatch;
            string fileName;

            foreach (var path in filePaths)
            {
                fileName = Path.GetFileName(path);
                doesMatch = Regex.IsMatch(fileName, HtmlFileNamePattern);

                Debug.Assert(doesMatch, $"doesMath - file '{fileName}'is a not a valid HTML.");
                if (!doesMatch)
                {
                    Console.WriteLine($"file '{fileName}'is a not a valid HTML.");
                    return false;
                }
            }

            return true;
        }
    }
}