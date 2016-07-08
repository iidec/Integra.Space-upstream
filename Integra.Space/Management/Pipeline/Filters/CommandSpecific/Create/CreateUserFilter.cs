//-----------------------------------------------------------------------
// <copyright file="CreateUserFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Models;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateUserFilter : CreateEntityFilter<User>
    {
        /// <inheritdoc />
        protected override User CreateEntity(PipelineExecutionCommandContext context)
        {
            List<SpaceUserOption> options = ((Language.CreateAndAlterUserNode)context.Command).UserOptions;
            return new User(Guid.NewGuid(), context.Command.ObjectName, (string)options.First(x => x.Option == SpaceUserOptionEnum.Password).Value, (bool)options.First(x => x.Option == SpaceUserOptionEnum.Status).Value);
        }
    }
}
