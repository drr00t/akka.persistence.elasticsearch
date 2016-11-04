using Akka.Persistence.Elasticsearch;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace Akka.Persistence.ElasticSearch.Tests
{

    [Collection("ElasticsearchJournalSpec")]
    public class ElasticsearchJournalSpec: JournalSpec
    {
        protected override bool SupportsRejectingNonSerializableObjects { get; } = false;

        private static readonly string SpecConfig = @"
            akka.test.single-expect-default = 3s
            akka.persistence {
                publish-plugin-commands = on
                journal {
                    plugin = ""akka.persistence.journal.mongodb""
                    mongodb {
                        class = ""Akka.Persistence.MongoDb.Journal.MongoDbJournal, Akka.Persistence.MongoDb""
                        connection-string = ""<ConnectionString>""
                        auto-initialize = on
                        collection = ""EventJournal""
                    }
                }
            }";

        [Fact]
        public void Should_ElasticSearch_journal_has_default_config()
        {
            var elasticsearch = ElasticsearchPersistence.Get(Sys);

            elasticsearch.JournalSettings.ConnectionString.Should().Be(string.Empty);
            elasticsearch.JournalSettings.AutoInitialize.Should().BeFalse();
            elasticsearch.JournalSettings.Collection.Should().Be("EventJournal");
            elasticsearch.JournalSettings.MetadataCollection.Should().Be("Metadata");

        }

        [Fact]
        public void ElasticSearch_SnapshotStoreSettingsSettings_must_have_default_values()
        {
            var elasticsearch = ElasticsearchPersistence.Get(Sys);

            elasticsearch.SnapshotStoreSettings.ConnectionString.Should().Be(string.Empty);
            elasticsearch.SnapshotStoreSettings.AutoInitialize.Should().BeFalse();
            elasticsearch.SnapshotStoreSettings.Collection.Should().Be("SnapshotStore");
        }
    }
}
