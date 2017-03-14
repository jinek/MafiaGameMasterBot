using System;
using System.Collections.Generic;
using MGM.Game.Persistance.Database.DataModels.Telegram;

namespace MGM.Game.Persistance.Database.DataModels.Game
{
    public class Game
    {
        public int Id { get; set; }
        public string SerializedGame { get; set; }
        public long ChatId { get; set; }
        public ChatInTelegram ChatInTelegram { get; set; }

        public DateTime? MaxWakeupTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public IList<Player> Players { get; set; }=new List<Player>();
        public DateTime? FinishTime { get; set; }
        public bool IsNight { get; set; }
    }
}