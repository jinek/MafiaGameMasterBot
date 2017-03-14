using MGM.BotFlow.Persistance;
using MGM.BotFlow.Persistance.InMemory;

namespace MGM.BotFlow.ConsoleTest
{
    internal class InMemoryStateProvider : IStateProvider
    {
        public IState GetStateForUserInChat(UserInChat userInChat)
        {
            return InMemoryRootState.GetStateForChat(userInChat);
        }
    }
}