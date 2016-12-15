using Akka.Configuration;
using Akka.Persistence.Elasticsearch;
using Akka.Persistence.TestKit.Journal;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Elasticsearch.Tests
{

    [Collection("ElasticsearchJournalExampleSpec")]
    public class ElasticsearchJournalExampleSpec : Xunit.
    {
        protected override bool SupportsRejectingNonSerializableObjects { get; } = false;

        private static readonly Config SpecConfig;

        static ElasticsearchJournalExampleSpec()
        {
            string specConfig = @"
            akka.test.single-expect-default = 3s
            akka.persistence {
                publish-plugin-commands = on
                journal {
                    plugin = ""akka.persistence.journal.elasticsearch""
                    elasticsearch {
                        class = ""Akka.Persistence.Elasticsearch.Journal.ElasticsearchJournal, Akka.Persistence.Elasticsearch""
                        connection-string = ""<ConnectionString>""
                        auto-initialize = on
                        collection = ""EventJournal""
                    }
                }
            }";

            SpecConfig = ConfigurationFactory.ParseString(specConfig);
   
        }
    }
}
