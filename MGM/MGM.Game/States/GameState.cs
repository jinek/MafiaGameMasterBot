using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MGM.BotFlow.Processing;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    [KnownType(typeof(NewGameState)),KnownType(typeof(DayState)), KnownType(typeof(PreVotingState)), KnownType(typeof(FinalVotingState)), KnownType(typeof(NightState)), KnownType(typeof(EndState)), KnownType(typeof(AutoGameState)),KnownType(typeof(VotingStateBase))]
    public abstract class GameState
    {
        [DataMember]
        private readonly DateTime _creationTimeStamp;
        [DataMember]
        private readonly GameStatus _status;

        protected GameState(GameStatus status, GameState oldState) : this(status)
        {
            Game = oldState.Game;
            _creationTimeStamp = DateTime.Now;
        }

        private GameState(GameStatus status)
        {
            _status = status;
        }

        public DateTime CreationTimeStamp => _creationTimeStamp;

        public GameStatus Status => _status;

        [IgnoreDataMember]
        protected internal Game Game { get; internal set; }

        /// <summary>
        ///     Enter to state machine
        /// </summary>
        [DataContract]
        public class NewGameState : GameState
        {
            public NewGameState(Game game) : base(GameStatus.New)
            {
                Game = game;
            }

            public GameState StartGame()
            {
                return new DayState(this);
            }

            [DataMember]
            public bool Distributed { get; private set; }
            public void DistributePlayers()
            {
                
                var players = Game.Players;

                var count = players.Count;
                if (count < 3) throw new GameCommandException(LocalizedStrings.GameState_PlayersShouldBeGreater3);
                if (count > 8) throw new GameCommandException(LocalizedStrings.GameState_PlayerShouldBeLess8);

                var playersDistribution = PlayersDistribution.Distributions[count];
                playersDistribution.Distribute(Game);
                Distributed = true;
            }

            public class PlayersDistribution
            {
                public static readonly PlayersDistribution[] Distributions =
                {
                    null, null, null,
                    new PlayersDistribution(1), //3
                    new PlayersDistribution(1), //4
                    new PlayersDistribution(1, true,winOnPoliceDie:true), //5 
                    new PlayersDistribution(2, true, true,true), //6 
                    new PlayersDistribution(2, true, true,true), //7 
                    new PlayersDistribution(2, true, true,true) //8 
                };

                private readonly bool _hasDoctor;
                private readonly bool _hasPolice;
                private readonly bool _winOnPoliceDie;
                private readonly int _mafiaCount;

                private PlayersDistribution(int mafiaCount, bool hasDoctor = false, bool hasPolice = false,bool winOnPoliceDie=false)
                {
                    _mafiaCount = mafiaCount;
                    _hasPolice = hasPolice;
                    _winOnPoliceDie = winOnPoliceDie;//todo: low may be should always be true
                    _hasDoctor = hasDoctor;
                }

                public void Distribute(Game game)
                {
                    var left = new List<Player>(game.Players);
                    //selecting mafia
                    for (var i = 0; i < _mafiaCount; i++)
                    {
                        var newMafia = GetRandomAndExclude(left);
                        game.Mafias.Add(newMafia);
                        SayToUser(game,newMafia.User,LocalizedStrings.GameState_MafiaWord);
                    }
                    //policeman
                    if (_hasPolice)
                    {
                        var police = GetRandomAndExclude(left);
                        game.Police = police;
                        SayToUser(game, police.User, LocalizedStrings.GameState_PolicemanWord);
                    }
                    //doctor
                    if (_hasDoctor)
                    {
                        var doctor = GetRandomAndExclude(left);
                        game.Doctor = doctor;
                        SayToUser(game, doctor.User, LocalizedStrings.GameState_DoctorWord);
                    }

                    foreach (var player in left)
                    {
                        SayToUser(game,player.User,LocalizedStrings.GameState_CivilianWord);
                    }
                    game.WinOnPoliceDie = _winOnPoliceDie;
                }

                private void SayToUser(Game game, User user,string whoIs)
                {
                    try
                    {
                        game.GameStateBannerProvider.PrivateMessage(user,
                            string.Format(LocalizedStrings.GameState_YouAreXInCityY, whoIs, game.Name));
                    }
                    catch (ApiChatException)//todo: сейчас нельзя отличить ошибки
                    {
                        throw new GameCommandException(string.Format(user.Username,user.Username));
                    }
                        
                }

                private static readonly Random Rnd = new Random(Environment.TickCount);

                private Player GetRandomAndExclude(List<Player> left)
                {
                    var choice = left[Rnd.Next(0, left.Count)];
                    if(!left.Remove(choice))throw new InvalidOperationException();
                    return choice;
                }

                public string ToString(int allCount)
                {
                    var civilCount = allCount - _mafiaCount;
                    if (_hasDoctor) civilCount--;
                    if (_hasPolice) civilCount--;
                    return $"{LocalizedStrings.GameState_MafiaWord} - {_mafiaCount}, "
                        + (_hasPolice ? LocalizedStrings.GameState_PolicemanWord+", " : "")
                        + (_hasDoctor ? LocalizedStrings.GameState_DoctorWord+", " : "")
                        + ($"{LocalizedStrings.GameState_CivilianWord} - {civilCount}, "
                        +(_winOnPoliceDie?LocalizedStrings.Distributing_LooseOnPoliceDie:LocalizedStrings.Distributing_LooseOnCiviliansDie));
                }
            }
        }

        public override string ToString()
        {
            switch (Status)
            {
                case GameStatus.New:
                    return LocalizedStrings.GameState_GameNotStarted;
                case GameStatus.Over:
                    return $@"{LocalizedStrings.GameState_GameFinished}.
{this.Cast<EndState>()}";
                case GameStatus.Day:
                    return string.Format(LocalizedStrings.DayState_DayCaption, Game.DayCount);//todo: low copypaste with smth
                case GameStatus.Night:
                    return string.Format(LocalizedStrings.GameState_NightCaption, Game.DayCount);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}