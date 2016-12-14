using Nest;
using Newtonsoft.Json;
using System;

namespace Akka.Persistence.Elasticsearch.Journal
{
    [ElasticsearchType(Name = nameof(JournalEntry), IdProperty = nameof(Id))]
    public class JournalEntry
    {
        [String]
        public string Id { get; set; }

        [String]
        public string PersistenceId { get; set; }

        [Number]
        public Int64 SequenceNr { get; set; }

        [Object]
        public object Payload { get; set; }

        [String(Name = "manifest")]
        public string Manifest { get; set; }

        [Boolean(Name = "is_deleted", NullValue = false, Store = true)]
        public bool IsDeleted { get; set; }
    }
}
