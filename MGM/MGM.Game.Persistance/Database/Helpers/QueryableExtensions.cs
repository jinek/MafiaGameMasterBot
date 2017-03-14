using System.Linq;
using MGM.Game.Persistance.Database.DataModels;
using MGM.Game.Persistance.Database.DataModels.State;
using MGM.Game.Persistance.Database.DataModels.Telegram;

namespace MGM.Game.Persistance.Database.Helpers
{
    public static class QueryableExtensions
    {
        public static UserInChat GetUserInChatPersistance(this IQueryable<UserInChat> users,
            BotFlow.Persistance.UserInChat userInChat)
        {
            return users.SingleOrDefault(state => state.ChatId == userInChat.ChatId && state.UserId == userInChat.UserId);
        }

        public static DataModels.Game.Game ById(this IQueryable<DataModels.Game.Game> games, long id)
        {
            return games.Single(game => game.Id == id);
        }

        public static UserInTelegram ById(this IQueryable<UserInTelegram> userInTelegrams,long id)
        {
            return userInTelegrams.SingleOrDefault(telegram => telegram.Id == id);
        }

        public static IQueryable<DataModels.Game.Game> NotFinished(this IQueryable<DataModels.Game.Game> games)
        {
            return games.Where(game => game.FinishTime == null);
        }

        public static ChatInTelegram ById(this IQueryable<ChatInTelegram> chats, long id)
        {
            return chats.SingleOrDefault(chat => chat.Id == id);
        }
    }
}