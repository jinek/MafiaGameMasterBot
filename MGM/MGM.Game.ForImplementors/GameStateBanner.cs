using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MGM.BotFlow.Processing;
using MGM.Game.DependencyInversion;
using MGM.Localization;

namespace MGM.Game.ForImplementors
{
    [DataContract]
    public class GameStateBanner : IGameStateBanner
    {
        [DataMember]
        private readonly Echo _echo;
        [DataMember]
        private Dictionary<string, string> _pars;

        public GameStateBanner(Echo echo)
        {
            _echo = echo;
        }

        public void Update(string newText)
        {
            if (newText != _echo.Text)
            {
                _echo.Text = newText;//todo: low check should be occur inside echo
                _echo.Update(newText);
            }
        }

        public string Text => _echo.Text;
        public int Id => _echo.Id;

        public void Update(Dictionary<string, string> pars)
        {
            if (IsEqual(_pars, pars)) return;
            _pars = pars;
            _echo.Update(EchoOptions.EchoInlineButtons(pars));
        }

        private bool IsEqual(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
            if (d1?.Count != d2.Count) return false;

            return CompareDictionaryValues(d1, d2) && CompareDictionaryValues(d2, d1);
        }

        private static bool CompareDictionaryValues(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
            return d1.All(pair => d2.Keys.Contains(pair.Key) && d1[pair.Key] == d2[pair.Key]);
        }

        public void Terminate()
        {//todo: low terminate should be called not only coz of timer, but also coz of abortion
            _echo.Update(EchoOptions.SimpleEcho());
        }
    }
}