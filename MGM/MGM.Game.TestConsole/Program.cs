using System;
using System.Linq;
using System.Threading;
using MGM.Game.Engine;
using Telegram.Bot;

namespace MGM.Game.TestConsole
{
    internal class Program
    {
        private static void Main()
        {
            Test();
            Console.ReadLine();
        }

        private static void Test()
        {
            Api api = new Api("Should be some API key");
            //GameEngine gameEngine = new GameEngine(@"Data Source=l78wumy660.database.windows.net;Initial Catalog=MGM_db;Integrated Security=False;User ID=jinek;Password=Ofooxath2;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False", "10", "10||no");
            GameEngine gameEngine = new GameEngine(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MafiaGM;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False", "9", "10||no");

            //gameEngine.Timer();
var lastUpdate = 0;
            do
            {
                var updates = api.GetUpdates(lastUpdate + 1).Result;
                if (!updates.Any())
                {
                    Thread.Sleep(500);
                    continue;
                }
                foreach (var update in updates)
                {
                    gameEngine.ProcessUpdate(update);

                    lastUpdate = update.Id;
                }
            } while (true);
        }

    }
}