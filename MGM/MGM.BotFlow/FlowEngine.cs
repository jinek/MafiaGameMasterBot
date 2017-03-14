using System;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.BotFlow.Processing;
using MGM.BotFlow.Steps;
using MGM.Helpers;
using MGM.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MGM.BotFlow
{
    public sealed class FlowEngine : Step
    {
        private readonly Api _api;
        private readonly IStateProvider _stateProvider;

        public FlowEngine(Api api,IStateProvider stateProvider, ApiChatLimiter limiter)
        {
            _api = api;
            _stateProvider = stateProvider;
            _limiter = limiter;
        }
        
        private readonly LockerCollection<UserInChat> _userInChatLockers = new LockerCollection<UserInChat>();
        private readonly ApiChatLimiter _limiter;

        public void Process(Update update,object userState,out IState state)
        {
            var chat = update.GetChat().ToChat();
            var apiChat = new ApiChat(_api, chat, _limiter);
            var userInChat = new UserInChat(chat, update.GetUser());
            var locker = _userInChatLockers.GetOrCreateLocker(userInChat);

            lock (locker)
            {
             
            state = _stateProvider.GetStateForUserInChat(userInChat);
#if !DEBUG
            try
            {
#endif
            var callContext = new CallContext(apiChat, update,userState,userInChat);

                try
                {
                    if (Process(callContext, state,
                        update))
                    {
                        state.ClearForward();
                    }
                }
                catch (CommandNotFoundException)
                {
                    if (callContext.IsMessage)
                        callContext.Echo(LocalizedStrings.FlowEngine_UnrecognizedCommand, update.GetMessageId());
                    else throw; //if it was not message update - it's developer fail that was not supported (don't post buttons if you dont support them) //todo: low substitute exception
                }
                catch (BrakeFlowCallException exception)
                {
                    if (callContext.IsMessage || callContext.IsQuery)
                        callContext.ReplyEcho(exception.Message);
                }
#if !DEBUG

            }
            catch (Exception exception)
            {
                throw new ChatException(apiChat,exception.Message,exception);
            }
#endif
            }
        }

        protected override bool IsMatch(Update update)
        {
            throw new InvalidOperationException("Should not be called");
        }

        protected override string GetParameter(Update update)
        {
            throw new InvalidOperationException("Should not be called");
        }

        protected internal override string GetId()
        {
            return string.Empty;
        }
    }
}