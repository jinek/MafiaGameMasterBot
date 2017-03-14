using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Processing
{
    [DataContract]
    public class EchoOptions
    {
        [DataMember]
        private readonly IReplyMarkup _replyMarkup;

        /// <summary>
        /// Simple echo
        /// </summary>
        private EchoOptions(IReplyMarkup replyMarkup=null)
        {
            _replyMarkup = replyMarkup;
        }


        internal IReplyMarkup ReplyMarkup => _replyMarkup;

        public static EchoOptions SimpleEcho()
        {
            return new EchoOptions();
        }

        public static EchoOptions EchoForceReply()
        {
            return new EchoOptions(new ForceReply {Force = true,Selective = true});
        }

        public static EchoOptions EchoInlineButtons(Dictionary<string,string> options)
        {
            var buttons = options.Select(pair => new InlineKeyboardButton(pair.Value, pair.Key)).ToArray();

            return new EchoOptions(new InlineKeyboardMarkup(buttons));
        }

        public static EchoOptions EchoReplyButtons(string[] options)
        {
            var buttons = options.Select(command => new KeyboardButton(command)).ToArray();

            return new EchoOptions(new ReplyKeyboardMarkup(buttons) {OneTimeKeyboard = true,ResizeKeyboard = true});
        }
    }
}