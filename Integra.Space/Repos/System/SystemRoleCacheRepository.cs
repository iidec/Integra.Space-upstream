//-----------------------------------------------------------------------
// <copyright file="SystemRoleCacheRepository.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Repos
{
    using System;
    using System.Collections.Generic;
    using Cache;
    using Models;

    /// <summary>
    /// Space object repository class.
    /// </summary>
    internal class SystemRoleCacheRepository : SystemRepositoryBase<SystemRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemRoleCacheRepository"/> class.
        /// </summary>
        /// <param name="context">Cache context.</param>
        public SystemRoleCacheRepository(SystemContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public SystemRole FindByName(Common.SystemRolesEnum roleType)
        {
            lock (this.Sync)
            {
                return this.Context.SystemRoles.Find(x => x.RoleType == roleType);
            }
        }
    }
}
