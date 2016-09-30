//-----------------------------------------------------------------------
// <copyright file="DatabaseMetadataQueryFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Data.Entity;
    using Common;
    using Database;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class DatabaseMetadataQueryFilter : MetadataQueryParserFilter<Space.Database.Database>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMetadataQueryFilter"/> class.
        /// </summary>
        public DatabaseMetadataQueryFilter() : base(SystemObjectEnum.Database, "DatabaseName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<Space.Database.Database> GetDbSet(SpaceDbContext context)
        {
            return context.Databases;
        }

        /// <inheritdoc />
        protected override Func<Space.Database.Database, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId };
        }

        /// <inheritdoc />
        protected override Func<Space.Database.Database, bool> GetPredicateForExtensionAny(Space.Database.Database @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}