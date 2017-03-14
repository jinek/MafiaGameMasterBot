using System;
using MGM.Game.States;

namespace MGM.Game.DependencyInversion
{
    public interface IDelayer
    {
        void Delayed(GameState state,Game game,Action doAction, TimeSpan? delay = null);
    }
}