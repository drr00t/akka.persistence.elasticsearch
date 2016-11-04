using Nest;
using Newtonsoft.Json;

namespace Akka.Persistence.ElasticSearch.Journal
{
    [ElasticsearchType(Name = nameof(JournalEntry), IdProperty = nameof(Id))]
    public class JournalEntry
    {
        [String(Name = "id")]
        public string Id { get; set; }

        [String(Name = "persistenceId")]
        public string PersistenceId { get; set; }

        [Number(Name = "sequenceNr")]
        public long SequenceNr { get; set; }

        [Object(Name = "payload")]
        public object Payload { get; set; }

        [String(Name = "manifest")]
        public string Manifest { get; set; }

        [Boolean(Name = "is_deleted", NullValue = false, Store = true)]
        public bool IsDeleted { get; set; }
    }
}
