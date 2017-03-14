using System;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;

namespace MGM.BotFlow.Executors
{
    public class ExecuteExecutor : IExecutor
    {
        private readonly Action<CallContext> _act;

        public ExecuteExecutor(Action<CallContext> act)
        {
            _act = act;
        }

        public void Execute(CallContext callContext, IState state)
        {
            _act(callContext);
        }
    }
}