//-----------------------------------------------------------------------
// <copyright file="UserMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class UserMetadataQueryFilter : MetadataQueryParserFilter<UserView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserMetadataQueryFilter"/> class.
        /// </summary>
        public UserMetadataQueryFilter() : base(SystemObjectEnum.DatabaseUser, "UserName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<UserView> GetDbSet(SpaceDbContext context)
        {
            return context.UsersView;
        }

        /// <inheritdoc />
        protected override Func<UserView, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.DatabaseId, x.UserId };
        }

        /// <inheritdoc />
        protected override Func<UserView, bool> GetPredicateForExtensionAny(UserView @object)
        {
            return x => x.ServerId == @object.ServerId && x.DatabaseId == @object.DatabaseId && x.UserId == @object.UserId;
        }

        /// <inheritdoc />
        protected override Func<PermissionView, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.DatabaseIdOfSecurable, x.SecurableId };
        }
    }
}