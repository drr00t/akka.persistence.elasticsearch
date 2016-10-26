using Newtonsoft.Json;

namespace Akka.Persistence.ElasticSearch.Journal
{
    public class MetadataEntry
    {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty("PersistenceId")]
        public string PersistenceId { get; set; }

        [JsonProperty("SequenceNr")]
        public long SequenceNr { get; set; }
    }
}
