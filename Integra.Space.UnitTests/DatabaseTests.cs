using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Database;
using System.Data.Entity.Migrations;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void Migrations()
        {
            Configuration configuration = new Configuration();
            DbMigrator migrator = new DbMigrator(configuration);
            migrator.Update();
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (SpaceDbContext context = new SpaceDbContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
