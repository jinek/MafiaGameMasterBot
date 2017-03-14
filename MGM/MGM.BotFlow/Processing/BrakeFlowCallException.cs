using System;

namespace MGM.BotFlow.Processing
{
    public class BrakeFlowCallException : Exception
    {
        public BrakeFlowCallException(string message, Exception exception=null):base(message,exception)
        {
        }
    }
}