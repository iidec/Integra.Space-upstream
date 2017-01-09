//-----------------------------------------------------------------------
// <copyright file="UserMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class UserMetadataQueryFilter : MetadataQueryParserFilter<DatabaseUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserMetadataQueryFilter"/> class.
        /// </summary>
        public UserMetadataQueryFilter() : base(SystemObjectEnum.DatabaseUser, "DbUsrName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<DatabaseUser> GetDbSet(SpaceDbContext context)
        {
            return context.DatabaseUsers;
        }

        /// <inheritdoc />
        protected override Func<DatabaseUser, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.DbUsrId };
        }

        /// <inheritdoc />
        protected override Func<DatabaseUser, bool> GetPredicateForExtensionAny(DatabaseUser @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.DbUsrId == @object.DbUsrId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SecurableId };
        }
    }
}