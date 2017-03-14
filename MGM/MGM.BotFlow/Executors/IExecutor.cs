using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;

namespace MGM.BotFlow.Executors
{
    public interface IExecutor
    {
        void Execute(CallContext callContext, IState state);
    }
}