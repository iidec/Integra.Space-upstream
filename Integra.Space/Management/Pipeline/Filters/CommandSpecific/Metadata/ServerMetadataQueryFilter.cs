//-----------------------------------------------------------------------
// <copyright file="ServerMetadataQueryFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Data.Entity;
    using Database;

    /// <summary>
    /// Create command action class.
    /// </summary>
    internal class ServerMetadataQueryFilter : MetadataQueryParserFilter<Server>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMetadataQueryFilter"/> class.
        /// </summary>
        public ServerMetadataQueryFilter() : base(Common.SystemObjectEnum.Server, "ServerName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<Server> GetDbSet(SpaceDbContext context)
        {
            return context.Servers;
        }

        /// <inheritdoc />
        protected override Func<Server, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId };
        }

        /// <inheritdoc />
        protected override Func<Server, bool> GetPredicateForExtensionAny(Server @object)
        {
            return x => x.ServerId == @object.ServerId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.SecurableId };
        }
    }
}