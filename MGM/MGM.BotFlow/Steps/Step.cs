using System;
using System.Collections.Generic;
using System.Linq;
using MGM.BotFlow.Executors;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    /// <summary>
    ///     Tree of states basis
    /// </summary>
    public abstract class Step
    {
        internal Dictionary<string, Step> Children; //May be this is logical fault that there are many children
        internal List<IExecutor> Executors;

        protected virtual bool Process(CallContext callContext, IState state, Update update)
        {
            var nextState = state.NextState; //todo: low: i don't like it acutally process next step, I think it should process curernt step

            if (nextState == null)
            {
                var nextStep = LocateNextChild(update);

                var parameter = nextStep.GetParameter(update);
                callContext.Push(parameter);
                nextStep.Execute(callContext, state);


                if (nextStep.Children == null)
                    return true;
                if (nextStep.Children.Count != 1 || !(nextStep.Children.Single().Value is AutomaticStep))
                    return false;

                nextState = state.NextState;
            }
            else
            {
                callContext.Push(nextState.Value);
            }

            var nextChildStep = Children[nextState.Id];
            bool reset = nextChildStep.Process(callContext, nextState, update);
            if (reset && nextChildStep.Children.Count > 1)
            {
                nextState.ClearForward();
                return false;
            }
            return reset;
        }

        internal virtual void Execute(CallContext callContext, IState state)
        {
            var newState = state.AddStepState(GetId(), callContext[0]);//todo: low i'm not sure it's right to add state before its executors were executed

            if (Executors != null)
            {
                try
                {
                    foreach (var executor in Executors)
                    {
                        executor.Execute(callContext, newState);
                    }
                }
                catch (BrakeFlowCallException)
                {
                    state.ClearForward();//this step was not done
                    throw;
                }
            }
        }

        private Step LocateNextChild(Update update)
        {
            try
            {
                return Children.First(child => child.Value.IsMatch(update)).Value;
            }
            catch (InvalidOperationException)
            {
                throw new CommandNotFoundException();
            }
        }

        protected abstract bool IsMatch(Update update);

        protected abstract string GetParameter(Update update);

        protected internal abstract string GetId();
    }
}