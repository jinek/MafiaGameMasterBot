using System;
using System.Linq;
using System.Threading;
using MGM.BotFlow.Extensions;
using MGM.BotFlow.Persistance;
using MGM.Game.Persistance.State;
using MGM.Localization;
using Telegram.Bot.Types;

namespace MGM.BotFlow.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
            Console.ReadLine();
        }

        private static void Test()
        {
            
            var api = new Telegram.Bot.Api("Paste some key for debugging purposes");
            
            var flowEngine = new FlowEngine(api,new DatabaseStateProvider(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=MafiaGM;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"),null);
            
            var mainMenu = flowEngine.AddCommand("start")
                .EchoReply(new OneLanguageString("Welcome"));
            
            flowEngine.AddAnyInput()
                .EchoReply(new OneLanguageString("Please, type /Start to start"));
            mainMenu.AddCommand("help")
                .EchoReply(new OneLanguageString("No help for now"));
            mainMenu.AddCommand("sum")
                .EchoReply(new OneLanguageString("Provide A"))
                .AddAnyInput()
                .EchoReply(new OneLanguageString("Provide B (A is {0}"))
                .AddAnyInput()
                .AddDelegateInput(context => (int.Parse(context[0]) + int.Parse(context[1])).ToString())
                .EchoReply(new OneLanguageString("Result is {0}"))
                .Execute(context => context.Echo(context[1]+context[2],context.UserMessageIdOrZero));//а тут сразу же показываем конкатенацию
            mainMenu.AddCommand("testPar", true)
                .EchoReply(new OneLanguageString("parameter is {0}"));
            mainMenu.AddCommandWithParameter("testAnyPar",new OneLanguageString("Please, enter value"))
                .ForEach(step => step.EchoReply(new OneLanguageString("Value is {0}")));

            int lastUpdate = 0;
            do
            {
                var updates = api.GetUpdates(lastUpdate + 1).Result;
                if (!updates.Any()) { Thread.Sleep(3000); continue; }
                foreach (var update in updates)
                {
                    IState state;
                    flowEngine.Process(update,null,out state);
                    ((DatabaseState) state).SaveAndDispose();
                    lastUpdate = update.Id;
                }
            } while (true);
        }
    }
}

/*if (update.CallbackQuery != null)
                   {
                       var message = update.CallbackQuery.Message;
                       await api.AnswerCallbackQuery(update.CallbackQuery.Id, "working...");
                       //await api.EditMessageText(message.Chat.Id, message.MessageId, "changed!");
                       await api.EditMessageReplyMarkup(message.Chat.Id, message.MessageId, new InlineKeyboardMarkup(new[]
                       {
                           new InlineKeyboardButton {CallbackData = "test1", Text = "test3"},
                           new InlineKeyboardButton {CallbackData = "test1", Text = "test3"}
                       }));
                   }
                   //Console.WriteLine(update.Message.Text);
                   else
                   {
                       var message = update.Message;


                       var message1 =
                           await
                               api.SendTextMessage(message.Chat.Id, "EchoReply: " + message.Text, false, false,
                                   message.MessageId,
                                   new InlineKeyboardMarkup(new[]
                                   {
                                       new InlineKeyboardButton {CallbackData = "test1", Text = "test1"},
                                       new InlineKeyboardButton {CallbackData = "test1", Text = "test1"}
                                   }));
                   }*/
