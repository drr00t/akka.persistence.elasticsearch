using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Persistence.Elasticsearch
{
    public class ElasticsearchSettings
    {
        public string ConnectionString { get; private set; }

        public bool AutoInitialize { get; private set; }

        public string Collection { get; private set; }

        protected ElasticsearchSettings(Config config)
        {
            ConnectionString = config.GetString("connection-string");
            Collection = config.GetString("collection");
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    public class ElasticsearchJournalSettings: ElasticsearchSettings
    {
        public string MetadataCollection { get; private set; }

        public ElasticsearchJournalSettings(Config config):base(config)
        {
            if(config == null)
            {
                throw new ArgumentException(nameof(config),
                    "Elasticsearch journal settings cannot be initialized, because required HOCON section couldn't been found");
            }

            MetadataCollection = config.GetString("metadata-collection");
        }
    }

    public class ElasticsearchSnapshotSettings : ElasticsearchSettings
    {
        public ElasticsearchSnapshotSettings(Config config) : base(config)
        {
            if (config == null)
            {
                throw new ArgumentException(nameof(config),
                    "Elasticsearch snapshot settings cannot be initialized, because required HOCON section couldn't been found");
            }
        }
    }
}
