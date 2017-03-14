using System;
using MGM.BotFlow.Steps;

namespace MGM.BotFlow.Extensions
{
    public class StepCollection
    {
        readonly Step[] _steps;

        internal StepCollection(Step[] steps)
        {
            _steps = steps;
        }

        public void ForEach(Action<Step> doAction)
        {
            foreach (var commandStep in _steps)
            {
                doAction(commandStep);
            }
        }
    }
}