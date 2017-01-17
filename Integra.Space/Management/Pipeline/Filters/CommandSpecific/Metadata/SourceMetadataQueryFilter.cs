//-----------------------------------------------------------------------
// <copyright file="SourceMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class SourceMetadataQueryFilter : MetadataQueryParserFilter<SourceView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceMetadataQueryFilter"/> class.
        /// </summary>
        public SourceMetadataQueryFilter() : base(SystemObjectEnum.Source, "SourceName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<SourceView> GetDbSet(SpaceDbContext context)
        {
            return context.SourcesView;
        }

        /// <inheritdoc />
        protected override Func<SourceView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.SchemaId, x.SourceId };
        }

        /// <inheritdoc />
        protected override Func<SourceView, bool> GetPredicateForExtensionAny(SourceView @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.SchemaId == @object.SchemaId && x.SourceId == @object.SourceId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SchemaIdOfSecurable, x.SecurableId };
        }
    }
}