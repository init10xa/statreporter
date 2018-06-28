using HtmlAgilityPack;
using NLog;
using StatReporter.Core;
using StatReporter.Reporting;
using StatReporter.Scraping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StatReporter
{
    internal class Program
    {
        private static readonly int NumberOfTotalBlocks = 20;
        private static int BlockPercentage;
        private static string EmptyBlockString = "-";
        private static string FullBlockString = "#";

        private static void Main(string[] args)
        {
            Debug.Assert(args.Length > 0, "args.Length > 0 - at least one argument should be passed to the program.");
            if (args.Length == 0)
            {
                Console.WriteLine("File path should be provided to the program.");
                return;
            }

            BlockPercentage = 100 / NumberOfTotalBlocks;

            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            string fileName = args[0];
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(fileName);

            var logger = LogManager.GetLogger(typeof(HtmlScraper).FullName);
            HtmlScraper scraper = new HtmlScraper(logger, htmlDoc);
            scraper.ProgressChanged += OnProgressChanged;
            var messages = scraper.Scrape();

            IReportGenerator rg = new AllContributionsByHourReport(messages);
            var report = rg.GenerateAsync().Result;

            foreach (var section in report.Sections)
            {
                foreach (var item in section.Content)
                    Console.WriteLine($"{item[0]}\t: {item[1]}");
            }

            Console.WriteLine();
            //Console.WriteLine(sb);

            Console.Write("Done!");
            Console.CursorVisible = true;
            Console.ReadKey();
        }

        private static void OnProgressChanged(object sender, ProgressEventArgs e)
        {
            ShowProgress(e.Progress, e.Message);
        }

        private static void ShowProgress(float progress, string message)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write(message);
            Console.SetCursorPosition(0, 1);
            Console.Write($"{progress:F0}%");
            Console.SetCursorPosition(6, 1);
            int fullCellsCount = (int)(Math.Floor(progress) + 1) / BlockPercentage;
            int emptyCellsCount = NumberOfTotalBlocks - fullCellsCount;

            Console.Write("[");

            for (int i = 0; i < fullCellsCount; i++)
                Console.Write(FullBlockString);

            for (int i = 0; i < emptyCellsCount; i++)
                Console.Write(EmptyBlockString);

            Console.WriteLine("]");
        }
    }
}