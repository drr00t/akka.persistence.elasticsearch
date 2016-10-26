using Newtonsoft.Json;

namespace Akka.Persistence.ElasticSearch.Journal
{
    public class JournalEntry
    {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty("PersistenceId")]
        public string PersistenceId { get; set; }

        [JsonProperty("SequenceNr")]
        public long SequenceNr { get; set; }

        [JsonProperty("IsDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("Payload")]
        public object Payload { get; set; }

        [JsonProperty("Manifest")]
        public string Manifest { get; set; }
    }
}
