using MGM.BotFlow.Processing;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    public class AnyInputStep : RegexInputStep
    {
        public AnyInputStep() : base($@"\A(?<{ParameterGroupName}>.+)\z")
        {
        }

        protected override string GetParameter(Update update)
        {
            var input = base.GetParameter(update);
            if(string.IsNullOrWhiteSpace(input))throw new CommandNotFoundException();
            return input;
        }
    }
}