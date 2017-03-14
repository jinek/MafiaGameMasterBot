using System;
using System.Collections.Generic;
using MGM.BotFlow.Executors;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using MGM.Localization;

namespace MGM.BotFlow.Extensions
{
    public static class FlowEngineExtensions
    {
        public static StepCollection AddCommandWithParameter(this Step step, string command,ILocalizedString suggestParameterText)
        {
            var commandWithParameter = new CommandStep(command, true);
            var commandWithoutOfParameter = new CommandStep(command, false);
            commandWithoutOfParameter.EchoReply(suggestParameterText, EchoOptions.EchoForceReply());
            var commandAwaitParameter = commandWithoutOfParameter.AddAnyInput();

            step.AddChildStep(commandWithParameter);
            step.AddChildStep(commandWithoutOfParameter);
            return new StepCollection(new[]{ commandWithParameter,commandAwaitParameter});
        }

        public static Step AddCommand(this Step step, string command, bool acceptParameter=false)
        {
            var commandStep = new CommandStep(command,acceptParameter);
            step.AddChildStep(commandStep);
            return commandStep;
        }

        public static Step AddQueryResult(this Step step)
        {
            var commandStep = new QueryResultStep();
            step.AddChildStep(commandStep);
            return commandStep;
        }

        public static Step AddAnyInput(this Step step)
        {
            var anyInputStep = new AnyInputStep();
            step.AddChildStep(anyInputStep);
            return anyInputStep;
        }

        public static Step AddPhotoInput(this Step step)
        {
            var photoStep = new PhotoStep();
            step.AddChildStep(photoStep);
            return photoStep;
        }

        /// <summary>
        ///     это добавит автоматический input
        /// </summary>
        public static Step AddDelegateInput(this Step step, Func<CallContext, string> act)
        {
            var executorStep = new AutomaticStep(act);
            step.AddChildStep(executorStep);
            return executorStep;
        }

        public static void AddChildStep(this Step step, Step childStep)
        {
            if (step.Children == null) step.Children = new Dictionary<string, Step>();
            step.Children.Add(childStep.GetId(), childStep);
        }

        /// <summary>
        ///     это не добавит input и просто выполнится при выполнении Input
        /// </summary>
        public static Step Execute(this Step step, Action<CallContext> act)
        {
            step.AppendExecutor(new ExecuteExecutor(act));
            return step;
        }

        public static Step EchoReply(this Step step, ILocalizedString localizedString, EchoOptions echoOptions=null)
        {
            step.AppendExecutor(new EchoExecutor(localizedString,echoOptions));
            return step;
        }

        public static void AppendExecutor(this Step step, IExecutor executor)
        {
            if (step.Executors == null)
            {
                step.Executors = new List<IExecutor>(1);
            }

            step.Executors.Add(executor);
        }
    }
}