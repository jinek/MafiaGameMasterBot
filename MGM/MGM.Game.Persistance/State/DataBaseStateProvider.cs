using MGM.BotFlow.Persistance;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.Helpers;
using UserInChat = MGM.BotFlow.Persistance.UserInChat;

namespace MGM.Game.Persistance.State
{
    public class DatabaseStateProvider : IStateProvider
    {
        private readonly string _connectionString;

        public DatabaseStateProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IState GetStateForUserInChat(UserInChat userInChat)
        {
            return DatabaseState.GetRootState(userInChat,_connectionString);
        }
        
        public static void CleanStateForChat(UserInChat userInChat,string connectionString)
        {
            using (var db = new DbContext(connectionString))
            {
                var userInChatPersistance = db.UsersInChat.GetUserInChatPersistance(userInChat);
                if (userInChatPersistance != null)
                {
                    db.Delete(userInChatPersistance.FlowStates);
                    db.SaveChanges();
                }
            }
        }
    }
}