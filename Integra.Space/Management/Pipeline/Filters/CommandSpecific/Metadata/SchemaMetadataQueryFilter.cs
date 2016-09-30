//-----------------------------------------------------------------------
// <copyright file="SchemaMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class SchemaMetadataQueryFilter : MetadataQueryParserFilter<Schema>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaMetadataQueryFilter"/> class.
        /// </summary>
        public SchemaMetadataQueryFilter() : base(SystemObjectEnum.Schema, "SchemaName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<Schema> GetDbSet(SpaceDbContext context)
        {
            return context.Schemas;
        }

        /// <inheritdoc />
        protected override Func<Schema, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.SchemaId };
        }

        /// <inheritdoc />
        protected override Func<Schema, bool> GetPredicateForExtensionAny(Schema @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.SchemaId == @object.SchemaId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SecurableId };
        }
    }
}