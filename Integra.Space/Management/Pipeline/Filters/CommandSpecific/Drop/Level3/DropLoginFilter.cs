//-----------------------------------------------------------------------
// <copyright file="DropLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using Database;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal class DropLoginFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(SpaceDbContext databaseContext, Schema schema, string name)
        {
            Login login = databaseContext.Logins.Single(x => x.ServerId == schema.ServerId
                                            && x.LoginName == name);

            databaseContext.Logins.Remove(login);
            databaseContext.SaveChanges();
        }
    }
}
