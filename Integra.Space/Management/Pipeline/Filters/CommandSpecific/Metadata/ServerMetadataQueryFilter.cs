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
    internal class ServerMetadataQueryFilter : MetadataQueryParserFilter<ServerView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMetadataQueryFilter"/> class.
        /// </summary>
        public ServerMetadataQueryFilter() : base(Common.SystemObjectEnum.Server, "ServerName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<ServerView> GetDbSet(SpaceDbContext context)
        {
            return context.ServersView;
        }

        /// <inheritdoc />
        protected override Func<ServerView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId };
        }

        /// <inheritdoc />
        protected override Func<ServerView, bool> GetPredicateForExtensionAny(ServerView @object)
        {
            return x => x.ServerId == @object.ServerId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.SecurableId };
        }
    }
}