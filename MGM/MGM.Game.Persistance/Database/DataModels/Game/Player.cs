using System;
using MGM.Game.Persistance.Database.DataModels.Telegram;

namespace MGM.Game.Persistance.Database.DataModels.Game
{
    public class Player
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime? PutToVoting { get; set; }
        
        public UserInTelegram UserInTelegram { get; set; }
    }
}