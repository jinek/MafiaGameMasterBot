using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.Localization;

namespace MGM.BotFlow.Executors
{
    public class EchoExecutor : IExecutor
    {
        private readonly ILocalizedString _echoText;
        private readonly EchoOptions _echoOptions;

        public EchoExecutor(ILocalizedString echoText, EchoOptions echoOptions)
        {
            _echoText = echoText;
            _echoOptions = echoOptions;
        }

        public void Execute(CallContext callContext, IState state)
        {
            callContext.ReplyEcho(_echoText.GetLocalizedString(), _echoOptions);
        }
    }
}