using HtmlAgilityPack;
using NLog;
using StatReporter.Core;
using StatReporter.Reporting;
using StatReporter.Scraping;
using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StatReporter
{
    internal class Program
    {
        private static readonly string HtmlFileNamePattern = @"^[\w\-. \[\]]+.html$";
        private static readonly int NumberOfTotalBlocks = 20;
        private static int BlockPercentage;
        private static string EmptyBlockString = "-";
        private static string FullBlockString = "#";
        private static List<IProgressable> Progressables;

        public static void Main(string[] args)
        {
            if (!VerfiyArguments(args))
                return;

            BlockPercentage = 100 / NumberOfTotalBlocks;

            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            HtmlDocument[] htmlDocs = CreateDocuments(args);

            Progressables = new List<IProgressable>();
            var messages = GetMessages(htmlDocs);

            IReportGenerator rg = new AllContributionsByMonthReport(messages[0]);
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

        private static HtmlDocument CreateDocument(object obj)
        {
            string filePath = obj as string;
            string fileName = Path.GetFileName(filePath);

            Console.WriteLine($"Loading file:\t{fileName}");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(filePath);

            return htmlDoc;
        }

        private static async Task<HtmlDocument> CreateDocumentAsync(string fileName)
        {
            return await Task.Factory.StartNew(CreateDocument, fileName);
        }

        private static HtmlDocument[] CreateDocuments(string[] args)
        {
            var tasksList = new List<Task<HtmlDocument>>();

            foreach (string htmlFileName in args)
            {
                var task = CreateDocumentAsync(htmlFileName);
                tasksList.Add(task);
            }

            var htmlDocs = Task.WhenAll(tasksList).Result;

            return htmlDocs;
        }

        private static MessageMetaData[][] GetMessages(HtmlDocument[] htmlDocs)
        {
            var tasksList = new List<Task<MessageMetaData[]>>();
            var logger = LogManager.GetLogger(typeof(HtmlScraper).FullName);
            Task<MessageMetaData[]> task;

            foreach (var doc in htmlDocs)
            {
                task = GetMessagesAsync(doc, logger);
                tasksList.Add(task);
            }

            var result = Task.WhenAll(tasksList).Result;

            return result;
        }

        private static MessageMetaData[] GetMessages(object state)
        {
            var args = state as Tuple<HtmlDocument, Logger>;
            var htmlDoc = args.Item1;
            var logger = args.Item2;

            HtmlScraper scraper = new HtmlScraper(logger, htmlDoc);
            SubscribeForChange(scraper);
            var messages = scraper.Scrape();

            return messages;
        }

        private static async Task<MessageMetaData[]> GetMessagesAsync(HtmlDocument htmlDoc, Logger logger)
        {
            var state = new Tuple<HtmlDocument, Logger>(htmlDoc, logger);
            return await Task.Factory.StartNew(GetMessages, state);
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

        private static void SubscribeForChange(IProgressable subject)
        {
            subject.ProgressChanged += OnProgressChanged;
            Progressables.Add(subject);
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