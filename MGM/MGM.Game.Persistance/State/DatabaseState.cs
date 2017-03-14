using System.Collections.Generic;
using System.Linq;
using MGM.BotFlow.Persistance;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.DataModels.State;
using MGM.Game.Persistance.Database.DataModels.Telegram;
using MGM.Game.Persistance.Database.Helpers;
using MGM.Localization;
using Telerik.OpenAccess;
using UserInChat = MGM.Game.Persistance.Database.DataModels.State.UserInChat;

namespace MGM.Game.Persistance.State
{
    public class DatabaseState : IState
    {
        private readonly DbContext _db;
        private readonly int _index;
        private readonly UserInChat _userInChatPersistance;
        private readonly IList<FlowState> _userStates;

        private DatabaseState _nextState;

        /// <summary>
        ///     Для создания следующего сохранённого состояния
        /// </summary>
        private DatabaseState(int index, IList<FlowState> states, DbContext db, UserInChat userInChatPersistance)
        {
            _index = index;
            _userStates = states;
            _userInChatPersistance = userInChatPersistance;
            _db = db;
            CurrentFlowState = states[index];
                //это можно конечно сделать выше, но сюда все равно эти параметры передаются
            //если в базе есть следующий, то читаем его в Next
            var nextIndex = index + 1;
            if (nextIndex < states.Count)
            {
                _nextState = new DatabaseState(nextIndex, states, db, userInChatPersistance);
            }
        }

        private FlowState CurrentFlowState { get; }

        public string Id => CurrentFlowState.FlowStateId;

        public string Value => CurrentFlowState.Value;
        public IState NextState => _nextState;

        public IState AddStepState(string stepId, string updateValue)
        {
            var flowState = new FlowState
            {
                FlowStateId = stepId,
                Value = updateValue,
                UserInChat = _userInChatPersistance
            };

            _db.Add(flowState);
            _userStates.Add(flowState);
            _nextState = new DatabaseState(_index + 1, _userStates, _db, _userInChatPersistance);
            return NextState; //todo: low получается, что AddStepState должен возвращать void
        }

        public void ClearForward()
        {
            ((DatabaseState)NextState)?.ClearForwardInternal();
            _nextState = null;
        }

        private void ClearForwardInternal()
        {
            ((DatabaseState)NextState)?.ClearForwardInternal();
            _db.Delete(CurrentFlowState);
            _userStates.Remove(CurrentFlowState);
        }

        private static object _chatCreationLocker = new object();//todo: low I don't like it's static, but i don't understand now where should lock be kept
        public static DatabaseState GetRootState(BotFlow.Persistance.UserInChat userInChat, string connectionString)
        {
            var db = new DbContext(connectionString);
            var userInChatPersistance = db.UsersInChat.GetUserInChatPersistance(userInChat);
            

            List<FlowState> flowStates;
            if (userInChatPersistance == null)
            {
                var userId = userInChat.UserId;
                uint lastLanguage = 0;

                if (db.UsersInChat.Any())
                {
                    var maxId = db.UsersInChat.Max(uic => uic.Id);
                    lastLanguage = db.UsersInChat.Single(uc => uc.Id == maxId).ChatInTelegram.LanguageIndex;
                }

                lock (_chatCreationLocker)//todo: low Not the fastest lock
                {
                    var chatInTelegram = db.ChatInTelegrams.ById(userInChat.ChatId) ?? new ChatInTelegram
                    {
                        Id = userInChat.ChatId,
                        LanguageIndex = /*todo: low I don't understand where it should be in this case*/
                            LocalizedStrings.Language = lastLanguage
                    };

                    var userInTelegram = db.UserInTelegrams.ById(userId) ?? new UserInTelegram
                    {
                        Id = userId,
                    };

                    userInChatPersistance = new UserInChat
                    {
                        ChatInTelegram = chatInTelegram,
                        UserInTelegram = userInTelegram,
                    };
                    db.Add(userInChatPersistance);
                    var firstFlowState = new FlowState
                    {
                        UserInChat = userInChatPersistance,
                        FlowStateId = string.Empty
                    };

                    db.Add(firstFlowState);
                    userInChatPersistance.FlowStates.Add(firstFlowState);

                    db.SaveChanges();
                    db.Refresh(RefreshMode.OverwriteChangesFromStore, userInChat);

                    flowStates = new List<FlowState> {firstFlowState};
                }
            }
            else
            {
                flowStates = new List<FlowState>(userInChatPersistance.FlowStates.ToArray());
                    //предполагаем, что у созданного сходу есть один стейт
            }


            return new DatabaseState(0, flowStates, db, userInChatPersistance);
        }

        public void SaveAndDispose()
        {
            _db.SaveChanges();
            _db.Dispose();
        }
    }
}