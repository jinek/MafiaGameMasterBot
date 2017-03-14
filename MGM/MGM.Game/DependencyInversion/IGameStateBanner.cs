using System.Collections.Generic;

namespace MGM.Game.DependencyInversion
{
    public interface IGameStateBanner
    {
        void Update(Dictionary<string, string> pars);
        void Terminate();
        void Update(string newText);
        string Text { get;}
        int Id { get; }
    }
}