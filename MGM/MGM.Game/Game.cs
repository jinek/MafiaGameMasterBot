using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MGM.Game.DependencyInversion;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Game.States;
using MGM.Localization;

namespace MGM.Game
{
    //https://msdn.microsoft.com/en-us/library/ms733734(v=vs.110).aspx
    [DataContract]
    public sealed class Game
    {
        public long ChatId => _chatId;//todo: low Actually should be taken from db

        public int Id => _id;

        [DataMember]
        private readonly string _name;

        [DataMember]
        private readonly List<Player> _players = new List<Player>();

        [DataMember]
        private readonly List<Player> _mafias = new List<Player>(2);

        [DataMember]
        private readonly List<Player> _dead = new List<Player>();

        [DataMember]
        private readonly int _id;

        [IgnoreDataMember]
        private IDelayer _delayer;

        [IgnoreDataMember]
        private IStatShower _statShower;

        [IgnoreDataMember]
        private IRandomStoryTeller _storyTeller;

        [DataMember]
        private readonly long _chatId;

        public Game(string name, IGameStateBannerProvider gameStateBannerProvider, int id,IDelayer delayer,IRandomStoryTeller storyTeller,long chatId, IStatShower statShower)
        {
            _storyTeller = storyTeller;
            _delayer = delayer;
            _name = name;
            GameStateBannerProvider = gameStateBannerProvider;
            _id = id;
            _chatId = chatId;
            _statShower = statShower;
        }

        [DataMember]
        public int DayCount { get; internal set; }

        [DataMember]
        public GameState State { get; set; }

        
        public List<Player> Players => _players;

        public List<Player> Mafias => _mafias;

        [DataMember]
        public Player Police { get; set; }

        [DataMember]
        public Player Doctor { get; set; }

        [DataMember]
        public IGameStateBannerProvider GameStateBannerProvider { get; private set; }

        public List<Player> Dead => _dead;

        public bool HasWinner => CitizenWins || MafiaWins;

        public bool MafiaWins => Mafias.Where(IsAlive).Count()*2 >= Players.Where(IsAlive).Count() || (Police!=null && Dead.Contains(Police) && WinOnPoliceDie);

        public bool CitizenWins => Mafias.All(mafia => Dead.Contains(mafia));

        [IgnoreDataMember]
        public string Name => _name;

        [IgnoreDataMember]
        public IStatShower StatShower => _statShower;

        [IgnoreDataMember]
        public IRandomStoryTeller StoryTeller => _storyTeller;

        [DataMember(IsRequired = false)]
        public bool WinOnPoliceDie { get; set; }

        public Player Ready(User user)
        {
            CheckNotDistributed();
            
            var maxCount = GameState.NewGameState.PlayersDistribution.Distributions.Length;
            if(Players.Count==maxCount)throw new GameCommandException(LocalizedStrings.Game_TooMuchPlayers);
            var player = new Player(user);
            if (Players.Contains(player))
                throw new GameCommandException(LocalizedStrings.Game_YouAreInGameAlready);
            if (Players.Any(pl => pl.Id == player.Id))
            {
                throw new GameCommandException(LocalizedStrings.Fault_TheSameId);
            }
            Players.Add(player);
            return player;
        }

        public void Unready(User user)
        {
            CheckNotDistributed();
            var player = Players.SingleOrDefault(pl => pl.Id == user.Id);
            if(player==null)
                throw new GameCommandException(LocalizedStrings.Game_YouAreNotInGame);
                
            Players.Remove(player);
        }

        public void Go()
        {
            var newGameState = CheckNotDistributed();
            newGameState.DistributePlayers();
            Delayed(State,() =>
            {
                State = newGameState.StartGame();
            },TimeSpan.FromSeconds(10));
            
        }

        private GameState.NewGameState CheckNotDistributed()
        {
            var newGameState = State.Cast<GameState.NewGameState>();
            if (newGameState == null) throw new GameCommandException(LocalizedStrings.Game_AlreadyStarted);
            if (newGameState.Distributed) throw new GameCommandException(LocalizedStrings.Game_PlayersDistributedAlready);
            return newGameState;
        }

        public PlayerRole? PrivateVote(User user, string voteKey)
        {
            State.CheckState<NightState>(LocalizedStrings.Game_NotVotingTime);
            var nightState = State.Cast<NightState>();
            return nightState.Vote(user,voteKey);
        }

        public void PublicVote(User user, string voteKey)
        {
            State.CheckState<VotingStateBase>(LocalizedStrings.Game_NotVotingTime);
            var votingState = State.Cast<VotingStateBase>();
            votingState.Vote(user, voteKey);
        }

        public string GetStatus()
        {
            string result = State + @"
";
            result += GetPlayerStatuses(State is EndState);
            var count = Players.Count;
            result += GameState.NewGameState.PlayersDistribution.Distributions[count]?.ToString(count);
            return result;
        }

        internal string GetPlayerStatuses(bool forceWhoIsWho=false)
        {
            string result = string.Empty;
            foreach (var player in Players)
            {
                var isAlive = IsAlive(player);
                result += $@"{player.Username}{(!isAlive?$" ({LocalizedStrings.Game_DeadWord}) ":"")}{(!isAlive||forceWhoIsWho ? $" {this.GetRoleText(player)}": "")}
";
            }
            return result;
        }

        public bool IsAlive(Player player)
        {
            return !Dead.Contains(player);
        }

        public Player[] GetAlivePlayers()
        {
            return Players.Where(IsAlive).ToArray();
        }
        
        public void ForcePrevoting()
        {
            State.CheckState<DayState>(LocalizedStrings.GameState_GameNotStarted);
            State.Cast<DayState>().ForceVoting();
        }

        public void Timer(bool force = false)
        {
            if (State is AutoGameState)
            {
                var newState = State.Cast<AutoGameState>().Timer(force);
                if (newState != null) State = newState;
            }
        }

        public void Delayed(GameState owner,Action doAction,TimeSpan? delay=null)
        {
            _delayer.Delayed(owner,this,doAction,delay);
        }

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext streamingContext)
        {
            State.Game = this;
            _delayer = (IDelayer) streamingContext.Context;
            _storyTeller = (IRandomStoryTeller) streamingContext.Context;
            _statShower = (IStatShower) streamingContext.Context;
        }

        public void Abort(bool bySystem=false)
        {
            if (State.Cast<EndState>() != null)
            {
                if (bySystem) return;
                throw new GameCommandException(LocalizedStrings.GameState_GameFinished);
            }

            State = new EndState(State,true,bySystem);
        }
    }
}