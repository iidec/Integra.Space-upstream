//-----------------------------------------------------------------------
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
    using Language;
    using Models;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterUserFilter : AlterEntityFilter<User>
    {
        /// <inheritdoc />
        protected override User CloneEntity(User entityToClone)
        {
            return new User(entityToClone.Guid, string.Copy(entityToClone.Name), string.Copy(entityToClone.Password), entityToClone.Enable, entityToClone.DefaultSchema);
        }

        /// <inheritdoc />
        protected override void DoChanges(User entity, PipelineContext context)
        {
            List<UserOption> options = ((Language.CreateAndAlterUserNode)context.Command).UserOptions;

            if (options.Count > 0)
            {
                if (options.Exists(x => x.Option == UserOptionEnum.Status))
                {
                    entity.Enable = (bool)options.First(x => x.Option == UserOptionEnum.Status).Value;
                }

                if (options.Exists(x => x.Option == UserOptionEnum.Password))
                {
                    entity.Password = (string)options.First(x => x.Option == UserOptionEnum.Password).Value;
                }
            }
            else
            {
                throw new Exception("Must specify an option for the user.");
            }
        }

        /// <inheritdoc />
        protected override void ReverseChanges(User entity, PipelineContext context)
        {
            entity.Password = this.OldEntityData.Password;
            entity.Enable = this.OldEntityData.Enable;
        }
    }
}
