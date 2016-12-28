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

        string _indexStore = "eventstore";
        string _typeJournal = "journalentry";
        string _typeMetadata = "metadataentry";

        public ElasticsearchJournal()
        {
            _settings = ElasticsearchPersistence.Get(Context.System).JournalSettings;
        }

        protected override void PreStart()
        {
            base.PreStart();

            string endpoint = "http://localhost:9200/";
            string indexStore = "eventstore-dados";
            string typeJournal = "journalentry";
            string typeMetadata = "metadataentry";
            int shards = 1;
            int replicaShards = 0;
            string user = "user";
            string pass = "pass";
            string apiKey = "apiKey";


            // client initialization
            var node = new Uri(endpoint);

            var connectionPool = new SniffingConnectionPool(new[] { node });

            var config = new ConnectionSettings(node)
                    .DisableDirectStreaming()
                    .EnableHttpCompression()
            ;

            _elasticClient = new ElasticClient(config);

            if (_settings.AutoInitialize)
            {
                if (!_elasticClient.IndexExists(_indexStore).Exists)
                {
                    _elasticClient.CreateIndex(_indexStore, c => c.Settings(s => s
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

        public override async Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            var builder = _elasticClient.SearchAsync<MetadataEntry>(
                s => s.Size(1).Index(Indices.Index(new IndexName { Name = } )
                .Sort(ss => ss.Descending(p => p.SequenceNr))
                .Query(
                    qr => qr.Term(
                        tr => tr.Field(
                            f => f.PersistenceId).Value(persistenceId))));

            //builder.ContinueWith<long>(p => p.Result.Documents
                    //.Where(entry => entry.PersistenceId.Equals(persistenceId))
                    //.Select(x => x.SequenceNr > fromSequenceNr).Single();
                    //;

            //Builders<MetadataEntry>.Filter;
            //var filter = builder.Eq(x => x.PersistenceId, persistenceId);

            //var highestSequenceNr = await _metadataCollection.Value.Find(filter).Project(x => x.SequenceNr).FirstOrDefaultAsync();

            return 8;
        }

        public override Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            //var messageList = messages.ToList();
            //var writeTasks = messageList.Select(async message =>
            //{
            //    var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();

            //    var journalEntries = persistentMessages.Select(ToJournalEntry).ToList();
            //    await _journalCollection.Value.InsertManyAsync(journalEntries);
            //});

            //await SetHighSequenceId(messageList);

            //return await Task<IImmutableList<Exception>>
            //    .Factory
            //    .ContinueWhenAll(writeTasks.ToArray(),
            //        tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null).ToImmutableList());
            return null;
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            //var builder = Builders<JournalEntry>.Filter;
            //var filter = builder.Eq(x => x.PersistenceId, persistenceId);

            //if (toSequenceNr != long.MaxValue)
            //    filter &= builder.Lte(x => x.SequenceNr, toSequenceNr);

            //return _journalCollection.Value.DeleteManyAsync(filter);
            return null;
        }

        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var messageList = messages.ToList();
            var writeTasks = messageList.Select(async message =>
            {
                var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();

                var journalEntries = persistentMessages.Select(ToJournalEntry).ToList();
                await _elasticClient.IndexManyAsync(journalEntries, _indexStore, _typeJournal);
            });

            await SetHighSequenceId(messageList);

            return await Task<IImmutableList<Exception>>
                .Factory
                .ContinueWhenAll(writeTasks.ToArray(),
                    tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null).ToImmutableList());
        }

        private JournalEntry ToJournalEntry(IPersistentRepresentation message)
        {
            return new JournalEntry
            {
                Id = message.PersistenceId + "_" + message.SequenceNr,
                IsDeleted = message.IsDeleted,
                Payload = message.Payload,
                PersistenceId = message.PersistenceId,
                SequenceNr = message.SequenceNr,
                Manifest = message.Manifest
            };
        }

        private Persistent ToPersistenceRepresentation(JournalEntry entry, IActorRef sender)
        {
            return new Persistent(entry.Payload, entry.SequenceNr, entry.PersistenceId, entry.Manifest, entry.IsDeleted, sender);
        }

        private async Task SetHighSequenceId(IList<AtomicWrite> messages)
        {
            var persistenceId = messages.Select(c => c.PersistenceId).First();
            var highSequenceId = messages.Max(c => c.HighestSequenceNr);

            var metadataEntry = new MetadataEntry
            {
                Id = persistenceId,
                PersistenceId = persistenceId,
                SequenceNr = highSequenceId
            };

            await _elasticClient.UpdateAsync<MetadataEntry, MetadataEntry>(
                    DocumentPath<MetadataEntry>.Id(metadataEntry.Id),
                    d => d.Doc(metadataEntry).DocAsUpsert());
        }
    }
}
