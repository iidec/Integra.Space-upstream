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
    internal class DatabaseRoleMetadataQueryFilter : MetadataQueryParserFilter<DatabaseRoleView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRoleMetadataQueryFilter"/> class.
        /// </summary>
        public DatabaseRoleMetadataQueryFilter() : base(SystemObjectEnum.DatabaseRole, "DatabaseRoleName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<DatabaseRoleView> GetDbSet(SpaceDbContext context)
        {
            return context.DatabaseRolesView;
        }

        /// <inheritdoc />
        protected override Func<DatabaseRoleView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.DatabaseRoleId };
        }

        /// <inheritdoc />
        protected override Func<DatabaseRoleView, bool> GetPredicateForExtensionAny(DatabaseRoleView @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.DatabaseRoleId == @object.DatabaseRoleId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SecurableId };
        }
    }
}