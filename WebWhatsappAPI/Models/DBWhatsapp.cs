namespace WebWhatsappBotCore.Models
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public partial class DBWhatsapp
    {
        [BsonId]
        public ObjectId MySuperId { get; set; }
        public string Id { get; set; }

        public List<Msg> Msgs { get; set; }

        public int LastComand { get; set; }

        public DateTime DateTimeLastComand { get; set; }

       
    }

    public partial class Msg
    {
        [BsonId]
        public ObjectId MySuperId { get; set; }
        public Id id { get; set; }
        public string body { get; set; }
        public string type { get; set; }
        public int t { get; set; }
        public string notifyName { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string self { get; set; }
        public int ack { get; set; }
        public bool invis { get; set; }
        public bool isNewMsg { get; set; }
        public bool star { get; set; }
        public bool recvFresh { get; set; }
        public bool broadcast { get; set; }
        public List<object> labels { get; set; }
    }

    public class Id
    {
        public bool fromMe { get; set; }
        public string remote { get; set; }
        public string id { get; set; }
        public string _serialized { get; set; }
    }


}
