using Nest;
using Newtonsoft.Json;

namespace Akka.Persistence.Elasticsearch.Journal
{
    [ElasticsearchType(Name = nameof(MetadataEntry), IdProperty = nameof(Id))]
    public class MetadataEntry
    {
        [String]
        public string Id { get; set; }

        [String]
        public string PersistenceId { get; set; }

        [Number]
        public long SequenceNr { get; set; }
    }
}
