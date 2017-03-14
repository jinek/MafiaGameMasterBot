using System;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    /// <summary>
    ///     Special type, can be only one single child
    /// </summary>
    internal class AutomaticStep : Step
    {
        private readonly Func<CallContext, string> _act;

        public AutomaticStep(Func<CallContext, string> act)
        {
            _act = act;
        }

        protected override bool IsMatch(Update update)
        {
            return true;
        }

        protected override string GetParameter(Update update)
        {
            return null;
        }

        protected internal override string GetId()
        {
            return string.Empty;
        }

        internal override void Execute(CallContext callContext, IState state)
        {
            var result = _act(callContext);
            callContext.Push(result);
            base.Execute(callContext, state);
        }
    }
}