﻿//-----------------------------------------------------------------------
// <copyright file="AlterUserFilter.cs" company="Integra.Space">
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
    /// Filter alter user class.
    /// </summary>
    internal class AlterUserFilter : AlterEntityFilter<User>
    {
        /// <inheritdoc />
        protected override User CloneEntity(User entityToClone)
        {
            return new User(entityToClone.Guid, string.Copy(entityToClone.Identifier), string.Copy(entityToClone.Password), entityToClone.Enable);
        }

        /// <inheritdoc />
        protected override void DoChanges(User entity, PipelineExecutionCommandContext context)
        {
            List<SpaceUserOption> options = ((Language.CreateAndAlterUserNode)context.Command).UserOptions;

            if (options.Count > 0)
            {
                if (options.Exists(x => x.Option == SpaceUserOptionEnum.Status))
                {
                    entity.Enable = (bool)options.First(x => x.Option == SpaceUserOptionEnum.Status).Value;
                }

                if (options.Exists(x => x.Option == SpaceUserOptionEnum.Password))
                {
                    entity.Password = (string)options.First(x => x.Option == SpaceUserOptionEnum.Password).Value;
                }
            }
            else
            {
                throw new Exception("Must specify an option for the user.");
            }
        }

        /// <inheritdoc />
        protected override void ReverseChanges(User entity, PipelineExecutionCommandContext context)
        {
            entity.Password = this.OldEntityData.Password;
            entity.Enable = this.OldEntityData.Enable;
        }
    }
}
