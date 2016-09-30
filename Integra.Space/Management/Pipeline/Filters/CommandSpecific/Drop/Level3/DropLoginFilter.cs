//-----------------------------------------------------------------------
// <copyright file="DropLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Linq;
    using Database;
    using Integra.Space.Pipeline;
    using Ninject;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    internal class DropLoginFilter : DropEntityFilter
    {
        /// <inheritdoc />
        protected override void DropEntity(PipelineContext context)
        {
            SpaceDbContext databaseContext = context.Kernel.Get<SpaceDbContext>();
            Login login = databaseContext.Logins.Single(x => x.ServerId == context.CommandContext.Schema.ServerId
                                            && x.LoginName == ((Language.DDLCommand)context.CommandContext.Command).MainCommandObject.Name);

            databaseContext.Logins.Remove(login);
            databaseContext.SaveChanges();
        }
    }
}
