using System.Collections.Generic;
using MGM.Game.Persistance.Database.DataModels.Telegram;

namespace MGM.Game.Persistance.Database.DataModels.State
{
    public class UserInChat
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public long ChatId { get; set; }

        public UserInTelegram UserInTelegram { get; set; }
        public ChatInTelegram ChatInTelegram { get; set; }
        
        public IList<FlowState> FlowStates { get; set; } = new List<FlowState>();
    }
}