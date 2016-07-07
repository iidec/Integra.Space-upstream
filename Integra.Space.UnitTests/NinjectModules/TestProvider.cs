using Integra.Space.Cache;
using Integra.Space.Models;
using Ninject.Activation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests.NinjectModules
{
    class TestProvider : Provider<SourceRepository>
    {
        protected override SourceRepository CreateInstance(IContext context)
        {
            List<Source> sources = new List<Source>();
            sources.Add(new Source(Guid.NewGuid(), "Source2"));

            SourceRepository sr = new SourceRepository(sources);
            return sr;
        }
    }
}
