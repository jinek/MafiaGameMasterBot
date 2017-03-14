using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MGM.BotFlow;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.Game.Persistance;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.DataModels.Telegram;
using MGM.Game.Persistance.Game;
using MGM.Game.Persistance.State;
using MGM.Localization;
using MGM.TelemetryGlobal;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MGM.Game.Engine
{
    public partial class GameEngine
    {
        private readonly string _connectionString;
        private readonly GameProvider _gameProvider;
        private readonly Api _api = new Api(ConfigurationManager.AppSettings["TELEGRAM_APIKEY"]);
        private readonly FlowEngine _privateEngine;
        private readonly FlowEngine _publicEngine;
        public string VersionNumber;
        private string _history;

        public GameEngine(string connectionString, string versionNumber, string versionFile)
        {
            _connectionString = connectionString;
            DatabaseHelper.Actualize(connectionString);
            _gameProvider = new GameProvider(connectionString,_api,_limiter);
            IStateProvider stateProvider = new DatabaseStateProvider(connectionString);
            _privateEngine = new FlowEngine(_api,stateProvider, _limiter);
            _publicEngine = new FlowEngine(_api, stateProvider, _limiter);

            BuildCommonFlow();
            BuildAdminFlow();
            BuildPrivateFlow();
            BuildPublicFlow();

            CheckVersion(versionNumber,versionFile);
            RunStatistic();
        }
        
        public event Action<Exception> OnError
        {
            add { _gameProvider.OnError += value; }
            remove { _gameProvider.OnError -= value; }
        }

        private void CheckVersion(string versionNumber,string versionFile)
        {
            var versionText = versionFile;
            var strings = versionText.Split(new[] {"||"}, StringSplitOptions.None);
            _history = strings[1];

            VersionNumber = strings[0];
            if (versionNumber != VersionNumber)
            {
               _gameProvider.UsingDb(
                        context =>
                        {
                            TelemetryStatic.TelemetryClient = new TelemetryClient();
                            var parentId = TelemetryStatic.TelemetryClient.Context.Operation.Id;
                            var subscribedChats = context.ChatInTelegrams.Where(chat => chat.Subscribed).ToArray();

                            Parallel.ForEach(subscribedChats, chat =>
                            {
                                TelemetryStatic.TelemetryClient = new TelemetryClient();
                                var operationContext = TelemetryStatic.TelemetryClient.Context.Operation;

                                operationContext.ParentId = parentId;
                                operationContext.Name = "Update notification";
                                TelemetryStatic.TelemetryClient.Context.Properties[TelemetryStatic.ChatKey] = chat.Id.ToString();

                                LocalizedStrings.Language = chat.LanguageIndex;
                                var chatId = chat.Id;
                                var apiChat = new ApiChat(_api, new BotFlow.Persistance.Chat(chatId, ""), _limiter);
                                try
                                {
                                    apiChat.Echo(LocalizedStrings.GameUpdatedMessage, 0, null);
                                }
                                catch (ApiChatException exception)
                                {
                                    var apiRequestException = exception.InnerException as ApiRequestException;
                                    if (apiRequestException != null && apiRequestException.ErrorCode != 400) throw;//это кейс что Chat was deactivated
                                }
                                //todo: should return it context.Delete(chat);
                            });
                            //context.SaveChanges();
                        });
            }
        }

        public Api Api => _api;

        private static bool _typingTyped;

        private readonly ApiChatLimiter _limiter = new ApiChatLimiter();

        public bool ProcessUpdate(Update update)
        {
            try
            {
                if (!IsSupportedUpdate(update)) return false;

                var chatId = update.GetChat().Id;
                TelemetryStatic.TelemetryClient.Context.Properties[TelemetryStatic.ChatKey] = chatId.ToString();

                if (!_typingTyped)
                {
                    _typingTyped = true;//no locks - fine to do it several times
                    
                    TelemetryStatic.TelemetryClient.TrackEvent("Typing",new Dictionary<string, string> {[TelemetryStatic.ToChatKey]=chatId.ToString()});//todo: low incapsulate telemetry so all this assemblies don't have to depend on microsoft insight
                    _api.SendChatAction(chatId, ChatAction.Typing);
                }
#if ONECHAT_DEBUG
                bool oneChatProcess = false;
                var realChatId = update.GetMessage().Chat.Id;

                if (update.Type == UpdateType.MessageUpdate && update.Message.Type == MessageType.TextMessage &&
                    update.GetMessage().Text.Contains("Sobachka2"))
                {
                    _oneChatId = realChatId;
                    _api.SendTextMessage(realChatId, "You are in one chat debug now");

                    return true;
                }

                if (_oneChatId==realChatId)
                {
                    oneChatProcess = true;
                }

                if (!oneChatProcess)
                {
                    _api.SendTextMessage(realChatId, LocalizedStrings.Servicing_Now);
                    return true;
                }
#endif

                var language = _gameProvider.GetLanguageForChat(chatId);
                LocalizedStrings.Language = language;

                IState finalState;
                var isPrivate = update.IsPrivate();
                if (isPrivate)
                {
                    _privateEngine.Process(update, _gameProvider, out finalState);
                }
                else
                {
                    _publicEngine.Process(update, _gameProvider, out finalState);
                }
                ((DatabaseState) finalState).SaveAndDispose();
            }
            catch (ChatException chatException)
            {
                //здесь не выносим за дебаг потому что он кидается только за дебагом

                chatException.Chat.Echo(LocalizedStrings.GameEngine_ErrorSorry, 0, null);

                // думаю не стоит этого делать, потому что пропадает инфа для отладки DatabaseStateProvider.CleanStateForChat(new UserInChat(update.GetChat().ToChat(), update.GetUser()), _connectionString);
            }
            return true;
        }

        public bool Timer()
        {
            var now = DateTime.Now;
            // ReSharper disable once InconsistentlySynchronizedField
            var games = _gameProvider.GetGamesForTimer(now);
            TelemetryStatic.TelemetryClient = new TelemetryClient();
            TelemetryStatic.TelemetryClient.TrackMetric("Active Game Count",games.Length);
            var parentId = TelemetryStatic.TelemetryClient.Context.Operation.Id;
            Parallel.ForEach(games, game =>
            {
                TelemetryStatic.TelemetryClient = new TelemetryClient();
                var operationContext = TelemetryStatic.TelemetryClient.Context.Operation;

                operationContext.ParentId = parentId;
                operationContext.Name = "Update notification";
                TelemetryStatic.TelemetryClient.Context.Properties[TelemetryStatic.ChatKey] = game.ChatId.ToString();
                TelemetryStatic.TelemetryClient.Context.Properties[TelemetryStatic.GameKey] = game.Id.ToString();

                lock (game)//todo: low I don't like this lock is public
                {
                    var language = _gameProvider.GetLanguageForChat(game.ChatId);
                    LocalizedStrings.Language = language;
                    game.Timer();
                    _gameProvider.SaveGame(game);
                }
            });
            // ReSharper disable once InconsistentlySynchronizedField
            return _gameProvider.HasGameStepInFuture(now);
        }

        private void RunStatistic()
        {
            _gameProvider.RunInThreadPoolAndReportErrors(() =>
            {
                Thread.Sleep(30000);
                while (true)
                {
                    _gameProvider.UsingDb(db =>
                    {
                        var telemetryClient = new TelemetryClient();
                        telemetryClient.TrackMetric("GamesCount", db.Games.Count());
                        telemetryClient.TrackMetric("GamesPlayedCount", db.Games.Count(game => game.FinishTime != null));
                        telemetryClient.TrackMetric("UserCount", db.UserInTelegrams.Count());
                        telemetryClient.TrackMetric("GameChatsCount",
                            db.Games.GroupBy(game => game.ChatInTelegram).Count());
                        telemetryClient.TrackMetric("CurrentGamesCount",
                            db.Games.Count(game => game.FinishTime == null && game.MaxWakeupTime != null));
                            //todo: low I assume this duplicates active game Count counter(metric)
                        telemetryClient.TrackMetric("SubscribedChatsCount",
                            db.ChatInTelegrams.Count(chat => chat.Subscribed));
                    });
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            });
        }

        private bool IsSupportedUpdate(Update update)
        {
            UpdateType updateType;
            try
            {
                updateType = update.Type;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            
            switch (updateType)
            {
                case UpdateType.MessageUpdate:
                    return update.Message.Type == MessageType.TextMessage || update.Message.Type == MessageType.PhotoMessage;
                case UpdateType.CallbackQueryUpdate:
                    return true;
                default:
                    return false;
            }
        }
    }
}
