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
    /// Filter create user class.
    /// </summary>
    internal class CreateUserFilter : CreateEntityFilter<User>
    {
        /// <inheritdoc />
        protected override User CreateEntity(PipelineExecutionCommandContext context)
        {
            List<SpaceUserOption> options = ((Language.CreateAndAlterUserNode)context.Command).UserOptions;

            if (options.Count > 0 && options.Exists(x => x.Option == SpaceUserOptionEnum.Password))
            {
                if (options.Exists(x => x.Option == SpaceUserOptionEnum.Status))
                {
                    return new User(Guid.NewGuid(), context.Command.ObjectName, (string)options.First(x => x.Option == SpaceUserOptionEnum.Password).Value, (bool)options.First(x => x.Option == SpaceUserOptionEnum.Status).Value);
                }
                else
                {
                    return new User(Guid.NewGuid(), context.Command.ObjectName, (string)options.First(x => x.Option == SpaceUserOptionEnum.Password).Value, false);
                }
            }
            else
            {
                throw new Exception("Must define a password for the user.");
            }
        }
    }
}
