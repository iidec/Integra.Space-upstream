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
    internal class SourceMetadataQueryFilter : MetadataQueryParserFilter<Source>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceMetadataQueryFilter"/> class.
        /// </summary>
        public SourceMetadataQueryFilter() : base(SystemObjectEnum.Source, "SourceName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<Source> GetDbSet(SpaceDbContext context)
        {
            return context.Sources;
        }

        /// <inheritdoc />
        protected override Func<Source, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.SchemaId, x.SourceId };
        }

        /// <inheritdoc />
        protected override Func<Source, bool> GetPredicateForExtensionAny(Source @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.SchemaId == @object.SchemaId && x.SourceId == @object.SourceId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SchemaIdOfSecurable, x.SecurableId };
        }
    }
}