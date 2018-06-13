using HtmlAgilityPack;
using NLog;
using StatReporter.Scraping;
using System;
using System.Diagnostics;

namespace StatReporter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Debug.Assert(args.Length > 0, "args.Length > 0 - at least one argument should be passed to the program.");
            if (args.Length == 0)
            {
                Console.WriteLine("File path should be provided to the program.");
                return;
            }

            string fileName = args[0];

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(fileName);

            //var nodes = htmlDoc.DocumentNode.CssSelect("div[class=im_message_text]");

            //foreach (var node in nodes)
            //    Console.WriteLine($"message is: {node.InnerText}");

            var logger = LogManager.GetLogger(typeof(HtmlScraper).FullName);
            HtmlScraper scraper = new HtmlScraper(logger, htmlDoc);
            var result = scraper.Scrape();

            foreach (var item in result)
                Console.WriteLine($"{item.Sender.Name} at {item.Timestamp}");

            Console.Write("Done!");
            Console.ReadKey();
        }
    }
}