//-----------------------------------------------------------------------
// <copyright file="ViewMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class ViewMetadataQueryFilter : MetadataQueryParserFilter<View>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewMetadataQueryFilter"/> class.
        /// </summary>
        public ViewMetadataQueryFilter() : base(SystemObjectEnum.View, "ViewName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<View> GetDbSet(SpaceDbContext context)
        {
            return context.Views;
        }

        /// <inheritdoc />
        protected override Func<View, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.SchemaId, x.ViewId };
        }

        /// <inheritdoc />
        protected override Func<View, bool> GetPredicateForExtensionAny(View @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.SchemaId == @object.SchemaId && x.ViewId == @object.ViewId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SchemaIdOfSecurable, x.SecurableId };
        }
    }
}