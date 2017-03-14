using MGM.BotFlow.Processing;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    public class CommandStep : RegexInputStep
    {
        private readonly bool _acceptParameter;

        public CommandStep(string command,bool acceptParameter) : base($@"\A/{command}(@MafiaGameMasterBot)?"+(acceptParameter?$"(?<{ParameterGroupName}> +?.+)" :string.Empty)+ @"\z")
        {
            _acceptParameter = acceptParameter;
        }

        protected override string GetParameter(Update update)
        {
            if (!_acceptParameter) return null;
            var parameter = base.GetParameter(update).Trim();

            if(string.IsNullOrWhiteSpace(parameter))throw new CommandNotFoundException();
            return parameter;
        }
    }
}