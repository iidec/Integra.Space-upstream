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
    internal class DatabaseMetadataQueryFilter : MetadataQueryParserFilter<DatabaseView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMetadataQueryFilter"/> class.
        /// </summary>
        public DatabaseMetadataQueryFilter() : base(SystemObjectEnum.Database, "DatabaseName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<DatabaseView> GetDbSet(SpaceDbContext context)
        {
            return context.DatabasesView;
        }

        /// <inheritdoc />
        protected override Func<Space.Database.DatabaseView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId };
        }

        /// <inheritdoc />
        protected override Func<DatabaseView, bool> GetPredicateForExtensionAny(DatabaseView @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}