using Akka.Persistence.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.Collections.Immutable;
using Akka.Persistence.Elasticsearch;

namespace Akka.Persistence.ElasticSearch.Journal
{
    public class ElasticsearchJournal : AsyncWriteJournal
    {
        private readonly ElasticsearchJournalSettings _settings;
        //private Lazy<IMongoDatabase> _mongoDatabase; _elasticClient
        //private Lazy<IMongoCollection<JournalEntry>> _journalCollection;
        //private Lazy<IMongoCollection<MetadataEntry>> _metadataCollection;

        public ElasticsearchJournal()
        {
            _settings = ElasticsearchPersistence.Get(Context.System).JournalSettings;
        }

        protected override void PreStart()
        {
            // client initialization

            //database or index setup

            // if autoinitialize

                // create de journal indexes
                //journal collection load ordered by lastest Sequencial number from persistence id

                //create metadata indexes 
            
        }

        public override Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            throw new NotImplementedException();
        }

        public override Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            throw new NotImplementedException();
        }

        protected override Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            throw new NotImplementedException();
        }
    }
}
