using Integra.Space.Cache;
using Integra.Space.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests.NinjectModules
{
    class TestModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<SourceRepository>().ToProvider(typeof(TestProvider)).InSingletonScope();
        }
    }
}
