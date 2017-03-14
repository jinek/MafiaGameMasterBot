using System;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    public class QueryResultStep : Step
    {
        private const string SplitString = "||";

        protected override bool IsMatch(Update update)
        {
            return update.Type == UpdateType.CallbackQueryUpdate;
        }

        protected override string GetParameter(Update update)
        {
            return $"{update.CallbackQuery.Message.MessageId}||{update.CallbackQuery.Data}";
        }

        public static bool ParseParameters(string parameter,out string messageId,out string queryData)
        {
            var strings = parameter.Split(new [] {SplitString},StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length != 2)
            {
                messageId = null;
                queryData = null;
                return false;
            }
            messageId = strings[0];
            queryData = strings[1];
            return true;
        }

        protected internal override string GetId()
        {
            return UpdateType.CallbackQueryUpdate.ToString();//Only one step of type QueryResult is possible
        }
    }
}