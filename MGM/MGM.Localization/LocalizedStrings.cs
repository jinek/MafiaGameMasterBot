using System;
using System.Linq;
using System.Reflection;
// ReSharper disable UnassignedReadonlyField
// ReSharper disable InconsistentNaming

namespace MGM.Localization
{

    //здесь текст путёвый https://www.facebook.com/Mafia-A-strategy-party-game-236567799706219/
    public static class LocalizedStrings
    {
        [ThreadStatic]
        public static uint Language;
        static LocalizedStrings()
        {
            var fields = typeof(LocalizedStrings).GetFields();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes<LVAttribute>().ToArray();
                if (attributes.Any())

                    field.SetValue(null, new LS(attributes.Select(attr => attr.Str).ToArray()), BindingFlags.Static,
                        null, null);
            }
        }

        [LV("{0:%s} seconds left")]
        [LV("Осталось {0:%s} секунд")]
        public static readonly LS AutoGameState_XSecondsLeft;

        [LV("Enter feedback text (feedback is anonymous, please, provide your contact in case you want us to get back to you)")]
        [LV("Введите текст отзыва (отзыв анонимный, если вы хотите дать возможность связаться с вами - укажите свой контакт)")]
        public static readonly LS CommonEngine_EnterFeedbackText;

        [LV("Your anonymous feedback is accepted. Thank you! Type /subscribe to be notified about game updates")]
        [LV("Ваш анонимный отзыв принят. Спасибо! Введите /subscribe ,что бы получать уведомления об обновлении игры")]
        public static readonly LS CommonEngine_FeedbackAccepted;

        [LV("3 to 8 players are needed to play Mafia. There are different actors in the game (mafia, doctor, policeman, civilian). All players are divided into 2 teams: civilians and mafia. Mafia need to kill all civilians (equal amount is enough) or to kill the policeman (if presented). Mafia kills secretly at night time. Civilians need to kill all mafia by voting and execution at day time. Please, vote and leave your comments here https://storebot.me/bot/MafiaGameMasterBot Source code is open now: https://github.com/jinek/MafiaGameMasterBot")]
        [LV("Для игры в мафию необходимо от 3-х до 8-ми игроков. В зависимости от количества в игре учасвтуют те или иные персонажи (мафия, доктор, комиссар, обычный мирный житель). Все игроки делятся на две команды: мирные жители и мафия. Задача мафии убить всех мирных жителей (достаточно сравнять кол-во) или убить комиссара (для некоторых режимов игры). Мафия убивает мирных жителей тайно, ночью. Задача мирных жителей убить мафию. Мирные жители убивают публично на открытом голосовании днём. Пожалуйста, голосуйте и оставляйте комментарии https://storebot.me/bot/MafiaGameMasterBot Исходный код доступен на github: https://github.com/jinek/MafiaGameMasterBot")]
        public static readonly LS CommonEngine_Help;

        [LV("Language has been switched to English")]
        [LV("Язык переключен на русский")]
        public static readonly LS CommonEngine_LanguageSwitched;

        [LV("Can not send secret message to {0}. Please, write something to me privately and try again")]
        [LV("Невозможно отправить секретное сообщение пользователю {0}. Попробуйте написать мне личное сообщение и попробовать ещё раз")]
        public static readonly LS PublicEngine_CannotSendToUser;

        [LV("*Day {0}*")]
        [LV("*День {0}*")]
        public static readonly LS DayState_DayCaption;

        [LV(@"City is waking up")]
        [LV(@"Город просыпается")]
        public static readonly LS DayState_EverybodyWakeUp;

        [LV("Everybody woke up, but *{0}*")]
        [LV("Просыпаются все, кроме *{0}*")]
        public static readonly LS DayState_EverybodyWakeUpBut;

        [LV("Lose on all civilians kill")]
        [LV("Проигрыш при убийстве всех жителей")]
        public static readonly LS Distributing_LooseOnCiviliansDie;

        [LV("Lose on policeman kill")]
        [LV("Проигрыш при убийстве полицейского")]
        public static readonly LS Distributing_LooseOnPoliceDie;

        [LV("Civilians won!")]
        [LV("Победили мирные жители")]
        public static readonly LS EndState_CivilianWin;

        [LV("*Game aborted*")]
        [LV("*Игра отменена*")]
        public static readonly LS EndState_GameAborted;

        [LV("*Game aborted by bot. May be bot was blocked*")]
        [LV("*Игра отменена системой. Возможно бот был заблокирован одним из пользователей или в чате*")]
        public static readonly LS EndState_GameAbortedBySystem;
        

        [LV("Mafia won!")]
        [LV("Победила мафия")]
        public static readonly LS EndState_MafiaWin;

        [LV(@"Next game can be started after {0:%s} sec.")]
        [LV(@"Следующую игру можно начать через {0:%s} сек.")]
        public static readonly LS EndState_NextGameCanBePlayedAfter;

        [LV("Against - {0}")]
        [LV("Против - {0}")]
        public static readonly LS FinalVotingState_Against;

        [LV("Civilians decided to let the player live. Additional voting is going. _In case of failed execution during next voting the city will go to sleep_")]
        [LV("Жители города решили помиловать игрока, будет открыто дополнительное голосование. _Если казнь не состоится на дополнительном голосовании, город ляжет спать_")]
        public static readonly LS FinalVotingState_CiviliansDecidedNotToKill;

        [LV("*{0}* was executed")]
        [LV("*{0}* был казнён")]
        public static readonly LS FinalVotingState_Kiiled;

        [LV("Skip - {0}")]
        [LV("Воздержалось - {0}")]
        public static readonly LS FinalVotingState_Skip;

        [LV("Skipped")]
        [LV("Воздержался")]
        public static readonly LS FinalVotingState_SkipFact;

        [LV("Final voting against *{0}*")]
        [LV("Финальное голосование против *{0}*")]
        public static readonly LS FinalVotingState_VotingAgainst;

        [LV("Support - {0}")]
        [LV("За - {0}")]
        public static readonly LS FinalVotingState_Yes;

        [LV("Supported")]
        [LV("За")]
        public static readonly LS FinalVotingState_YesFact;

        [LV("You are not allowed to vote")]
        [LV("Вы не можете голосовать")]
        public static readonly LS FinalVotingState_YouCanNotVote;

        [LV("Unrecognized command")]
        [LV("Незнакомая команда")]
        public static readonly LS FlowEngine_UnrecognizedCommand;

        [LV("Sorry, we've got an error. We are already notified, please try again later. Type /subscribe to be notified when game is updated")]
        [LV("У нас произошла ошибка, и мы уже опевещены. Приносим свои извинения. Введите /subscribe , что бы получить уведомление об обновлении игры")]
        public static readonly LS GameEngine_ErrorSorry;

        [LV("Player *{0}* is dead")]
        [LV("Игрок *{0}* мертв")]
        public static readonly LS GameHelper_UserIsDead;

        [LV("Player *{0}* does not participate in this game")]
        [LV("Пользователь *{0}* не учавствует в игре")]
        public static readonly LS GameHelper_UserIsNotInGame;

        [LV(@"Please, write a personal message to me, so I will be able to send you secret messages. To do that follow this link: https://telegram.me/MafiaGameMasterBot and choose 'Send Message'")]
        [LV(@"Пожалуйста, напишите мне личное сообщение, что бы я смог слать Вам секретные сообщения. Для этого пройдите по ссылке: https://telegram.me/MafiaGameMasterBot")]
        public static readonly LS GameProvider_MessageMeToPrivateChat;

        [LV("You can heal or kill only at night time. _Wait for the night in one of your current games, in which you participate as a doctor, policeman or mafia_")]
        [LV("Убивать/лечить и проверять можно только ночью. _Дождитесь ночи в одной из игр, в которой вы учавствуете как мафия, полицейский или доктор_")]
        public static readonly LS GameProvider_NoGamesYouCanVote;

        [LV("_Voting finished_")]
        [LV("_Голосование завершено_")]
        public static readonly LS GameStateBanner_VoteFinished;

        [LV(@"*civilian*")]
        [LV(@"*мирный житель*")]
        /*_Вам необходимо понять кто играет за мафию и убить её с помощью публичного голосования днём_
_Будьте осторожны, если вы казните комиссара, вы проиграете_*/
        public static readonly LS GameState_CivilianWord;

        [LV(@"*doctor*")]
        [LV(@"*доктор*")]
        /*
_Вы играете на стороне мирных жителей_
_Каждую ночь мафия будет пытаться убивать мирного жителя, попытайтесь раскрыть планы мафии и вылечить жителя города_*/
        public static readonly LS GameState_DoctorWord;

        [LV("*Game is finished*")]
        [LV("*Игра завершена*")]
        public static readonly LS GameState_GameFinished;

        [LV("Game has not been started yet. _Players are allowed to enter and exit the game_")]
        [LV("Игра ещё не началась. _Игроки могут присоединяться и отключаться_")]
        public static readonly LS GameState_GameNotStarted;

        [LV(@"*mafia*")]
        [LV(@"*мафия*")]
        /*
_Каждую ночь вы сможете убивать одного из мирных жителей (доктор будет стараться мешать вам)_
_Что бы победить вам нужно убить комиссара, либо всех мирных жителей, если его нет_*/
        public static readonly LS GameState_MafiaWord;
        [LV(@"*Night {0}*
_City falls asleep_")]
        [LV(@"*Ночь {0}*
_Город спит_")]
        public static readonly LS GameState_NightCaption;

        [LV("Players count need to be less than 8")]
        [LV("Игроков должно быть не более 8")]
        public static readonly LS GameState_PlayerShouldBeLess8;

        [LV("Players count should be greater than 3")]
        [LV("Игроков должно быть не менее трех")]
        public static readonly LS GameState_PlayersShouldBeGreater3;

        [LV(@"*policeman*")]
        [LV(@"*комисcар*")]
        public static readonly LS GameState_PolicemanWord;

        [LV("In city *{1}* You are - *{0}*")]
        [LV("В городе *{1}* Вы - *{0}*")]
        public static readonly LS GameState_YouAreXInCityY;

        [LV("Game is in progress _(may be this game was finished and you have to wait few seconds. Type /status to see more information)_")]
        [LV("Игра уже идёт _(или ещё не закончена предыдущая игра. Введите /status что бы увидеть подробную информацию)_")]
        public static readonly LS Game_AlreadyStarted;

        [LV("dead")]
        [LV("мертв")]
        public static readonly LS Game_DeadWord;

        [LV("It's not voting time now")]
        [LV("Сейчас не время голосования")]
        public static readonly LS Game_NotVotingTime;

        [LV(@"Sorry, but players already have been distributed
_Wait until the game is finished and type /ready again _")]
        [LV(@"Извините, но игроки уже распределены
_Дождитесь окончания игры и введите команду /ready ещё раз_")]
        public static readonly LS Game_PlayersDistributedAlready;

        [LV("Sorry, too many players")]
        [LV("Извините, в игре уже максимальное количество игроков")]
        public static readonly LS Game_TooMuchPlayers;

        [LV(@"You are already participating in this game
_Get enough players and type /go to start the game_")]
        [LV(@"Вы уже участвуете в этой игре
_Наберите достаточное количество игроков и введите /go что бы начать игру_")]
        public static readonly LS Game_YouAreInGameAlready;

        [LV("Because of unknown issue you can not participate in this game. Please, leave your /feedback")]
        [LV("Вы не можете учавствовать в этой игре из за технической проблемы. Пожалуйста, оставьте свой отзыв /feedback")]
        public static readonly LS Fault_TheSameId;

        [LV("You have exited the game")]
        [LV("Вы вышли из игры")]
        public static readonly LS Game_YouAreNotInGame;

        [LV(@"Another *mafia* votes this night
_Voting mafia is selected randomly but you can make a deal in private chat_")]
        [LV(@"В эту ночь голосует другая *мафия*
_Голосующая мафия выбирается случайно, но вы можете договариваться в приватном чате_")]
        public static readonly LS NightState_AnotherMafiaVoting;

        [LV("You can not kill *mafia*")]
        [LV("Нельзя убивать *мафию*")]
        public static readonly LS NightState_CanNotKillMafia;

        [LV(@"Choose a player for checking
_Don't hurry to tell everybody if you know who is mafia. If you uncover yourself, the second mafia will try to kill you next night_
_If you decide to do that, be persuasive because mafia can try to introduce himself as a 'real' police_")]
        [LV(@"Выберите игрока для проверки, что бы узнать кто он
_Днём не спешите говорить, если вы раскрыли в мафию. Вы раскроете себя и вас попытается убить вторая мафия_
_Если же вы решились - будьте убедительными, так как мафия может постараться выдать себя за комиссара_")]
        public static readonly LS NightState_ChooseToCheck;

        [LV("Players statistic")]
        [LV("Статистика игроков")]
        public static readonly LS Stat_PlayerStatHeader;

        [LV("wins")]
        [LV("побед")]
        public static readonly LS Stat_PlayerWin;

        [LV("games played")]
        [LV("игр сыграно")]
        public static readonly LS Stat_PlayerGamesCount;

        [LV(@"Choose your victim
_You need to kill the policeman to win (or all civilians)_
_Hurry, by default you will kill yourself_")]
        [LV(@"Выберите жертву
_Вам нужно убить комиссара что бы победить (либо всех жителей, если его нет)_
_Если вы не выберите игрока, то убьёте себя_")]
        public static readonly LS NightState_ChooseToKill;

        [LV(@"*City goes to sleep and mafia wake up*
_Everybody must be silent during the night_
_You can complain to anybody speaking by forwarding his message with command to me_ /complain")]
        [LV(@"*Город засыпает. Просыпается мафия*
_Жителям города нужно соблюдать тишину_
_Если вы хотите пожаловаться на игрока за разговор ночью - сделайте репост его сообщения с командой_ /complain")]
        public static readonly LS NightState_CityGoesSleep;

        [LV("You can not vote now, *civilians* sleep at night")]
        [LV("Вы не можете голосовать, ночью *мирные жители* спят")]
        public static readonly LS NightState_CiviliansSleep;

        [LV("You can check only one person per night")]
        [LV("За ночь вы можете проверить только одного игрока")]
        public static readonly LS NightState_OnlyOnePlayerCanBeChecked;

        [LV("We can't find that player")]
        [LV("Игрок не найден")]
        public static readonly LS NightState_PlayerNotFound;

        [LV(@"You can heal anyone including yourself
_This night mafia will try to kill someone, try to reveal their plans_")]
        [LV(@"Вы можете вылечить одного игрока, в  том числе себя
_Этой ночью мафия попытается убить кого-то, если вы догадываетесь кого - вылечите его_")]
        public static readonly LS NightState_YouCanHeelOnePlayer;

        [LV("You can't choose yourself")]
        [LV("Вы не можете выбрать себя")]
        public static readonly LS NightState_YouCanNotChooselYourself;

        
        [LV("Hello! Add me to chat to play Mafia")]
        [LV("Приветствую! Добавьте меня в чат, что бы начать игру")]
        public static readonly LS PirvateEngine_AddMeToChat;

        [LV("Too many players")]
        [LV("Слишком много игроков")]
        public static readonly LS PirvateEngine_CanNotAddMorePlayers;

        [LV("Your choice is accepted")]
        [LV("Ваш выбор принят")]
        public static readonly LS PirvateEngine_YourChoiceAccepted;

        [LV(@"Early voting started.
_Day ends earlier if everybody voted_")]
        [LV(@"Объявлено досрочное голосование.
_День закончится раньше, если проголосуют все жители_")]
        public static readonly LS PrevotingState_EarlyVotingStarted;

        [LV(@"*Preliminary voting started*
_Later there will be a final voting against selected player_")]
        [LV(@"*Началось предварительное голосование*
_По итогам будет начато ещё одно финальное голосование за убийство игрока_")]
        public static readonly LS PrevotingState_FirstVotingStarted;

        [LV("Civilians decided not to execute anybody")]
        [LV("Жители решили никого не казнить")]
        public static readonly LS PrevotingState_PlayerWasNotSelected;

        [LV("Skip")]
        [LV("Пропустить")]
        public static readonly LS PrevotingState_ToSkip;

        [LV("Player does not participate in the game")]
        [LV("Игрок не участвует в игре")]
        public static readonly LS PrevotingState_UserDoesNotPlay;

        [LV("Now I'm allowed to send messages to you.")]
        [LV("Теперь я смогу отправлять Вам сообщения.")]
        public static readonly LS PrivateEngine_ICanTextYou;

        [LV("_Type /modes to see possible game modes_")]
        [LV("_Для просмотра возможных режимов игры введите_ /modes")]
        public static readonly LS PrivateEngine_ToSeeMoreModes;

        [LV("Welcome to the game!")]
        [LV("Добро пожаловать в игру!")]
        public static readonly LS PrivateEngine_WelcomeToGame;

        [LV("User {0} is joining the game")]
        [LV("Пользователь {0} присоединяется к игре")]
        public static readonly LS UserJoinsGame;

        [LV("You can play in  mode {0}. _Type /go to start_")]
        [LV("Вы можете играть в режиме {0}. _Введите /go что бы начать игру_")]
        public static readonly LS PrivateEngine_YouCanPlayAs;

        [LV("*{0}* more players needed ({1})")]
        [LV("Необходимо ещё минимум *{0}* игрока ({1})")]
        public static readonly LS PrivateEngine_YouNeedMorePlayers;

        [LV("Please, setup your nickname (Telegram Options) and try again")]
        [LV("Установите себе никнейм (настройки Telegram) и попробуйте ещё раз")]
        public static readonly LS PublicEngine_ChangeName;

        [LV("*{0}* players: {1}")]
        [LV("*{0}* игроков: {1}")]
        public static readonly LS PublicEngine_CountPlayersAndShowMode;

        [LV(@"*Game started!*
_Check private messages from me to know who you are_
_Main rule of the game - don't show my private messages to anybody_")]
        [LV(@"*Игра началась!*
_Проверьте личные сообщения от меня, что бы узнать свою роль в городе_
_Главное правило игры - не показывать личные сообщения от меня другим игрокам_")]
        public static readonly LS PublicEngine_GameStartedAfterGo;

        [LV(@"Hello! To play Mafia all players are required to type /ready")]
        [LV(@"Приветствую! Для игры в мафию всем игрокам необходимо написать /ready")]
        public static readonly LS PublicEngine_StartCommandReply;

        [LV("You have exited the game")]
        [LV("Вы вышли из игры")]
        public static readonly LS PublicEngine_YouAreOut;

        [LV("Against")]
        [LV("Против")]
        public static readonly LS VinalVotingState_NoFact;

        [LV(@"Game updated. Type /version to see update history
_Type /unsubscribe to stop notification for this chat_")]
        [LV(@"Игра обновлена. Для просмотра истории обновлений введите /version
_Введите /unsubscribe что бы больше не получать сообщения_")]
        public static readonly LS GameUpdatedMessage;

        [LV(@"This chat is now subscribed to notifications")]
        [LV(@"Этот чат теперь подписан на обновления")]
        public static readonly LS CommonEngine_Subscribed;

        [LV(@"This chat is now unsubscribed")]
        [LV(@"Этот чат теперь отписан от обновлений")]
        public static readonly LS CommonEngine_Unsubscribed;

        [LV("It's not voting time now")]
        [LV("Сейчас нельзя голосовать")]
        public static readonly LS Voting_NotVotingTime;

        [LV("Bot is in service now, please, try again later")]
        [LV("Бот в настоящее время обслуживается, приносим свои извинения")]
        public static readonly LS Servicing_Now;
    }
}
