//-----------------------------------------------------------------------
// <copyright file="LoginMetadataQueryFilter.cs" company="Integra.Space">
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
    internal class LoginMetadataQueryFilter : MetadataQueryParserFilter<Login>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginMetadataQueryFilter"/> class.
        /// </summary>
        public LoginMetadataQueryFilter() : base(SystemObjectEnum.Login, "LoginName")
        {
        }

        /// <inheritdoc />
        protected override DbSet<Login> GetDbSet(SpaceDbContext context)
        {
            return context.Logins;
        }

        /// <inheritdoc />
        protected override Func<Login, dynamic> GetObjectKeySelector()
        {
            return x => new { x.ServerId, x.LoginId };
        }

        /// <inheritdoc />
        protected override Func<Login, bool> GetPredicateForExtensionAny(Login @object)
        {
            return x => x.ServerId == @object.ServerId && x.LoginId == @object.LoginId;
        }

        /// <inheritdoc />
        protected override Func<ViewPermission, dynamic> GetViewPermissionKeySelector()
        {
            return x => new { x.ServerIdOfSecurable, x.SecurableId };
        }
    }
}