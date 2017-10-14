using System;

namespace MGM.BotFlow.Processing
{
    /// <summary>
    /// This exception is thrown when telegram API response http 400 Bad request
    /// This is group of errors, like chat not found,query id not found and so on
    /// I hope that in the root of all of them there is a situation that bot was blocked
    /// </summary>
    public class ApiChatBadRequestException : Exception
    {//Error_Code is 400Bad Request: group chat was migrated to a supergroup chat Error_Code is 400Bad Request: group chat was migrated to a supergroup chat
        internal ApiChatBadRequestException()
        {
        }
    }
}