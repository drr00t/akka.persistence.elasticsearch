using Akka.Persistence.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.Collections.Immutable;
using Akka.Persistence.Elasticsearch;
using Nest;
using Elasticsearch.Net;

namespace Akka.Persistence.Elasticsearch.Journal
{
    public class ElasticsearchJournal : AsyncWriteJournal
    {
        private readonly ElasticsearchJournalSettings _settings;
        private IElasticClient _elasticClient;
        //private Lazy<IMongoDatabase> _mongoDatabase; _elasticClient
        //private Lazy<IMongoCollection<JournalEntry>> _journalCollection;
        //private Lazy<IMongoCollection<MetadataEntry>> _metadataCollection;

        public ElasticsearchJournal()
        {
            _settings = ElasticsearchPersistence.Get(Context.System).JournalSettings;
        }

        protected override void PreStart()
        {
            base.PreStart();

            string endpoint = "http://localhost:9200/";
            string index = "eventstore";
            int shards = 1;
            int replicaShards = 0;
            string user = "user";
            string pass = "pass";
            string apiKey = "apiKey";


            // client initialization
            var node = new Uri(endpoint);

            var connectionPool = new SniffingConnectionPool(new[] { node });

            var config = new ConnectionSettings(connectionPool)
                    .DisableDirectStreaming()
                    .EnableHttpCompression()
            ;

            _elasticClient = new ElasticClient(config);

            if (_settings.AutoInitialize)
            {
                if (!_elasticClient.IndexExists(index).Exists)
                {
                    _elasticClient.CreateIndex(index, c => c.Settings(s => s
                    .NumberOfReplicas(0)
                    .NumberOfShards(1))
                    .Mappings(m => m.Map<JournalEntry>(map => map.AutoMap()))
                    .Mappings(m => m.Map<MetadataEntry>(map => map.AutoMap())));
                }                
            }




            //database or index setup

            // if autoinitialize

            // create de journal indexes
            //journal collection load ordered by lastest Sequencial number from persistence id

            //create metadata indexes 

        }

        public override Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            var builder = _elasticClient.SearchAsync<MetadataEntry>(se => se.Sort(ss => ss
                        .Ascending(p => p.PersistenceId)
                        .Descending(p => p.SequenceNr))
                        .Query(q => q.Bool(bo => 
                            bo.Must(ma => ma.Field(fi => fi.PersistenceId == persistenceId))))
                            .Limit(ql => ql.Limit(1),)

                ).ContinueWith<long>(p => p.Result.Documents
                    .Where(entry => entry.PersistenceId.Equals(persistenceId))
                    .Select(x => x.SequenceNr > fromSequenceNr)

            Builders<MetadataEntry>.Filter;
            var filter = builder.Eq(x => x.PersistenceId, persistenceId);

            var highestSequenceNr = await _metadataCollection.Value.Find(filter).Project(x => x.SequenceNr).FirstOrDefaultAsync();

            return highestSequenceNr;
        }

        public override Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            var messageList = messages.ToList();
            var writeTasks = messageList.Select(async message =>
            {
                var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();

                var journalEntries = persistentMessages.Select(ToJournalEntry).ToList();
                await _journalCollection.Value.InsertManyAsync(journalEntries);
            });

            await SetHighSequenceId(messageList);

            return await Task<IImmutableList<Exception>>
                .Factory
                .ContinueWhenAll(writeTasks.ToArray(),
                    tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null).ToImmutableList());
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            var builder = Builders<JournalEntry>.Filter;
            var filter = builder.Eq(x => x.PersistenceId, persistenceId);

            if (toSequenceNr != long.MaxValue)
                filter &= builder.Lte(x => x.SequenceNr, toSequenceNr);

            return _journalCollection.Value.DeleteManyAsync(filter);
        }

        protected override Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var response = _elasticClient.<TElasticModel, TElasticUpdateModel>(
                            DocumentPath<TElasticModel>.Id(GetId(data)),
                                d => d.Doc(ConvertToUpdateModel(data)).DocAsUpsert());

            _elasticClient.IndexAsync(tweet, idx => idx.Index("mytweetindex"));
        }
    }
}
