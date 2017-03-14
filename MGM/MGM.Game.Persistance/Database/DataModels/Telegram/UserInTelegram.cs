using System.Collections.Generic;
using MGM.Game.Persistance.Database.DataModels.Game;
using MGM.Game.Persistance.Database.DataModels.State;

namespace MGM.Game.Persistance.Database.DataModels.Telegram
{
    public class UserInTelegram
    {
        public UserInTelegram()
        {
            AllGameCount = 0;
            WinCount = 0;
        }

        public long Id { get; set; }
        public IList<Player> PlayerInGames { get; set; } = new List<Player>();
        public IList<UserInChat> UserInChats { get; set; } = new List<UserInChat>();

        public long? PrivateChatId { get; set; }
        public ChatInTelegram PrivateChat { get; set; }

        public int AllGameCount { get; set; }
        public int WinCount { get; set; }
    }
}