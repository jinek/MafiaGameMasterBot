using System;
using System.Linq;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using MGM.Game.Persistance.Database.Helpers;
using MGM.Game.Persistance.Game;
using MGM.Game.States;
using MGM.Localization;

namespace MGM.Game.Engine
{
    public partial class GameEngine
    {
        public void BuildCommonFlow()
        {
            AddFeedbackCommand(_publicEngine);
            AddFeedbackCommand(_privateEngine);
            AddHelpCommand(_publicEngine);
            AddHelpCommand(_privateEngine);
            AddModesCommand(_publicEngine);
            AddModesCommand(_privateEngine);
            AddLanguageCommand(_publicEngine);
            AddLanguageCommand(_privateEngine);
            AddVersionCommands(_publicEngine);
            AddVersionCommands(_privateEngine);
        }

        private void AddVersionCommands(Step engine)
        {
            engine.AddCommand("subscribe")
                .Execute(context =>
                {
                    _gameProvider.UsingDb(dbContext =>
                    {
                        dbContext.ChatInTelegrams.ById(context.UserInChat.ChatId).Subscribed = true;
                        dbContext.SaveChanges();
                    });
                    context.ReplyEcho(LocalizedStrings.CommonEngine_Subscribed);
                });

            engine.AddCommand("unsubscribe")
                .Execute(context =>
                {
                    _gameProvider.UsingDb(dbContext =>
                    {
                        dbContext.ChatInTelegrams.ById(context.UserInChat.ChatId).Subscribed = false;
                        dbContext.SaveChanges();
                    context.ReplyEcho(LocalizedStrings.CommonEngine_Unsubscribed);
                    });
                });

            engine.AddCommand("version")
                .Execute(context =>
                {
                    context.ReplyEcho(_history);
                });
        }

        private void AddLanguageCommand(Step engine)
        {
            var languages = new[] {"english","russian"};
            engine.AddCommand("language")
                .EchoReply(new OneLanguageString("Choose language/Выберите язык: " + languages.Aggregate(string.Empty,(s, s1) => $"{s}/{s1}")),
                    EchoOptions.EchoReplyButtons(languages))
                .AddAnyInput()
                .Execute(context =>
                {
                    uint? selectedIndex = null;
                    var languageStr = context[0];
                    for (uint i = 0; i < languages.Length; i++)
                    {
                        var language = languages[i];
                        if (!string.Equals(language, languageStr, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        selectedIndex = i;
                        break;
                    }

                    if (selectedIndex != null)
                    {
                        var gameProvider = (GameProvider)context.UserState;
                        var choosenLanguage = (uint)selectedIndex;
                        gameProvider.UsingDb(dbContext =>
                        {
                            var userInChatPersistance = dbContext.UsersInChat.GetUserInChatPersistance(context.UserInChat);
                            var chatInTelegram = userInChatPersistance.ChatInTelegram;
                            chatInTelegram.LanguageIndex = choosenLanguage;
                            dbContext.SaveChanges();
                        });
                        LocalizedStrings.Language = choosenLanguage;
                        context.ReplyEcho(LocalizedStrings.CommonEngine_LanguageSwitched);
                    }
                    else
                    {
                        context.ReplyEcho("Can not recognize selected language/Не удаётся распознать выбранный язык");//todo: low в реусрс
                    }
                });
        }

        private void AddFeedbackCommand(Step engine)
        {
            engine.AddCommandWithParameter("feedback", LocalizedStrings.CommonEngine_EnterFeedbackText)
                .ForEach(step => step.Execute(context =>
                {
                    string feedback = context[0];
                    RaiseFeedback(new FeedbackEventArgs(feedback));
                    context.ReplyEcho(LocalizedStrings.CommonEngine_FeedbackAccepted);
                }));
        }

        private void AddModesCommand(Step engine)
        {

            engine.AddCommand("modes").Execute(context =>
            {
                var str = string.Empty;
                var distributions = GameState.NewGameState.PlayersDistribution.Distributions;
                for (var i = 3; i < distributions.Length; i++)
                {
                    str += @"
" + string.Format(LocalizedStrings.PublicEngine_CountPlayersAndShowMode, i, distributions[i].ToString(i));
                }
                context.ReplyEcho(str);
            });
        }

        private void AddHelpCommand(Step engine)
        {
            engine.AddCommand("help").EchoReply(LocalizedStrings.CommonEngine_Help);
        }

        public event EventHandler<FeedbackEventArgs> Feedback;

        private void RaiseFeedback(FeedbackEventArgs feedback)
        {
            Feedback?.Invoke(this, feedback);
        }
    }

    public class FeedbackEventArgs : EventArgs
    {
        public FeedbackEventArgs(string feedbackText)
        {
            FeedbackText = feedbackText;
        }

        public string FeedbackText { get; }
    }
}