using MGM.BotFlow.Steps;
using MGM.Game.Models;
using MGM.Game.Persistance;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.DataModels.Telegram;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegram.Bot.Types;
/*using Telerik.JustMock;
using Telerik.JustMock.Helpers;*/
using User = MGM.Game.Models.User;

namespace MGM.Game.Tests
{
    /// <summary>
    /// Summary description for SomeTests
    /// </summary>
    [TestClass]
    public class SomeTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestPlayerAndUserEquality()
        {
            var user1 = new User(2,"testUser");
            var user2 = new User(2,"testUser");
            var user3 = new User(3,"testUser 3");
            var player1 = new Player(user1);
            var player2 = new Player(user2);
            var player3 = new Player(user3);

            Assert.AreEqual(player1,player2);
            Assert.AreNotEqual(player1,player3);
        }

        [TestMethod]
        public void TestAliveAndDeadPlayers()
        {
            var game = new Game("Тестовый город",null,11,null,null,0,null);
            for (int i = 0; i < 7; i++)
            {
                game.Ready(new User(i, i.ToString()));
            }

            var deadPlayer = new Player(new User(4,"4"));
            game.Dead.Add(deadPlayer);

            var alivePlayers = game.GetAlivePlayers();
            Assert.AreEqual(6, alivePlayers.Length);
            Assert.IsFalse(game.IsAlive(deadPlayer));
        }

        class TestAnyInputStep : AnyInputStep
        {
            public new string GetParameter(Update update)
            {
                return base.GetParameter(update);
            }
        }
        /*[TestMethod]
        public void TestAnyInputParameter()
        {
            var update = Mock.Create<Update>();
            update.Arrange(update1 => update1.Message.Text).Returns("qwerty");

            var anyInputStep = new TestAnyInputStep();
            var result = anyInputStep.GetParameter(update);
            Assert.AreEqual(result,"qwerty");
        }*/

        [TestMethod]
        public void TestDatabaseRelations()
        {
            var connectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=MafiaGMTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            DatabaseHelper.Actualize(connectionString);

            using (
                var db =
                    new DbContext(
                        connectionString)
                )
            {
                var userInTelegram = new UserInTelegram() { Id = 2,PrivateChat = new ChatInTelegram() {Id = 3,LanguageIndex = 2} };
                db.Add(userInTelegram);
                db.SaveChanges();
            }
        }
    }
}
