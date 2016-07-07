using Integra.Space.Repos;

namespace Integra.Space.UnitTests
{
    class TestModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<SourceCacheRepository>().ToProvider(typeof(TestProvider)).InSingletonScope();
        }
    }
}
