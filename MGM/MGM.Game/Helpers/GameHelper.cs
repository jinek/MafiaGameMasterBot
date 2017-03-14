using System.Collections.Generic;
using System.Linq;
using MGM.Game.Models;
using MGM.Game.States;
using MGM.Localization;

namespace MGM.Game.Helpers
{
    public static class GameHelper
    {
        public static T Cast<T>(this GameState state) where T:GameState
        {
            return state as T;
        }

        public static void CheckStatus(this GameState state,GameStatus status,string errorText)
        {
            if (state.Status == status) return;
            throw new GameCommandException(errorText);
        }

        public static void CheckState<T>(this GameState state, string errorText) where T : GameState
        {
            if(state is T)return;
            throw new GameCommandException(errorText);
        }

        public static Player ById(this IEnumerable<Player> players,long id)
        {
            var player = players.SingleOrDefault(pl=>pl.Id==id);
            return player;
        }

        public static Player CheckUserIsPlayer(this Game game, User user,string notPlayerMessage=null)
        {
            if (notPlayerMessage == null) notPlayerMessage = LocalizedStrings.GameHelper_UserIsNotInGame;
            var id = user.Id;
            return game.CheckUserIsPlayer(id, string.Format(notPlayerMessage,user.Username));
        }

        public static Player CheckUserIsPlayer(this Game game, long id, string notPlayerMessage)
        {
            var player = game.Players.ById(id);
            if (player == null) throw new GameCommandException(notPlayerMessage);
            return player;
        }

        public static void CheckPlayerIsAlive(this Game game, Player player, string notAliveMessage = null,
            params object[] formatParameters)
        {
            if (notAliveMessage == null) notAliveMessage = LocalizedStrings.GameHelper_UserIsDead;
            if (formatParameters.Length == 0) formatParameters = new object[] { player.Username };
            if (!game.IsAlive(player)) throw new GameCommandException(string.Format(notAliveMessage, formatParameters));
        }

        public static Player CheckUserIsPlayerAndAlive(this Game game, User user)
        {
            var player = game.CheckUserIsPlayer(user);
            game.CheckPlayerIsAlive(player);
            return player;
        }

        public static string GetNameAndRole(this Game game, Player player)
        {
            return $"{player.Username} - {game.GetRoleText(player)}";
        }

        public static string GetRoleText(this Game game,Player player)
        {
            if (Equals(game.Doctor, player)) return LocalizedStrings.GameState_DoctorWord;
            if (Equals(game.Police, player)) return LocalizedStrings.GameState_PolicemanWord;
            if (game.Mafias.Contains(player)) return LocalizedStrings.GameState_MafiaWord;
            return LocalizedStrings.GameState_CivilianWord;
        }
    }
}