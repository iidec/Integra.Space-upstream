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
    internal class ServerRoleMetadataQueryFilter : MetadataQueryParserFilter<ServerRoleView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRoleMetadataQueryFilter"/> class.
        /// </summary>
        public ServerRoleMetadataQueryFilter() : base(SystemObjectEnum.ServerRole, "ServerRoleName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<ServerRoleView> GetDbSet(SpaceDbContext context)
        {
            return context.ServerRolesView;
        }

        /// <inheritdoc />
        protected override Func<ServerRoleView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.ServerRoleId };
        }

        /// <inheritdoc />
        protected override Func<ServerRoleView, bool> GetPredicateForExtensionAny(ServerRoleView @object)
        {
            return x => x.ServerId == @object.ServerId && x.ServerRoleId == @object.ServerRoleId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}