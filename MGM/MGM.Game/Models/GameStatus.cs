using System.Runtime.Serialization;
using MGM.Game.States;

namespace MGM.Game.Models
{
    /// <summary>
    /// Game status - less detailed than <see cref="GameState"/>
    /// </summary>
    [DataContract]
    public enum GameStatus
    {
        /// <summary>
        /// Game was not started yet. It can be tuned, players can join, exit and start the game
        /// </summary>
        [EnumMember]
        New,
        /// <summary>
        /// State when every body discuss who should be killed.
        /// Any one player can be killed by voting
        /// </summary>
        [EnumMember]
        Day,
        /// <summary>
        /// During night time mafia, police and doctor do act
        /// </summary>
        [EnumMember]
        Night,

        /// <summary>
        /// Game is finished. Only status can be retrieved
        /// </summary>
        [EnumMember]
        Over,

    }
}