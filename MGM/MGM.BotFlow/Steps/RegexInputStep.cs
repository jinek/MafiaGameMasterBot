using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    //https://msdn.microsoft.com/en-us/library/az24scfc(v=vs.110).aspx
    public class RegexInputStep : TextInputStep
    {
        internal const string ParameterGroupName = "parameter";
        private readonly string _pattern;

        public RegexInputStep(string pattern)
        {
            _pattern = pattern;
        }

        protected internal override string GetId()
        {
            return _pattern;
        }

        protected override bool IsMatch(Update update)
        {
            if (update.Type != UpdateType.MessageUpdate) return false;
            if (update.Message.Type != MessageType.TextMessage) return false;
            var text = update.Message.Text;
            var regex = GetRegex();
            return regex.IsMatch(text);
        }

        private Regex GetRegex()
        {
            return new Regex(_pattern, RegexOptions.IgnoreCase);
        }

        protected override string GetParameter(Update update)
        {
            var regex = GetRegex();
            var match = regex.Match(base.GetParameter(update));
            return match.Groups[ParameterGroupName].Value;
        }
    }
}