using System.Collections.Generic;
using MGM.Game.Persistance.Database.DataModels.State;

namespace MGM.Game.Persistance.Database.DataModels.Telegram
{
    public class ChatInTelegram
    {
        public ChatInTelegram()
        {
            Subscribed = false;
        }

        public long Id { get; set; }
        public uint LanguageIndex { get; set; }
        public bool Subscribed { get; set; }
        
        public IList<UserInChat> UserInChats { get; set; }=new List<UserInChat>();
        public IList<Game.Game> Games { get; set; }=new List<Game.Game>();
        
        public UserInTelegram UserOfPrivateChat { get; set; }
    }
}