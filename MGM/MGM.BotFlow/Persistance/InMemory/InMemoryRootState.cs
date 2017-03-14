using System.Collections.Generic;

namespace MGM.BotFlow.Persistance.InMemory
{
    public static class InMemoryRootState
    {
        private static readonly Dictionary<UserInChat, InMemoryState> States = new Dictionary<UserInChat, InMemoryState>();

        public static IState GetStateForChat(UserInChat chat)
        {
            if (!States.ContainsKey(chat))
            {
                States.Add(chat, new InMemoryState(string.Empty, null));
            }

            return States[chat];
        }
    }
}