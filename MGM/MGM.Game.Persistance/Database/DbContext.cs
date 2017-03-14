using System.Linq;
using MGM.BotFlow.Processing;
using MGM.Game.Persistance.Database.DataModels;
using MGM.Game.Persistance.Database.DataModels.Game;
using MGM.Game.Persistance.Database.DataModels.State;
using MGM.Game.Persistance.Database.DataModels.Story;
using MGM.Game.Persistance.Database.DataModels.Telegram;
using Telerik.OpenAccess;
using Telerik.OpenAccess.Metadata;

namespace MGM.Game.Persistance.Database
{
    public class DbContext : OpenAccessContext
    {
        private static readonly MetadataSource MetadataSource = new DbMetadataSource();
        public DbContext(string connectionString) : base(connectionString,connectionString.ToUpper().Contains("database.windows.net".ToUpper()) ?GetAzureBackendConfiguration():GetSqlBackendConfiguration(), MetadataSource.GetModel())
        {
        }

        public IQueryable<UserInChat> UsersInChat => GetAll<UserInChat>();
        public IQueryable<FlowState> FlowStates => GetAll<FlowState>();
        public IQueryable<DataModels.Game.Game> Games => GetAll<DataModels.Game.Game>();
        public IQueryable<Player> Players => GetAll<Player>();
        public IQueryable<UserInTelegram> UserInTelegrams => GetAll<UserInTelegram>();
        public IQueryable<Story> Stories => GetAll<Story>();
        public IQueryable<ChatInTelegram> ChatInTelegrams => GetAll<ChatInTelegram>();

        public static BackendConfiguration GetAzureBackendConfiguration()
        {
            var backend = new BackendConfiguration
            {
                Backend ="Azure",
                ProviderName = "System.Data.SqlClient"
            };
            backend.Logging.MetricStoreSnapshotInterval = 0;
            return backend;
        }

        public static BackendConfiguration GetSqlBackendConfiguration()
        {
            var backend = new BackendConfiguration
            {
                Backend = "MsSql",
                ProviderName = "System.Data.SqlClient"
            };
            backend.Logging.MetricStoreSnapshotInterval = 0;
            return backend;
        }
    }
}