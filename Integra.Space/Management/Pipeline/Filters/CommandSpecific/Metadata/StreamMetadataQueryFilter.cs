//-----------------------------------------------------------------------
// <copyright file="StreamMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class StreamMetadataQueryFilter : MetadataQueryParserFilter<StreamView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamMetadataQueryFilter"/> class.
        /// </summary>
        public StreamMetadataQueryFilter() : base(SystemObjectEnum.Stream, "StreamName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<StreamView> GetDbSet(SpaceDbContext context)
        {
            return context.StreamsView;
        }

        /// <inheritdoc />
        protected override Func<StreamView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.SchemaId, x.StreamId };
        }

        /// <inheritdoc />
        protected override Func<StreamView, bool> GetPredicateForExtensionAny(StreamView @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.SchemaId == @object.SchemaId && x.StreamId == @object.StreamId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SchemaIdOfSecurable, x.SecurableId };
        }
    }
}