using Telegram.Bot.Types;

namespace MGM.BotFlow.Persistance
{
    public struct UserInChat
    {
        private readonly Chat _chat;

        public UserInChat(Chat chat, User user)
        {
            _chat = chat;
            UserName = user.Username;
            UserId = user.Id;
        }

        public long ChatId => _chat.Id;
        public long UserId { get; }

        public Chat Chat => _chat;

        public string UserName { get; }
    }
}