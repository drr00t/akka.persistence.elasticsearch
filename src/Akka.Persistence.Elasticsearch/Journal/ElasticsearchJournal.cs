﻿using Akka.Persistence.Journal;
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
                    .DefaultIndex("eventstore-dados")
                    .EnableHttpCompression()
            ;

            _elasticClient = new ElasticClient();

            //if (_settings.AutoInitialize)
            //{
            //    if (!_elasticClient.IndexExists(String.Format("{0}-{1}",_indexStore, "dados")).Exists)
            //    {
            //        _elasticClient.CreateIndex(String.Format("{0}-{1}", _indexStore, "dados"), c => c.Settings(s => s
            //        .NumberOfReplicas(0)
            //        .NumberOfShards(1))
            //        .Mappings(m => m.Map<JournalEntry>(map => map.AutoMap()))
            //        .Mappings(m => m.Map<MetadataEntry>(map => map.AutoMap())));
            //    }                
            //}
            
            //database or index setup

            // if autoinitialize

            // create de journal indexes
            //journal collection load ordered by lastest Sequencial number from persistence id

            //create metadata indexes 

        }

        public override async Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            var metadataEntry = _elasticClient.SearchAsync<MetadataEntry>(
                s => s.Size(1)
                .Sort(ss => ss.Descending(p => p.SequenceNr))
                .Query(
                    qr => qr.Term(
                        tr => tr.Field(
                            f => f.PersistenceId).Value(persistenceId)))
                            .Index(Indices.Index(new IndexName { Name = String.Format("{0}-{1}", _indexStore, persistenceId), Type = typeof(MetadataEntry) })));

            return await metadataEntry.ContinueWith<long>(p => p.Result.Documents.Single().SequenceNr);// .Select(x => x.SequenceNr > fromSequenceNr));

            

            //Builders<MetadataEntry>.Filter;
            //var filter = builder.Eq(x => x.PersistenceId, persistenceId);

            //var highestSequenceNr = await _metadataCollection.Value.Find(filter).Project(x => x.SequenceNr).FirstOrDefaultAsync();

            //return 8;
        }

        public override async Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            // Do not replay messages if limit equal zero
            if (max == 0)
                return;

            var entries = await _elasticClient.SearchAsync<JournalEntry>(
                s => s.Sort(ss => ss.Ascending(p => p.SequenceNr))
                .Query(
                    qr => qr.Range(
                        rg => rg.Field(f => f.SequenceNr)
                            .GreaterThanOrEquals(fromSequenceNr)
                            .LessThanOrEquals(toSequenceNr)))
                            .Index(Indices.Index(new IndexName { Name = "eventstore-dados", Type = typeof(JournalEntry) })));

            if (entries.Total == 0)
                return;

            foreach(var doc in entries.Documents)
            {
                recoveryCallback(ToPersistenceRepresentation(doc.Payload as JournalEntry, context.Sender));
            }
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            return _elasticClient.DeleteAsync(DocumentPath<JournalEntry>.Id(toSequenceNr), u => u.Index(String.Format("{0}-{1}", _indexStore, persistenceId)));
        }

        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var messageList = messages.ToList();
            var writeTasks = messageList.Select(async message =>
            {
                var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();

                var journalEntries = persistentMessages.Select(ToJournalEntry).ToList();
                await _elasticClient.IndexManyAsync(journalEntries, new IndexName { Name = "eventstore-dados" });
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
                Id = message.PersistenceId + "-" + message.SequenceNr,
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
                Id = String.Format("{0}-{1}", "dados", highSequenceId),
                PersistenceId = persistenceId,
                SequenceNr = highSequenceId
            };

            await _elasticClient.IndexAsync<MetadataEntry>(metadataEntry,d => d.Index(new IndexName { Name = "eventstore-dados"}));
        }
    }
}
