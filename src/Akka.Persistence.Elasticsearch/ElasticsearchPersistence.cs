using Akka.Actor;
using Akka.Configuration;
using System;

namespace Akka.Persistence.Elasticsearch
{
    public class ElasticsearchPersistence: IExtension
    {
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<ElasticsearchPersistence>("Akka.Persistence.Elasticsearch.reference.conf");
        }

        public static ElasticsearchPersistence Get(ActorSystem system)
        {
            return system.WithExtension<ElasticsearchPersistence, ElasticsearchPersistenceProvider>();
        }

        public ElasticsearchJournalSettings JournalSettings { get; }

        public ElasticsearchSnapshotSettings SnapshotStoreSettings { get; }

        public ElasticsearchPersistence(ExtendedActorSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            system.Settings.InjectTopLevelFallback(DefaultConfiguration());

            var journalConfig = system.Settings.Config.GetConfig("akka.persistence.journal.elasticsearch");
            JournalSettings = new ElasticsearchJournalSettings(journalConfig);

            var snapshotConfig = system.Settings.Config.GetConfig("akka.persistence.snapshot-store.elasticsearch");
            SnapshotStoreSettings = new ElasticsearchSnapshotSettings(snapshotConfig);
        }
    }
}
