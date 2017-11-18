namespace WebWhatsappBotCore.Models
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class DBWhatsapp
    {
        [JsonProperty("archive")]
        public bool Archive { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonProperty("modifyTag")]
        public long? ModifyTag { get; set; }

        [JsonProperty("msgs")]
        public Msg[] Msgs { get; set; }

        [JsonProperty("muteExpiration")]
        public long MuteExpiration { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notSpam")]
        public bool NotSpam { get; set; }

        [JsonProperty("pendingMsgs")]
        public bool PendingMsgs { get; set; }

        [JsonProperty("pin")]
        public long Pin { get; set; }

        [JsonProperty("t")]
        public long T { get; set; }

        [JsonProperty("unreadCount")]
        public long UnreadCount { get; set; }
    }

    public partial class Msg
    {
        [JsonProperty("ack")]
        public double? Ack { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("broadcast")]
        public bool? Broadcast { get; set; }

        [JsonProperty("canonicalUrl")]
        public string CanonicalUrl { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("clientUrl")]
        public string ClientUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("filehash")]
        public string Filehash { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("height")]
        public long? Height { get; set; }

        [JsonProperty("id")]
        public Id Id { get; set; }

        [JsonProperty("invis")]
        public bool? Invis { get; set; }

        [JsonProperty("isNewMsg")]
        public bool? IsNewMsg { get; set; }

        [JsonProperty("matchedText")]
        public string MatchedText { get; set; }

        [JsonProperty("mediaKey")]
        public string MediaKey { get; set; }

        [JsonProperty("mentionedJidList")]
        public string[] MentionedJidList { get; set; }

        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("notifyName")]
        public string NotifyName { get; set; }

        [JsonProperty("pageCount")]
        public long? PageCount { get; set; }

        [JsonProperty("quotedMsg")]
        public QuotedMsg QuotedMsg { get; set; }

        [JsonProperty("quotedParticipant")]
        public string QuotedParticipant { get; set; }

        [JsonProperty("quotedStanzaID")]
        public string QuotedStanzaID { get; set; }

        [JsonProperty("recipients")]
        public string[] Recipients { get; set; }

        [JsonProperty("recvFresh")]
        public bool? RecvFresh { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("star")]
        public bool Star { get; set; }

        [JsonProperty("streamingSidecar")]
        public StreamingSidecar StreamingSidecar { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        [JsonProperty("t")]
        public long T { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }
    }

    public partial class StreamingSidecar
    {
    }

    public partial class QuotedMsg
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Id
    {
        [JsonProperty("fromMe")]
        public bool FromMe { get; set; }

        [JsonProperty("id")]
        public string PurpleId { get; set; }

        [JsonProperty("remote")]
        public string Remote { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("_serialized")]
        public string Serialized { get; set; }
    }

    public partial class DBWhatsapp
    {
        public static DBWhatsapp[] FromJson(string json) => JsonConvert.DeserializeObject<DBWhatsapp[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DBWhatsapp[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
