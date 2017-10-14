using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.Game.DependencyInversion;
using MGM.Game.ForImplementors;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.Helpers;
using MGM.Game.States;
using MGM.Helpers;
using MGM.Localization;
using MGM.TelemetryGlobal;
using Telegram.Bot;
using Telerik.OpenAccess;
using InvalidOperationException = System.InvalidOperationException;
using Player = MGM.Game.Persistance.Database.DataModels.Game.Player;

namespace MGM.Game.Persistance.Game
{
    public sealed class GameProvider : IApiContainer,IDelayer, IRandomStoryTeller, IStatShower, IApiChatLimiterProvider
    {
        private static readonly NetDataContractSerializer Serializer = new NetDataContractSerializer();
        private readonly ConcurrentDictionary<int, MGM.Game.Game> _cachedGames = new ConcurrentDictionary<int, MGM.Game.Game>();
        private readonly string _connectionString;
        //one chat can create only one game
        private readonly LockerCollection<Chat> _gameCreationLockers = new LockerCollection<Chat>();
        private readonly LockerCollection<int> _lockers = new LockerCollection<int>();

        public GameProvider(string connectionString, Api api, ApiChatLimiter limiter)
        {
            _connectionString = connectionString;
            Api = api;
            _limiter = limiter;
        }

        public MGM.Game.Game GetGameForUser(UserInChat userInChat, bool isPrivate)
        {
            return UsingDb(db =>
            {
                {
                    int gameIdToLoad;
                    if (!isPrivate)
                    {
                        //Retrieve game for this chat
                        var chatId = userInChat.ChatId;
                        var chat = db.ChatInTelegrams.ById(chatId);
                        var thisChatGames = chat.Games;

                        var notFinishedGame =
                            thisChatGames.SingleOrDefault(
                                game =>
                                    game.FinishTime == null ||
                                    game.FinishTime > DateTime.Now-EndState.EndStateMinMinutesTime);
                        if (notFinishedGame == null)
                        {
                            var locker = _gameCreationLockers.GetOrCreateLocker(userInChat.Chat); //todo low clean
                            lock (locker)
                            {
                                notFinishedGame = new Database.DataModels.Game.Game
                                {
                                    ChatInTelegram = chat,
                                    CreationTime = DateTime.Now,
                                    LastAccessTime = DateTime.Now
                                };
                                db.Add(notFinishedGame);
                                db.SaveChanges();
                                db.Refresh(RefreshMode.OverwriteChangesFromStore, notFinishedGame);


                                var newGameP = new MGM.Game.Game(userInChat.Chat.Title,
                                    new GameStateBannerProvider(new ApiChat(Api, userInChat.Chat, _limiter)), notFinishedGame.Id,
                                    this, this,chatId,this);
                                newGameP.State = new GameState.NewGameState(newGameP);
                                notFinishedGame.SerializedGame = SerializeGame(newGameP);
                                db.SaveChanges();
                                db.Refresh(RefreshMode.OverwriteChangesFromStore, notFinishedGame);
                            }
                        }

                        gameIdToLoad = notFinishedGame.Id;
                    }
                    else
                    {
                        //looking for games with this player, if many - choose first one that is awaiting for vote
                        var userInTelegram = db.UserInTelegrams.ById(userInChat.UserId);

                        var gameOfThrones = db.Players
                            .Where(player => player.UserInTelegram == userInTelegram)
                            .Where(player => player.Game.IsNight)
                            .Where(player => player.Game.FinishTime == null)
                            .Where(player => player.PutToVoting != null)
                            .OrderBy(player => player.PutToVoting) //first is earliest
                            .FirstOrDefault()
                            ?.Game;

                        if (gameOfThrones == null)
                        {
//если игра не найдена
                            throw new BrakeFlowCallException(LocalizedStrings.GameProvider_NoGamesYouCanVote);
                        }
                        gameIdToLoad = gameOfThrones.Id;
                    }

                    return InternalLoadFromCacheOrDb(gameIdToLoad);
                }
            });
        }

        private MGM.Game.Game InternalLoadFromCacheOrDb(int gameId)
        {
            var gameGettingLock = _lockers.GetOrCreateLocker(gameId);

            lock (gameGettingLock)
            {
                MGM.Game.Game game;
                if (!_cachedGames.TryGetValue(gameId, out game))
                {
                    UsingDb(db =>
                    {
                        var gamePerc = db.Games.ById(gameId);
                        var serializedGame = gamePerc.SerializedGame;
                        game = Deserialize(serializedGame);
                        if (!_cachedGames.TryAdd(gameId, game))
                            throw new InvalidOperationException("Game already exists");
                    });
                }
                return game;
            }
        }

        [NotNull]
        private string SerializeGame([NotNull] MGM.Game.Game game)
        {
            var memoryStream = new MemoryStream();
            Serializer.WriteObject(memoryStream, game);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        [NotNull]
        private MGM.Game.Game Deserialize([NotNull] string serializedGame)
        {
            var contractSerializer =
                new NetDataContractSerializer(new StreamingContext(StreamingContextStates.All,
                    this));
            return (MGM.Game.Game) contractSerializer.ReadObject(new XmlTextReader(new StringReader(serializedGame)));
        }

        public MGM.Game.Game[] GetGamesForTimer(DateTime now)
        {
            var gameIds = UsingDb(db =>
            {
                return db.Games
                    .NotFinished()
                    .Where(game => game.MaxWakeupTime < now)
                    .Select(game => game.Id)
                    .ToArray();
            });

            return gameIds.Select(InternalLoadFromCacheOrDb).ToArray();
        }

        /// <summary>
        ///     It's not thread safe and must be called from game lock
        /// </summary>
        public void SaveGame(MGM.Game.Game game)
        {
            UsingDb(db =>
            {
                var gameP = db.Games.ById(game.Id);
                gameP.LastAccessTime = DateTime.Now;
                gameP.SerializedGame = SerializeGame(game);

                var creationTimeStamp = game.State.CreationTimeStamp;
                if (gameP.FinishTime == null)
                    if (game.State.Status == GameStatus.Over)
                    {
                        //if game is finished - save when and update gamers rating
                        gameP.FinishTime = creationTimeStamp;
                        
                        if (!game.State.Cast<EndState>().Abort)//if game was not aborted
                        {
                            foreach (var player in gameP.Players)
                            {
                                player.UserInTelegram.AllGameCount++;
                            }

                            if (game.MafiaWins)
                            {
                                foreach (var mafia in game.Mafias)
                                {
                                    db.UserInTelegrams.ById(mafia.Id).WinCount ++;
                                }
                            }
                            else
                            {
                                if (!game.CitizenWins) throw new InvalidOperationException("How it could be so?");
                                var mafiaIds = game.Mafias.Select(player1 => player1.Id).ToArray();
                                foreach (var civilian in gameP.Players.Where(player => !mafiaIds.Contains(player.Id)))
                                {
                                    civilian.UserInTelegram.WinCount ++;
                                }
                            }
                            
                        }
                    }

                if (game.State is AutoGameState)
                {
                    var timeLeft = game.State.Cast<AutoGameState>().TimeLeft;

                    gameP.MaxWakeupTime = DateTime.Now + timeLeft -
                                          (timeLeft > TimeSpan.FromSeconds(AutoGameState.WarnBeforeEndInSecond)
                                              ? TimeSpan.FromSeconds(20)
                                              : TimeSpan.Zero);
                }
                else
                {
                    gameP.MaxWakeupTime = null;
                }

                if (game.State is GameState.NewGameState)
                {
                    //everytime resave players
                    //todo: low perfomance impact
                    db.Delete(gameP.Players);
                    foreach (var player in game.Players)
                    {
                        db.Add(new Player
                        {
                            Game = gameP,
                            UserId = player.Id//todo: low
                        });
                    }
                }
                if (game.State is NightState)
                {
                    gameP.IsNight = true;
                    foreach (var mafia in game.Mafias)
                    {
                        gameP.Players.Single(player => player.UserId == mafia.Id).PutToVoting =
                            creationTimeStamp;
                    }

                    if (game.Police != null && game.IsAlive(game.Police))
                        gameP.Players.Single(player => player.UserId == game.Police.Id).PutToVoting =
                            creationTimeStamp;

                    if (game.Doctor != null && game.IsAlive(game.Doctor))
                        gameP.Players.Single(player => player.UserId == game.Doctor.Id).PutToVoting =
                            creationTimeStamp; //todo: low too much queryable clones 
                }
                else
                {
                    gameP.IsNight = false;
                }

                db.SaveChanges();
            });
        }

        /// <summary>
        /// See code
        /// </summary>
        /// <returns>Returns True if User was created</returns>
        /// <exception cref="GameCommandException">if createIfNotExist is false and user does not exist</exception>
        public bool CheckUserInTelegram(long userId, long? privateChatId=null,long? publicChatId=null)
        {
            return UsingDb(db =>
            {
                var userInTelegram = db.UserInTelegrams.ById(userId);//If if was called, user exists

                if (userInTelegram.PrivateChat == null)
                {
                    if (privateChatId == null)
                    {
                        if(publicChatId!=null)
                        {
                            var userInPublicChat = userInTelegram.UserInChats.Single(userInChat => userInChat.ChatId==publicChatId);
                            userInPublicChat.WantToBeReady = true;
                            db.SaveChanges();
                        }
                        
                        throw new GameCommandException(LocalizedStrings.GameProvider_MessageMeToPrivateChat);
                    }
                    
                    var chatInTelegram = db.ChatInTelegrams.ById((long) privateChatId);
                    userInTelegram.PrivateChat = chatInTelegram;
                    db.SaveChanges();
                    return true;
                }
                return false;
            });
        }

        public Api Api { get; }
        public void Delayed(GameState ownerState,MGM.Game.Game game,Action doAction, TimeSpan? delay = null)
        {
            if (delay == null) delay = TimeSpan.FromSeconds(3);
            // ReSharper disable once LocalizableElement
            if (delay > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(nameof(delay), "Delayed action must occur within 1 minute");
            var language = LocalizedStrings.Language;
            var telemetry = TelemetryStatic.TelemetryClient;
            RunInThreadPoolAndReportErrors(() =>
            {
                Thread.Sleep((TimeSpan)delay);
                LocalizedStrings.Language = language;
                TelemetryStatic.TelemetryClient = telemetry;
                lock (game)//todo: this action (receive game, lock it, process and save) is repeted in several places - must be extracted to somewhere
                {
                    if (ownerState != game.State) return; //silently give up, state was changed
                    try
                    {
                        doAction();
                    }
                    catch (ApiChatBadRequestException )
                    {
                        try
                        {
                            game.Abort(true);
                        }
                        catch (ApiChatBadRequestException )
                        {
                        }
                    }
                    SaveGame(game);
                }
            });
        }

        public void RunInThreadPoolAndReportErrors(Action action)//todo: awesome not clear why this method is here. (In fact it's here coz this is highest level we need it). I mean why threadpool running belongs to GameProvider?!?
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    RaiseError(exception);
                }
            });
        }

        public event Action<Exception> OnError;

        public bool HasGameStepInFuture(DateTime now)
        {
            return UsingDb(db =>
            {
                return db.Games
                    .NotFinished()
                    .Any(game => game.MaxWakeupTime >= now);
            });
            
        }

        private readonly Random _storyRandom = new Random(DateTime.Now.Millisecond);
        private readonly ApiChatLimiter _limiter;

        public void ShowRandomStory(MGM.Game.Game game)
        {
            UsingDb(db =>
            {
                var count = db.Stories.Count();
                if (count == 0) return;//doing nothing - it can be so, just no stories from admin
                var storyToTellIndex = _storyRandom.Next(count);
                var story = db.Stories.Skip(storyToTellIndex).Take(1).Single();
                game.GameStateBannerProvider.ShowPhoto(story.PhotoId, story.Body);
            });
        }

        public void ShowStat(MGM.Game.Game game)
        {//8/3/2016 2:46:18 AM
            UsingDb(db =>
            {
                string stat = $@"{LocalizedStrings.Stat_PlayerStatHeader}
{LocalizedStrings.Stat_PlayerWin}/{LocalizedStrings.Stat_PlayerGamesCount}
";
                var users = db.Games.ById(game.Id).Players.Select(player => player.UserInTelegram).ToArray();
                foreach (var userInTelegram in users)
                {
                    stat += $@"*{game.Players.ById(userInTelegram.Id).Username}* -{userInTelegram.WinCount}/{userInTelegram.AllGameCount}
";
                }
                game.GameStateBannerProvider.CreateBanner(stat);
            });
        }

        

        public void UsingDb(Action<DbContext> doAction)
        {
            UsingDb<object>(context =>
            {
                doAction(context);
                return null;
            });
        }

        public T UsingDb<T>(Func<DbContext, T> doAction)
        {
            using (var db = new DbContext(_connectionString))
            {
                return doAction(db);
            }
        }

        public string GetStat()
        {
            return UsingDb(db =>
            {
                
                return $@"Games: {db.Games.Count()}(Played {db.Games.Count(game => game.FinishTime != null)}) Users:{db.UserInTelegrams.Count()} Game Chats:{db.Games.GroupBy(game => game.ChatInTelegram).Count()}
Current games: {db.Games.Count(game => game.FinishTime == null && game.MaxWakeupTime != null)}
Subscribed: {db.ChatInTelegrams.Count(chat => chat.Subscribed)}";
            });
        }

        public uint GetLanguageForChat(long chatId)
        {
            return UsingDb(context => context.ChatInTelegrams.ById(chatId)?.LanguageIndex ?? 0);
        }

        private void RaiseError(Exception obj)
        {
            OnError?.Invoke(obj);
        }

        public ApiChatLimiter GetLimiter()
        {
            return _limiter;
        }
    }
}