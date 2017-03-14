using System.Collections.Generic;
using MGM.BotFlow.Processing;
using MGM.Game.Persistance.Database.DataModels.State;
using MGM.Game.Persistance.Database.DataModels.Story;
using MGM.Game.Persistance.Database.DataModels.Telegram;
using Telerik.OpenAccess;
using Telerik.OpenAccess.Metadata;
using Telerik.OpenAccess.Metadata.Fluent;
using Telerik.OpenAccess.Metadata.Fluent.Advanced;

namespace MGM.Game.Persistance.Database
{
    internal class DbMetadataSource : FluentMetadataSource
    {
        protected override IList<MappingConfiguration> PrepareMapping()
        {
            var configurations = new List<MappingConfiguration>();

            {
                var configuration = MapConfig<ChatInTelegram>();
                configuration.HasProperty(arg => arg.Id).IsIdentity();
                configuration.HasIndex(chat => chat.Subscribed).WithName("IX_Described");
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<UserInTelegram>();
                configuration.HasProperty(arg => arg.Id).IsIdentity();
                configuration.HasProperty(user => user.PrivateChatId).IsNullable();
                configuration.HasIndex(user => user.PrivateChatId)/*.IsUnique() не поддерживается в SQL*/.WithName("IX_PrivateChat");

                configuration.HasAssociation(user => user.PrivateChat)
                    .WithOpposite(chat => chat.UserOfPrivateChat)
                    .HasConstraint((user, chat) => user.PrivateChatId==chat.Id)
                    /*.IsDependent()*/;

                configuration.HasProperty(user => user.AllGameCount).IsNotNullable();
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<UserInChat>();
                configuration.HasProperty(arg => arg.Id).IsIdentity(KeyGenerator.Autoinc);
                configuration.HasIndex(state => new {state.UserId, state.ChatId}).IsUnique().WithName("IX_UserChatB");
                configuration.HasAssociation(chat => chat.UserInTelegram)
                    .WithOpposite(telegram => telegram.UserInChats)
                    .HasConstraint((chat, telegram) => chat.UserId == telegram.Id)
                    /*.IsDependent()*/;
                configuration.HasAssociation(chat => chat.ChatInTelegram)
                    .WithOpposite(telegram => telegram.UserInChats)
                    .HasConstraint((userInChat, chatInTelegram) => userInChat.ChatId == chatInTelegram.Id)
                    /*.IsDependent()*/;
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<FlowState>();
                configuration.HasProperty(arg => arg.Id).IsIdentity(KeyGenerator.Autoinc);
                configuration.HasAssociation(state => state.UserInChat)
                    .WithOpposite(chat => chat.FlowStates)
                    .HasConstraint((state, chat) => state.UserInChatId == chat.Id)
                    /*.IsDependent()*/;
                configuration.HasProperty(state => state.FlowStateId).AsNVarChar(150).IsNotNullable();
                configuration.HasProperty(state => state.Value).AsInfiniteStringNText();
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<DataModels.Game.Game>();
                configuration.HasProperty(arg => arg.Id).IsIdentity(KeyGenerator.Autoinc);
                configuration.HasProperty(arg => arg.SerializedGame).AsInfiniteStringNText().WithLoadBehavior(LoadBehavior.Lazy);
                configuration.HasProperty(arg => arg.CreationTime).IsNotNullable();
                configuration.HasProperty(arg => arg.LastAccessTime).IsNotNullable();

                configuration.HasIndex(game => game.MaxWakeupTime).WithName("IX_MaxWakeupTime");
                configuration.HasIndex(game => game.FinishTime).WithName("IX_FinishTime");
                configuration.HasIndex(game => game.IsNight).WithName("IX_IsNight");

                configuration.HasAssociation(game => game.ChatInTelegram)
                    .WithOpposite(telegram => telegram.Games)
                    .HasConstraint((game, chat) => game.ChatId == chat.Id)
                    /*.IsDependent()*/;

                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<DataModels.Game.Player>();
                configuration.HasProperty(arg => arg.Id).IsIdentity(KeyGenerator.Autoinc);
                configuration.HasAssociation(player => player.Game)
                    .WithOpposite(game => game.Players)
                    .HasConstraint((player, game) => player.GameId == game.Id)
                    /*.IsDependent()*/;
                configuration.HasAssociation(player => player.UserInTelegram)
                    .WithOpposite(telegram => telegram.PlayerInGames)
                    .HasConstraint((player, telegram) => player.UserId == telegram.Id)
                    /*.IsDependent()*/;
                configuration.HasIndex(player => player.PutToVoting).WithName("IX_PutToVoting");
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<Story>();
                configuration.HasProperty(arg => arg.Id).IsIdentity(KeyGenerator.Autoinc);
                configuration.HasProperty(arg => arg.Body).AsInfiniteStringNText().IsNotNullable();
                configuration.HasProperty(arg => arg.PhotoId).IsNotNullable();
                configurations.Add(configuration);
            }

            /*{
                var configuration = MapConfig<TestDbData>();

                configuration.HasProperty(data => data.Id)
                    .IsIdentity(KeyGenerator.Autoinc);

                configuration.HasProperty(data => data.Name)
                    .IsNotNullable();

                configuration.HasAssociation(data => data.Category)
                    .WithOpposite(category => category.DbDatas)
                    .HasConstraint((data, category) => data.CategoryId == category.Id);

                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<SessionRegistration>();

                configuration.HasProperty(data => data.Id)
                    .IsIdentity(KeyGenerator.Autoinc);

                configuration.HasProperty(data => data.PhoneNumber)
                    .AsNVarChar()//NVarChar нужен что бы можно было делать сравнение
                    .IsNotNullable();

                configuration.HasProperty(registration => registration.Pushdata)
                    .IsNullable()
                    .AsNVarChar(255);

                configuration.HasProperty(data => data.DeviceId)
                    .IsNotNullable();

                configuration.HasProperty(data => data.SessionId)
                    .AsNVarChar(36)//https://ru.wikipedia.org/wiki/GUID 
                    .IsNotNullable();

                configuration.HasIndex(data => data.SessionId)
                    .IsUnique()
                    .WithName("IX_SessionId");

                configuration.HasProperty(data => data.MasterId)
                    .IsNotNullable();
                
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<Station>();
                configuration.HasProperty(data => data.Name).AsNVarChar().IsNotNullable().IsIdentity();
                configuration.HasProperty(data => data.LineNumber).IsNotNullable().IsIdentity();
                configurations.Add(configuration);
            }

            {
                var configuration = MapConfig<Line>();
                configuration.HasProperty(data => data.Color).AsNVarChar().IsNotNullable();
                configuration.HasProperty(data => data.Name).AsNVarChar().IsNotNullable();
                configuration.HasProperty(data => data.Number).IsNotNullable().IsIdentity();
                configurations.Add(configuration);
            }*/

            return configurations;
        }
        
        private MappingConfiguration<T> MapConfig<T>()
        {
            
            var configuration = new MappingConfiguration<T>();

            configuration.MapType()
                .WithConcurencyControl(OptimisticConcurrencyControlStrategy.Changed);
            
            return configuration;
        }

        protected override MetadataContainer CreateModel()
        {
            var metadataContainer = base.CreateModel();
            metadataContainer.DefaultMapping.NullForeignKey = true;


            var nameGenerator = metadataContainer.NameGenerator;

            nameGenerator.SourceStrategy = NamingSourceStrategy.Property;
            nameGenerator.ResolveReservedWords = false;
            nameGenerator.RemoveCamelCase = false;



            return metadataContainer;
        }
    }
}