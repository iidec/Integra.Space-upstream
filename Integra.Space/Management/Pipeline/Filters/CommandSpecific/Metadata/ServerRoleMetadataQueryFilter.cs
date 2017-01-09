//-----------------------------------------------------------------------
// <copyright file="ServerRoleMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class ServerRoleMetadataQueryFilter : MetadataQueryParserFilter<ServerRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRoleMetadataQueryFilter"/> class.
        /// </summary>
        public ServerRoleMetadataQueryFilter() : base(SystemObjectEnum.ServerRole, "ServerRoleName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<ServerRole> GetDbSet(SpaceDbContext context)
        {
            return context.ServerRoles;
        }

        /// <inheritdoc />
        protected override Func<ServerRole, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.ServerRoleId };
        }

        /// <inheritdoc />
        protected override Func<ServerRole, bool> GetPredicateForExtensionAny(ServerRole @object)
        {
            return x => x.ServerId == @object.ServerId && x.ServerRoleId == @object.ServerRoleId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}