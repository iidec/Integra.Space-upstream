//-----------------------------------------------------------------------
// <copyright file="DatabaseRoleMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class DatabaseRoleMetadataQueryFilter : MetadataQueryParserFilter<DatabaseRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRoleMetadataQueryFilter"/> class.
        /// </summary>
        public DatabaseRoleMetadataQueryFilter() : base(SystemObjectEnum.DatabaseRole, "DbRoleName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<DatabaseRole> GetDbSet(SpaceDbContext context)
        {
            return context.DatabaseRoles;
        }

        /// <inheritdoc />
        protected override Func<DatabaseRole, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.DbRoleId };
        }

        /// <inheritdoc />
        protected override Func<DatabaseRole, bool> GetPredicateForExtensionAny(DatabaseRole @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.DbRoleId == @object.DbRoleId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SecurableId };
        }
    }
}