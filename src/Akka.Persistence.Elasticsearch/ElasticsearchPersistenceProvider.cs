using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Persistence.Elasticsearch
{
    public class ElasticsearchPersistenceProvider : ExtensionIdProvider<ElasticsearchPersistence>
    {
        public override ElasticsearchPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new ElasticsearchPersistence(system);
        }
    }
}
