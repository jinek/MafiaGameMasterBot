using System;
using System.Runtime.Serialization;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Processing
{
    public class ApiChatException : Exception
    {

        public ApiChatException()
        {
        }

        public ApiChatException(string message) : base(message)
        {
        }

        public ApiChatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiChatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public bool IsApiRequestException => InnerException is ApiRequestException;
        private ApiRequestException ApiRequestException => (ApiRequestException) InnerException;

        public int ErrorCode => ApiRequestException.ErrorCode;
    }
}