using System;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.Game.Helpers;
using MGM.Game.Persistance.Game;
using MGM.Localization;
using MGM.TelemetryGlobal;

namespace MGM.Game.Engine.Helpers
{
    public static class GameInvoker
    {
        public static void InvokeGame(this GameProvider gameProvider, UserInChat userInChat, Action<Game> action,
            bool isPrivate = false)
        {
            var game = gameProvider.GetGameForUser(userInChat, isPrivate);

            if (isPrivate
            ) // when writing to private use language if the game chat (i'm not sure this should be done in this manner)
                LocalizedStrings.Language = gameProvider.GetLanguageForChat(game.ChatId);

            lock (game)
            {
                try
                {
                    TelemetryStatic.TelemetryClient.Context.Properties[TelemetryStatic.GameKey] = game.Id.ToString();
                    action(game);
                }
                catch (GameCommandException exception)
                {
                    throw new BrakeFlowCallException(exception.Message, exception);
                }
                gameProvider.SaveGame(game);
            }
        }

        public static void InvokeGame(this CallContext context, Action<Game> action)
        {
            var gameProvider = context.GetGameProvider();

            var isPrivate = context.Update.IsPrivate();

            var userInChat = context.UserInChat;

            gameProvider.InvokeGame(userInChat, action, isPrivate);
        }

        public static GameProvider GetGameProvider(this CallContext callContext)
        {
            return (GameProvider) callContext.UserState;
        }
    }
}