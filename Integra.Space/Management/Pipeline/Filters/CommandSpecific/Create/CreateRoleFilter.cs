//-----------------------------------------------------------------------
// <copyright file="CreateRoleFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Models;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateRoleFilter : CreateEntityFilter<Role>
    {
        /// <inheritdoc />
        protected override Role CreateEntity(PipelineExecutionCommandContext context)
        {
            return new Role(Guid.NewGuid(), context.Command.ObjectName);
        }
    }
}
