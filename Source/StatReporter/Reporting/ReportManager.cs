using HtmlAgilityPack;
using NLog;
using StatReporter.Core;
using StatReporter.Scraping;
using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatReporter.Reporting
{
    public class ReportManager : IReportManager
    {
        private string[] filesPaths;
        private ILogFactory logFactory;
        private List<Report> reports;

        public ReportManager(string[] htmlFiles, ILogFactory logFactory)
        {
            filesPaths = htmlFiles;
            this.logFactory = logFactory;
        }

        public Report[] Results
        {
            get { return reports.ToArray(); }
        }

        public void Start()
        {
            reports = new List<Report>();

            HtmlDocument[] htmlDocs = CreateDocuments();
            var messages = GetMessages(htmlDocs);
            var mergedMessages = Merge(messages);

            IReportGenerator rg = new AllContributionsByMonthReport(mergedMessages);
            var report = rg.GenerateAsync().Result;
            reports.Add(report);
        }

        private HtmlDocument CreateDocument(object obj)
        {
            string filePath = obj as string;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(filePath);

            return htmlDoc;
        }

        private async Task<HtmlDocument> CreateDocumentAsync(string fileName)
        {
            return await Task.Factory.StartNew(CreateDocument, fileName);
        }

        private HtmlDocument[] CreateDocuments()
        {
            var tasksList = new List<Task<HtmlDocument>>();

            foreach (string htmlFileName in filesPaths)
            {
                var task = CreateDocumentAsync(htmlFileName);
                tasksList.Add(task);
            }

            var htmlDocs = Task.WhenAll(tasksList).Result;

            return htmlDocs;
        }

        private MessageMetaData[] GetMessages(object state)
        {
            var args = state as Tuple<HtmlDocument, Logger>;
            var htmlDoc = args.Item1;
            var logger = args.Item2;

            HtmlScraper scraper = new HtmlScraper(logger, htmlDoc);
            var messages = scraper.Scrape();

            return messages;
        }

        private MessageMetaData[][] GetMessages(HtmlDocument[] htmlDocs)
        {
            var tasksList = new List<Task<MessageMetaData[]>>();
            var logger = logFactory.CreateLogger(typeof(HtmlScraper));
            Task<MessageMetaData[]> task;

            foreach (var doc in htmlDocs)
            {
                task = GetMessagesAsync(doc, logger);
                tasksList.Add(task);
            }

            var result = Task.WhenAll(tasksList).Result;

            return result;
        }

        private async Task<MessageMetaData[]> GetMessagesAsync(HtmlDocument htmlDoc, Logger logger)
        {
            var state = new Tuple<HtmlDocument, Logger>(htmlDoc, logger);
            return await Task.Factory.StartNew(GetMessages, state);
        }

        private MessageMetaData[] Merge(MessageMetaData[][] messages)
        {
            var mergedMessages = new HashSet<MessageMetaData>();
            MessageMetaData[] resultArray;

            for (int i = 0; i < messages.Length; i++)
                for (int j = 0; j < messages[i].Length; j++)
                    mergedMessages.Add(messages[i][j]);

            resultArray = new MessageMetaData[mergedMessages.Count];
            mergedMessages.CopyTo(resultArray, 0);

            return resultArray;
        }
    }
}