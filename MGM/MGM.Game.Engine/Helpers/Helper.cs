using System;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using User = MGM.Game.Models.User;

namespace MGM.Game.Engine.Helpers
{
    public static class Helper
    {

        public static User ToUser(this UserInChat userInChat)
        {
            return new User(userInChat.UserId, userInChat.UserName);
        }

        public static Step ExecuteGame(this Step step, Action<CallContext,Game> doAction)
        {
            return step.Execute(context =>
            {
                GameInvoker.InvokeGame(context, game =>
                {
                    doAction(context, game);
                });
            });
        }
    }
}