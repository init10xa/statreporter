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
        private readonly string ActivityMessageNodeXPath = "./div/div/div/div[@class='im_service_message']";
        private readonly string AddedOrDeletedUserNodeXPath = "./div/div/div/div/span/span/span/my-i18n-param/a[@my-peer-link='historyMessage.action.user_id']";
        private readonly string CreateChatActivityMessageXPath = "./div/div/div/div/span/span/my-i18n/span";
        private readonly string DateStringNodeXOath = "./div/div/span[@class='im_message_date_split_text']";
        private readonly string GenericActivityMessageNodeXPath = "./div/div/div/div/span/span/span";
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

        private string ExtractMessageAuthor(HtmlNode messageNode)
        {
            var messageAuthorNode = messageNode.SelectSingleNode(MessageAuthorNodeXPath);

            Debug.Assert(messageAuthorNode != null,
                        $"messageAuthorNode != null - message's author node expected to be found at the following path: '{MessageTimeNodeXPath}'.");
            if (messageAuthorNode == null)
            {
                logger.Error($"Could not find message's author node for the current node at the following path: '{MessageTimeNodeXPath}'. " +
                             $"Here is the current node: {Environment.NewLine}{messageNode.OuterHtml}");
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

            foreach (var msgNode in historyMessages)
            {
                string date;
                if (TryExtractDate(msgNode, out date))
                    UpdateCurrentDate(date);

                isMessageProcessed = false;
                if (IsUserMessage(msgNode))
                {
                    MessageMetaData metaData = GetMessageMetaData(msgNode);
                    messages.Add(metaData);
                    isMessageProcessed = true;
                }
                else if (IsActivityMessage(msgNode))
                {
                    ProcessActionMessage(msgNode);
                    isMessageProcessed = true;
                }

                Debug.Assert(isMessageProcessed, "The msg is expected to match either a 'user message'" +
                             " or a 'notification message' and be processed already.");
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

        private ActionType GetAction(HtmlNode messageNode)
        {
            var actionNode = messageNode.SelectSingleNode(GenericActivityMessageNodeXPath);

            if (actionNode == null)
                actionNode = messageNode.SelectSingleNode(CreateChatActivityMessageXPath);

            Debug.Assert(actionNode != null, "actionNode != null - action node expected to exist.");
            if (actionNode == null)
            {
                logger.Error($"Could not find an action node at the expected path in: {Environment.NewLine}{messageNode.OuterHtml}");
                return ActionType.None;
            }

            string actionString = actionNode.Attributes["ng-switch-when"].Value;

            Debug.Assert(!string.IsNullOrWhiteSpace(actionString),
                        "!string.IsNullOrWhiteSpace(action) - action expected to exist.");
            if (string.IsNullOrWhiteSpace(actionString))
            {
                logger.Error($"Could not find an action on the action node for in: {Environment.NewLine}{messageNode.OuterHtml}");
                return ActionType.None;
            }

            ActionType action = ActionTypeConverter.GetActionType(actionString);

            Debug.Assert(action != ActionType.None, "action != ActionType.None - action string expected to be valid.");
            if (action == ActionType.None)
            {
                logger.Error($"Could not convert action string ('{actionString}') to a valid ActionType.");
                return ActionType.None;
            }

            return action;
        }

        private string GetAddedDeletedUserName(HtmlNode messageNode)
        {
            var userNode = messageNode.SelectSingleNode(AddedOrDeletedUserNodeXPath);

            Debug.Assert(userNode != null,
                        "userNode != null - user's name node is expected to exist.");
            if (userNode == null)
            {
                logger.Error($"Could not find the node containing user's name in message node: {Environment.NewLine}{messageNode.OuterHtml}");
                return string.Empty;
            }

            string userName = userNode.InnerText;

            Debug.Assert(!string.IsNullOrWhiteSpace(userName), "!string.IsNullOrWhiteSpace(userName) - user's name is expected to be non-empty.");
            if (string.IsNullOrWhiteSpace(userName))
            {
                logger.Error($"User's name is empty/null in the node: {Environment.NewLine}{messageNode.OuterHtml}");
                return string.Empty;
            }

            return userName;
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

        private MessageMetaData GetMessageMetaData(HtmlNode messageNode)
        {
            var metaData = new MessageMetaData();

            var messageTime = ExtractMessageTime(messageNode);
            metaData.Timestamp = CreateFullDate(currentDate, messageTime);
            string messageAuthor = ExtractMessageAuthor(messageNode);
            metaData.Sender = GetUser(messageAuthor);

            return metaData;
        }

        private User GetUser(string name)
        {
            User user;

            Debug.Assert(!string.IsNullOrWhiteSpace(name),
                        "!string.IsNullOrWhiteSpace(name) - given name is expected to be non-empty.");
            if (string.IsNullOrWhiteSpace(name))
            {
                string message = "Received an empty/null user name to look up in the list.";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            if (!users.TryGetValue(name, out user))
            {
                user = new User();
                user.Name = name;
            }

            return user;
        }

        private bool IsActivityMessage(HtmlNode messageNode)
        {
            logger.Debug($"Checking if the node is an activity message... {Environment.NewLine}{messageNode.OuterHtml}");

            var activityMessageNode = messageNode.SelectSingleNode(ActivityMessageNodeXPath);

            if (activityMessageNode == null)
            {
                logger.Debug("The node is not an activity message.");
                return false;
            }

            logger.Debug("The node is an activity message.");
            return true;
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

        private void ProcessActionMessage(HtmlNode messageNode)
        {
            ActionType action = GetAction(messageNode);

            switch (action)
            {
                case ActionType.CreateChat:
                    // nothing to process
                    return;

                case ActionType.AddUser:
                    ProcessUserAddedMessage(messageNode);
                    return;

                case ActionType.DeleteUser:
                    ProcessUserDeletedMessage(messageNode);
                    return;

                case ActionType.LeaveChat:
                    ProcessUserLeftMessage(messageNode);
                    return;
            }

            logger.Error($"Received an invalid activity message of type {action} which could not be processed.");
        }

        private void ProcessUserAddedMessage(HtmlNode messageNode)
        {
            string userName = GetAddedDeletedUserName(messageNode);
            var user = GetUser(userName);
            user.AddAJoinDate(currentDate);
        }

        private void ProcessUserDeletedMessage(HtmlNode messageNode)
        {
            string userName = GetAddedDeletedUserName(messageNode);
            var user = GetUser(userName);
            user.AddLeftDate(currentDate);
        }

        private void ProcessUserLeftMessage(HtmlNode messageNode)
        {
            var userName = ExtractMessageAuthor(messageNode);
            var user = GetUser(userName);
            user.AddLeftDate(currentDate);
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