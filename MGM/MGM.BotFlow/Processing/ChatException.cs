using System;

namespace MGM.BotFlow.Processing
{
    public class ChatException : Exception
    {
        public ChatException(ApiChat chat,string message, Exception innerException) : base(message, innerException)
        {
            Chat = chat;
        }

        public ApiChat Chat { get; }
    }
}