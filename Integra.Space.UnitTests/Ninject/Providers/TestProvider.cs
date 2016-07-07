using Integra.Space.Cache;
using Integra.Space.Models;
using Integra.Space.Repos;
using Ninject.Activation;
using System;
using System.Collections.Generic;

namespace Integra.Space.UnitTests
{
    class TestProvider : Provider<SourceCacheRepository>
    {
        protected override SourceCacheRepository CreateInstance(IContext context)
        {
            List<Source> sources = new List<Source>();
            sources.Add(new Source(Guid.NewGuid(), "Source2"));

            SourceCacheRepository sr = new SourceCacheRepository(sources);
            return sr;
        }
    }
}
