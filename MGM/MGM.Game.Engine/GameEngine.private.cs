using System;
using System.Linq;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.Game.Engine.Helpers;
using MGM.Game.Helpers;
using MGM.Game.Persistance.Database.Helpers;
using MGM.Game.Persistance.Game;
using MGM.Game.States;
using MGM.Localization;

namespace MGM.Game.Engine
{
    public partial class GameEngine
    {
        private bool CheckUserInTelegram(CallContext context)
        {
            var gameProvider = (GameProvider) context.UserState;
            var userInChat = context.UserInChat;
            if (gameProvider.CheckUserInTelegram(userInChat.UserId, userInChat.ChatId))
            {
                context.ReplyEcho(LocalizedStrings.PrivateEngine_ICanTextYou);
                return true;
            }
            return false;
        }

        private void ExecuteIfPrivateChatWasAlreadyCreatedBefore(CallContext context, Action action)
        {
            if (!CheckUserInTelegram(context))
            {
                action();
            }
            else
            {
                var gameProvider = context.GetGameProvider();

                gameProvider.UsingDb(dbContext =>
                {
                    var userInTelegram = dbContext.UserInTelegrams.ById(context.UserInChat.UserId);
                    foreach (var userInChatToBeReady in userInTelegram.UserInChats.Where(userInChat => userInChat
                        .WantToBeReady))
                        Ready(
                            new UserInChat(
                                new Chat(userInChatToBeReady.ChatId, "" /*Where to take name from. May be from game*/),
                                context.Update.GetUser()), _gameProvider);
                });
            }
        }

        private void BuildPrivateFlow()
        {
#if ONECHAT_DEBUG
            _privateEngine.AddCommand("public")
                            .Execute(callContext => InternalExtensions._isPrivate = false)
                            .EchoReply(new OneLanguageString("You are in public"));
#endif
            _privateEngine.AddCommand("start").Execute(context =>
            {
                ExecuteIfPrivateChatWasAlreadyCreatedBefore(context,
                    () => { context.ReplyEcho(LocalizedStrings.PirvateEngine_AddMeToChat); });
            });

            _privateEngine.AddAnyInput().Execute(context =>
            {
                ExecuteIfPrivateChatWasAlreadyCreatedBefore(context, () =>
                {
                    context.InvokeGame(game =>
                    {
                        var privateVoteResult = game.PrivateVote(context.UserInChat.ToUser(), context[0]);
                        if (privateVoteResult != null)
                        {
                            string str;
                            switch (privateVoteResult)
                            {
                                case PlayerRole.Citizen:
                                    str = LocalizedStrings.GameState_CivilianWord;
                                    break;
                                case PlayerRole.Doctor:
                                    str = LocalizedStrings.GameState_DoctorWord;
                                    break;
                                case PlayerRole.Mafia:
                                    str = LocalizedStrings.GameState_MafiaWord;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            context.ReplyEcho(str);
                        }
                        else
                        {
                            context.ReplyEcho(LocalizedStrings.PirvateEngine_YourChoiceAccepted);
                        }
                    });
                });
            });
        }

        private static void SayPlayersCount(Game game, CallContext context=null)
        {
            var count = game.Players.Count;
            var distributions = GameState.NewGameState.PlayersDistribution.Distributions;
            var contextProvided = context != null;
            var welcome = (contextProvided?LocalizedStrings.PrivateEngine_WelcomeToGame:LocalizedStrings.UserJoinsGame )+ @"
";
            if (count < 3)
            {
                welcome += string.Format(LocalizedStrings.PrivateEngine_YouNeedMorePlayers, 3 - count,
                    distributions[3].ToString(3));
            }
            else
            {
                if (count >= distributions.Length)
                    throw new GameCommandException(LocalizedStrings.PirvateEngine_CanNotAddMorePlayers);
                welcome += string.Format(LocalizedStrings.PrivateEngine_YouCanPlayAs,
                    distributions[count].ToString(count));
            }
            welcome += @"
" + LocalizedStrings.PrivateEngine_ToSeeMoreModes;
            if (contextProvided) context.ReplyEcho(welcome);
            else game.GameStateBannerProvider.CreateBanner(welcome);
        }
    }
}