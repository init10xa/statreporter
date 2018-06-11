using HtmlAgilityPack;
using NLog;
using StatReporter.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatReporter.Scraping
{
    public class HtmlScraper
    {
        private readonly string DateStringNodeXOath = "./div/div/span[@class='im_message_date_split_text']";
        private readonly string HistoryMessageItemDivClass = "im_history_message_wrap";
        private readonly string HistoryMessageItemDivXPath = "/html/body/div/div/div/div/div/div/div/div/div/div/div/div/div[@my-message]";
        private readonly string MessageAuthorNodeXPath = "./div/div/div/div/span/a[contains(@class,'im_message_author')]";
        private readonly string MessageBodyItemDivXPath = "./div/div/div/div/div[@my-message-body='historyMessage']";
        private readonly string MessageTimeNodeXPath = "./div/div/div/div/span/span/span[@ng-bind='::historyMessage.date | time']";
        private DateTime currentDate;
        private HtmlDocument doc;
        private ILogger logger;
        private Dictionary<string, User> users;

        public HtmlScraper(ILogger logger, HtmlDocument htmlDoc, IEnumerable<User> users = null)
        {
            doc = htmlDoc;
            this.logger = logger;
            currentDate = DateTime.MinValue;
            CreateUsers(users);
        }

        public MessageMetaData[] Scrape()
        {
            var historyMessages = GetHistoryMessageItems();

            var messages = ExtractMessages(historyMessages);

            return messages;
        }

        private DateTime CreateFullDate(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        private void CreateUsers(IEnumerable<User> users)
        {
            this.users = new Dictionary<string, User>();

            if (users == null)
                return;

            foreach (var user in users)
                this.users.Add(user.Name, user);
        }

        private string ExtractMessageAuthor(HtmlNode msg)
        {
            var messageAuthorNode = msg.SelectSingleNode(MessageAuthorNodeXPath);

            Debug.Assert(messageAuthorNode != null,
                        $"messageAuthorNode != null - message's author node expected to be found at the following path: '{MessageTimeNodeXPath}'.");
            if (messageAuthorNode == null)
            {
                logger.Error($"Could not find message's author node for the current node at the following path: '{MessageTimeNodeXPath}'. " +
                             $"Here is the current node: {Environment.NewLine}{msg.OuterHtml}");
                return string.Empty;
            }

            var authorName = messageAuthorNode.InnerText;
            Debug.Assert(!string.IsNullOrWhiteSpace(authorName),
                        $"!string.IsNullOrWhiteSpace(authorName) - message author's name is expected to be non-empty.");

            return authorName;
        }

        private MessageMetaData[] ExtractMessages(HtmlNode[] historyMessages)
        {
            bool isMessageProcessed;
            var messages = new List<MessageMetaData>();

            foreach (var msg in historyMessages)
            {
                isMessageProcessed = false;
                if (IsUserMessage(msg))
                {
                    MessageMetaData metaData = GetMessageMetaData(msg);
                    messages.Add(metaData);
                    isMessageProcessed = true;
                }
            }

            return messages.ToArray();
        }

        private DateTime ExtractMessageTime(HtmlNode msg)
        {
            var messageTimeNode = msg.SelectSingleNode(MessageTimeNodeXPath);

            Debug.Assert(messageTimeNode != null,
                        $"messageDateNode != null - message's time node expected to be found at the following path: '{MessageTimeNodeXPath}'.");
            if (messageTimeNode == null)
            {
                logger.Error($"Could not find message's time node for the current node at the following path: '{MessageTimeNodeXPath}'. " +
                             $"Here is the current node: {Environment.NewLine}{msg.OuterHtml}");
                return DateTime.MinValue;
            }

            var messageTime = DateTime.MinValue;
            var isTimeParsed = DateTime.TryParse(messageTimeNode.InnerText, out messageTime);

            Debug.Assert(isTimeParsed, "isTimeParsed == true - message's time should be well-formed.");
            if (!isTimeParsed)
                logger.Error($"Message's time ({messageTimeNode.InnerText}) could not be parsed. Here is the current node: { Environment.NewLine}{ msg.OuterHtml}");

            Debug.Assert(messageTime > DateTime.MinValue,
                        "messageTime > DateTime.MinValue - the message time is expected to be a valid time.");

            return messageTime;
        }

        private HtmlNode[] GetHistoryMessageItems()
        {
            Debug.Assert(doc != null, "doc != null - doc cannot be null at this point.");

            var historyItemNodes = new List<HtmlNode>();
            var nodes = doc.DocumentNode.SelectNodes(HistoryMessageItemDivXPath);

            foreach (var node in nodes)
            {
                var nodeClassAttrib = node.Attributes["class"].Value;
                Debug.Assert(!string.IsNullOrWhiteSpace(nodeClassAttrib),
                             "!string.IsNullOrWhiteSpace(nodeClassAttrib) - the node is expected to have class attribute");

                if (nodeClassAttrib.Contains(HistoryMessageItemDivClass))
                {
                    logger.Debug($"found a history message item. {Environment.NewLine} {node.InnerHtml}");
                    historyItemNodes.Add(node);
                }
            }

            Debug.Assert(historyItemNodes.Count > 0,
                         "historyItemNodes.Count > 0 - history list expected to have some items in it.");

            logger.Debug($"Searching history messages finished. found {historyItemNodes.Count} message(s).");
            return historyItemNodes.ToArray();
        }

        private MessageMetaData GetMessageMetaData(HtmlNode msg)
        {
            var metaData = new MessageMetaData();

            string date;
            if (TryExtractDate(msg, out date))
                UpdateCurrentDate(date);

            var messageTime = ExtractMessageTime(msg);
            metaData.Timestamp = CreateFullDate(currentDate, messageTime);
            string messageAuthor = ExtractMessageAuthor(msg);
            metaData.Sender = GetUser(messageAuthor);

            return metaData;
        }

        private User GetUser(string messageAuthor)
        {
            User user;

            if (!users.TryGetValue(messageAuthor, out user))
            {
                user = new User();
                user.Name = messageAuthor;
            }

            return user;
        }

        private bool IsUserMessage(HtmlNode messageNode)
        {
            logger.Debug($"Checking if the node is a user message... {Environment.NewLine}{messageNode.OuterHtml}");

            var messageBodyNode = messageNode.SelectSingleNode(MessageBodyItemDivXPath);

            if (messageBodyNode == null)
            {
                logger.Debug("The node is not a user message.");
                return false;
            }

            logger.Debug("The node is a user message");
            return true;
        }

        private bool TryExtractDate(HtmlNode msg, out string date)
        {
            bool hasExtracted = false;
            date = null;

            var dateNode = msg.SelectSingleNode(DateStringNodeXOath);

            if (dateNode != null)
            {
                date = dateNode.InnerText;
                hasExtracted = true;
            }

            return hasExtracted;
        }

        private void UpdateCurrentDate(string dateString)
        {
            bool isNullOrEmpty = string.IsNullOrWhiteSpace(dateString);
            Debug.Assert(!isNullOrEmpty, "!isNullOrEmpty - date is not expected to be null or empty.");
            if (isNullOrEmpty)
            {
                logger.Error("The date string found is null/empty.");
                return;
            }

            DateTime date;
            bool isParsed = DateTime.TryParse(dateString, out date);

            Debug.Assert(isParsed, $"isParsed - the date string givend ('{dateString}') is expected to be well-formed.");
            if (!isParsed)
            {
                logger.Error($"The date string ('{dateString}') could not be parsed.");
                return;
            }

            currentDate = date.Date;
        }
    }
}