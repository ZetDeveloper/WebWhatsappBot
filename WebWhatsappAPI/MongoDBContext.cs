using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebWhatsappBotCore.Models;

namespace WebWhatsappBotCore
{
    public class MongoDBContext
    {
        public static string ConnectionString = "mongodb://localhost:27017";
        public static string DatabaseName { get; set; }
        public static bool IsSSL { get; set; }

        private IMongoDatabase _database { get; }

        public MongoDBContext()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase(DatabaseName);
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }
        }

        public IMongoCollection<DBWhatsapp> Chats
        {
            get
            {
                return _database.GetCollection<DBWhatsapp>("Chats");
            }
        }

        public IMongoCollection<Msg> Msg
        {
            get
            {
                return _database.GetCollection<Msg>("Msg");
            }
        }
    }
    }
