//-----------------------------------------------------------------------
// <copyright file="MyMigrateDatabaseToLatestVersion.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Database
{
    using System;
    using System.Data.Entity;
    using System.IO;
    /// <summary>
    /// Custom initializer class.
    /// </summary>
    internal class MyMigrateDatabaseToLatestVersion : MigrateDatabaseToLatestVersion<SpaceDbContext, Configuration>
    {
        public MyMigrateDatabaseToLatestVersion() : base("SpaceConnection")
        {
        }

        /// <inheritdoc />
        public override void InitializeDatabase(SpaceDbContext context)
        {
            base.InitializeDatabase(context);

            using (DbContextTransaction tran = context.Database.BeginTransaction())
            {
                try
                {
                    string comands = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + @"Scripts\CheckConstraints.sql");
                    context.Database.ExecuteSqlCommand(comands);
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                }
            }
        }
    }
}
