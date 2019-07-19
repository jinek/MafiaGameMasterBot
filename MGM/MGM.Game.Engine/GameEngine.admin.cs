using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using MGM.Game.Engine.Helpers;
using MGM.Game.Persistance;
using MGM.Localization;

namespace MGM.Game.Engine
{
    public partial class GameEngine
    {
        private readonly Dictionary<Guid,UserInChat> _sessions = new Dictionary<Guid, UserInChat>();//not thread safe but we don't care (+ we really care, must be thread safe)

        public void BuildAdminFlow()
        {
            List<string> commands = new List<string>();

            /*AppendAuthorizedCommand("test", commands, context =>
            {
                context.ReplyEcho(@"Game has not been started yet. _Players are allowed to enter and exit the game_
@Yukichang28
");

                @chanyuhan
@n
*mafia* - 1, *civilian* - 2, Loose on all civilians kill
            });*/

            AppendAuthorizedCommand("list_stories",commands, context =>
            {
                var allStories = DatabaseHelper.GetAllStories(_connectionString);
                if(!allStories.Any())context.ReplyEcho("Нет историй");
                for (int index = 0; index < allStories.Length; index++)
                {
                    var story = allStories[index];
                    context.ApiChat.PhotoEcho($"{index+1}. "+story.Body, story.PhotoId);
                }
            });

            AppendAuthorizedCommand("add_story", commands)
                .EchoReply(new OneLanguageString("Введите название истории"))
                .AddAnyInput()
                .EchoReply(new OneLanguageString("Пришлите банер"))
                .AddPhotoInput()
                .Execute(context =>
                {
                    var photoId = context[0];
                    var caption = context[1];
                    if (caption.Length > 200)
                        context.ReplyEcho("Длина заголовка должна быть не более 200 символов");
                    else
                    {
                        DatabaseHelper.AddStory(photoId, caption, _connectionString);
                        context.ReplyEcho("История добавлена");
                    }
                });

            AppendAuthorizedCommand("stat", commands,
                context =>
                {
                    context.ReplyEcho(context.GetGameProvider().GetStat());
                });

            AppendAuthorizedCommand("delete_story", commands)
                .EchoReply(new OneLanguageString("Укажите номер истории"))
                .AddAnyInput()
                .Execute(context =>
                {
                    var indexStr = context[0];
                    int index;
                    if (!int.TryParse(indexStr, out index))throw new BrakeFlowCallException("Необходимо указать порядковый номер истории");
                    index--;
                    if (index < 0)throw new BrakeFlowCallException("Номер должен быть больше 0");
                        var allStories = DatabaseHelper.GetAllStories(_connectionString);
                    var count = allStories.Length;
                    if(index>=count)throw new BrakeFlowCallException("Индекс должен быть меньше, чем кол-во историй");
                    DatabaseHelper.DeleteStory(index, _connectionString);
                    
                })
                .EchoReply(new OneLanguageString("История удалена"));

            _privateEngine.AddCommandWithParameter("admin",new OneLanguageString("Введите код доступа"))
                .ForEach(step =>
                {
                    step.Execute(context =>
                    {
                        if (context[0] != string.Empty) //todo: low may be redundant
                        {
                            var code = context[0];
                            if (code != ConfigurationManager.AppSettings["ADMIN_PASSWORD"]) throw new BrakeFlowCallException("Неверный код");
                            var sessionId = Guid.NewGuid();

                            List<string> authorizedCommands = new List<string>();
                            _sessions.Add(sessionId, context.UserInChat);
                            foreach (var command in commands)
                            {
                                authorizedCommands.Add(@"/" + command + $" {sessionId}");
                            }
                            context.ReplyEcho("Выберите команду",
                                EchoOptions.EchoReplyButtons(authorizedCommands.ToArray()));

                        }
                    });
                });
        }

#if ONECHAT_DEBUG
        private long _oneChatId;
#endif

        private Step AppendAuthorizedCommand(string commandName,List<string> commands, Action<CallContext> doAction=null)
        {
            commands.Add(commandName);
            
            return _privateEngine.AddCommand(commandName, true)
                .Execute(context =>
                {//проверяем код
                    var sessionIdText = context[0];
                    Guid sessionId;
                    if (!Guid.TryParse(sessionIdText, out sessionId))
                    {
                        throw new BrakeFlowCallException(@"Пожалуйста, авторизуйтесь с помощью команды /admin");
                    }

                    if (!_sessions.ContainsKey(sessionId))
                    {
                        throw new BrakeFlowCallException(@"Неверный ключ сессии. Возможно сессия уже истекла");
                    }

                    if (! Equals(_sessions[sessionId],context.UserInChat))
                    {
                        throw new BrakeFlowCallException("Вы не авторизованы");
                    }

                    doAction?.Invoke(context);
                });
        }
    }
}