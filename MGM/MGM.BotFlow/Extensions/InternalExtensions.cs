using System;
using MGM.BotFlow.Processing;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Extensions
{
    public static class InternalExtensions
    {
#if ONECHAT_DEBUG
        // ReSharper disable once InconsistentNaming
        public static bool _isPrivate = false;
#endif
        public static bool IsPrivate(this Update update)
        {
#if ONECHAT_DEBUG
            return _isPrivate;
#else

            var chatType = update.GetMessage().Chat.Type;
            switch (chatType)
            {
                case ChatType.Private:
                    return true;
                case ChatType.Group:
                case ChatType.Supergroup:
                    return false;

                default:
                    throw new NotSupportedException(chatType.ToString());
            }
#endif
        }

        public static Chat GetChat(this Update update)
        {
#if ONECHAT_DEBUG
            ApiChat.RealChat = update.GetMessage().Chat.Id;
            var chat = new Chat();
            var type = chat.GetType();

            var idProperty = type.GetProperty("Id");
            idProperty.SetValue(chat, _isPrivate?OneChatDebugCurrent.Id:666);

            var titleProperty = type.GetProperty("Title");
            titleProperty.SetValue(chat, _isPrivate ? OneChatDebugCurrent.Username : "666");
            return chat;
#else
            return update.GetMessage().Chat;
#endif
        }

        public static long GetMessageId(this Update update)
        {
            return update.GetMessage().MessageId;
        }

        public static User OneChatDebugCurrent = new User();
        public static User GetUser(this Update update)
        {
#if ONECHAT_DEBUG
            return OneChatDebugCurrent;
#else

            switch (update.Type)
            {
                case UpdateType.CallbackQueryUpdate:
                    return update.CallbackQuery.From;
                case UpdateType.MessageUpdate:
                    return update.GetMessage().From;
                default:
                    throw new NotSupportedException();
            }
#endif
        }

        public static Message GetMessage(this Update update)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQueryUpdate:
                    return update.CallbackQuery.Message;
                case UpdateType.MessageUpdate:
                    return update.Message;
                default:
                    throw new NotSupportedException();
            }
        }

        public static Persistance.Chat ToChat(this Chat chat)
        {
            return new Persistance.Chat(chat.Id,chat.Title);
        }
    }
}