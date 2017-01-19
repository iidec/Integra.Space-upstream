//-----------------------------------------------------------------------
// <copyright file="EndpointMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class EndpointMetadataQueryFilter : MetadataQueryParserFilter<EndpointView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointMetadataQueryFilter"/> class.
        /// </summary>
        public EndpointMetadataQueryFilter() : base(SystemObjectEnum.Endpoint, "EnpointName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<EndpointView> GetDbSet(SpaceDbContext context)
        {
            return context.EndpointsView;
        }

        /// <inheritdoc />
        protected override Func<EndpointView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.EndpointId };
        }

        /// <inheritdoc />
        protected override Func<EndpointView, bool> GetPredicateForExtensionAny(EndpointView @object)
        {
            return x => x.ServerId == @object.ServerId && x.EndpointId == @object.EndpointId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}