using System;

namespace MGM.Game.Helpers
{
    /// <summary>
    /// Message from this exception will be shown to the user
    /// </summary>
    public class GameCommandException : Exception
    {
        public GameCommandException(string message) : base(message)
        {
        }
    }
}