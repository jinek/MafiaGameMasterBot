using System;
using System.Linq;
using MGM.Game.Persistance.Database;
using MGM.Game.Persistance.Database.DataModels.Story;
using Telerik.OpenAccess;

namespace MGM.Game.Persistance
{
    public static class DatabaseHelper
    {
        public static void Actualize(string connectionString)
        {
            using (var db = new DbContext(connectionString))
            {
                var schema = db.GetSchemaHandler();//нужно подключить базу

                if (!schema.DatabaseExists())
                {
                    bool databaseCreated;
                    try
                    {
                        databaseCreated = schema.CreateDatabase();
                    }
                    catch (OpenAccessException exception)
                    {
                        throw new InvalidOperationException(
                            $"Не удалось создать базу данных. Возможно сервер не установлен или недоступен: {exception.Message}");
                    }
                    if (!databaseCreated)
                    {
                        throw new InvalidOperationException(
                            "Не удалось создать базу данных. Возможно сервер не установлен или недоступен");
                    }
                }


                var schemaUpdateInfo = schema.CreateUpdateInfo(new SchemaUpdateProperties());

                if (schemaUpdateInfo.HasScript)
                {
                    schema.ForceUpdateSchema(schemaUpdateInfo);
                }
            }
        }

        public static Story[] GetAllStories(string connectionString)
        {
            using (var db = new DbContext(connectionString))
            {
                return db.Stories.ToArray();
            }
        }

        public static void AddStory(string photoId, string body,string connectionString)
        {
            using (var db = new DbContext(connectionString))
            {
                db.Add(new Story {Body = body,PhotoId = photoId});
                db.SaveChanges();
            }
        }

        public static void DeleteStory(int index, string connectionString)
        {
            using (var db = new DbContext(connectionString))
            {
                db.Delete(db.Stories.ToArray()[index]);//not many stories
                db.SaveChanges();
            }
        }
    }
}