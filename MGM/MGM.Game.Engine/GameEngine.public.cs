using System;
using System.Diagnostics;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using MGM.Game.Engine.Helpers;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Game.Persistance.Game;
using MGM.Game.States;
using MGM.Localization;

namespace MGM.Game.Engine
{
    public partial class GameEngine
    {
        public void Ready(UserInChat userInChat, GameProvider gameProvider, CallContext context=null)
        {
            string userName = userInChat.UserName;
            if (userName == null)
                throw new BrakeFlowCallException(LocalizedStrings.PublicEngine_ChangeName);
            Trace.WriteLine(userName);
            gameProvider.InvokeGame(userInChat, game =>
            {
                _gameProvider.CheckUserInTelegram(userInChat.UserId, publicChatId: userInChat.ChatId);
                game.Ready(userInChat.ToUser());
                SayPlayersCount(game,context);
            });
        }

        private void BuildPublicFlow()
        {
            _publicEngine.AddCommand("status").ExecuteGame((context, game) =>
            {
                context.ReplyEcho(game.GetStatus());
            });

            //_publicEngine.AddCommand("exception").Execute(context => { throw new InvalidOperationException("test exception"); });
            _publicEngine.AddCommand("abort")
                .ExecuteGame((context, game) =>
                {
                    game.Abort();
                });

            _publicEngine.AddCommand("ready").Execute(context =>
            {
                Ready(context.UserInChat,context.GetGameProvider(), context);
            });

            _publicEngine.AddCommand("exit").Execute(context =>
            {
                context.InvokeGame(game =>
                {
                    var user = context.UserInChat.ToUser();
                    game.Unready(user);
                    //SayPlayersCount(context, game);
                });
            }).EchoReply(LocalizedStrings.PublicEngine_YouAreOut);

            _publicEngine.AddCommand("vote").ExecuteGame((context, game) =>
            {
                game.ForcePrevoting();
            });

            _publicEngine.AddCommand("go")
                .Execute(context =>
                {
                    context.InvokeGame(game =>
                    {
                        game.Go();
                    });
                    context.ReplyEcho(LocalizedStrings.PublicEngine_GameStartedAfterGo);
                });

            _publicEngine.AddQueryResult().ExecuteGame((context, game) =>
            {
                var user = context.UserInChat.ToUser();

                string messageId;
                string queryData;
                var parameters = context[0];
                if (!QueryResultStep.ParseParameters(parameters, out messageId, out queryData))
                    throw new InvalidOperationException("Can not parse " + parameters);

                var autoGameState = game.State.Cast<AutoGameState>();
                if (autoGameState != null)
                {

                    if (autoGameState.Banner.Id.ToString() != messageId)
                    {
                        throw new BrakeFlowCallException(LocalizedStrings.Voting_NotVotingTime);
                    }
                    game.PublicVote(user, queryData);
                    context.ReplyEcho(LocalizedStrings.PirvateEngine_YourChoiceAccepted);
                }
                else
                {
                    context.ReplyEcho(LocalizedStrings.Voting_NotVotingTime);
                }
            });
            
#if ONECHAT_DEBUG
            _publicEngine.AddCommand("switch", true).Execute(context =>
            {
                var userId = int.Parse(context[0]);
                var user = new Telegram.Bot.Types.User();
                Type type = user.GetType();

                var idProperty = type.GetProperty("Id");
                idProperty.SetValue(user,userId);
                var nameProperty = type.GetProperty("Username");
                nameProperty.SetValue(user,userId.ToString());
                InternalExtensions.OneChatDebugCurrent = user;
            }).EchoReply(new OneLanguageString("User switched to {0}"));

            _publicEngine.AddCommand("private")
                .Execute(context => InternalExtensions._isPrivate = true)
                .EchoReply(new OneLanguageString("You are in private"));


            _publicEngine.AddCommand("timer").Execute(context => context.InvokeGame(game => game.Timer()));
#endif
        }
    }
}