using Integra.Space.Database;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.UnitTests
{
    public class MockEnvironment
    {
        public Mock<DbSet<T>> MethodTest<T>(IQueryable<T> data) where T : class
        {
            Mock<DbSet<T>> mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            return mockSet;
        }

        public void CreateDefaultEnvironment()
        {
            var inputSourceColumns = this.MethodTest(DefaultSystemObjects.CreateInputSourceColumn());
            var sources = this.MethodTest(DefaultSystemObjects.CreateSources());

            var mockContext = new Mock<SpaceDbContext>();
            mockContext.Setup(c => c.Sources).Returns(sources.Object);
            var source = mockContext.Object.Sources.First(x => x.SourceName == "InputSourceTest");
            return;
        }
    }
}
