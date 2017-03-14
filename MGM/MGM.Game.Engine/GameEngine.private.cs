using System;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Processing;
using MGM.Game.Engine.Helpers;
using MGM.Game.Helpers;
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

        private void BuildPrivateFlow()
        {
#if ONECHAT_DEBUG
            _privateEngine.AddCommand("public")
                            .Execute(callContext => InternalExtensions._isPrivate = false)
                            .EchoReply(new OneLanguageString("You are in public"));
#endif
            _privateEngine.AddCommand("start").Execute(context =>
            {
                if (!CheckUserInTelegram(context))
                {
                    context.ReplyEcho(LocalizedStrings.PirvateEngine_AddMeToChat);
                }
            });
            _privateEngine.AddAnyInput().Execute(context =>
            {
                if (!CheckUserInTelegram(context))
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
                            context.ReplyEcho(LocalizedStrings.PirvateEngine_YourChoiceAccepted);
                    });
                }
            });
        }

        private static void SayPlayersCount(CallContext context, Game game)
        {
            var count = game.Players.Count;
            var distributions = GameState.NewGameState.PlayersDistribution.Distributions;
            var welcome = LocalizedStrings.PrivateEngine_WelcomeToGame + @"
";
            if (count < 3)
            {
                welcome += string.Format(LocalizedStrings.PrivateEngine_YouNeedMorePlayers, 3 - count, distributions[3].ToString(3));
            }
            else
            {
                if (count >= distributions.Length)
                {
                    throw new GameCommandException(LocalizedStrings.PirvateEngine_CanNotAddMorePlayers);
                }
                welcome += string.Format(LocalizedStrings.PrivateEngine_YouCanPlayAs, distributions[count].ToString(count));
            }
            welcome += @"
" + LocalizedStrings.PrivateEngine_ToSeeMoreModes;
            context.ReplyEcho(welcome);
        }
    }
}