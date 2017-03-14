using System;
using System.Collections.Generic;
using System.Linq;
using MGM.BotFlow.Persistance;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Processing
{
    public class CallContext
    {
        private readonly Stack<string> _stack = new Stack<string>();
        private readonly ApiChat _apiChat;
        public UserInChat UserInChat { get; }

        internal CallContext(ApiChat apiChat, Update update, object userState, UserInChat userInChat)
        {
            _apiChat = apiChat;
            UserInChat = userInChat;
            UserState = userState;
            Update = update;
        }

        public string this[int index]
        {
            get
            {
                var strings = _stack.ToArray();
                if (!strings.Any()) return null;
                return strings[index];
            }
        }

        public Update Update { get; }

        public long UserMessageIdOrZero => Update.Message?.MessageId??0;

        public bool IsMessage => Update.Type == UpdateType.MessageUpdate;
        public bool IsQuery => Update.Type == UpdateType.CallbackQueryUpdate;

        public ApiChat ApiChat => _apiChat;

        public object UserState { get; }

        public void ReplyEcho(string str, EchoOptions echoOptions = null)
        {
            switch (Update.Type)
            {
                case UpdateType.MessageUpdate:
                    Echo(str, Update.Message.MessageId, echoOptions);
                    break;
                case UpdateType.CallbackQueryUpdate:
                    _apiChat.ReplyQuery(str,Update.CallbackQuery.Id);
                    //Update.CallbackQuery.Id
                    break;
                default:
                    throw new InvalidOperationException("Message you are trying to reply to unsupported message");
            }
        }
        
        public Echo Echo(string str, long? replyTo=null,EchoOptions echoOptions=null)
        {
            string format;
            try
            {
                format = string.Format(str, _stack.Cast<object>().ToArray());
            }
            catch (FormatException exception)
            {
                throw new FormatException("Seems stack misses values",exception);
            }
            return _apiChat.Echo(format, replyTo??0,echoOptions);
        }

        internal void Push(string value)
        {
            if (value == null) return;
            _stack.Push(value);
        }
    }

}