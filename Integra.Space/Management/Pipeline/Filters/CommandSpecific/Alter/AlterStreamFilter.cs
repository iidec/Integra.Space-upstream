//-----------------------------------------------------------------------
// <copyright file="AlterStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Models;

    /// <summary>
    /// Filter alter user class.
    /// </summary>
    internal class AlterStreamFilter : AlterEntityFilter<Stream>
    {
        /// <inheritdoc />
        protected override Stream CloneEntity(Stream entityToClone)
        {
            return new Stream(entityToClone.Guid, string.Copy(entityToClone.Name), string.Copy(entityToClone.Query), entityToClone.Schema);
        }

        /// <inheritdoc />
        protected override void DoChanges(Stream entity, PipelineContext context)
        {
            string query = ((Language.CreateAndAlterStreamNode)context.Command).Query;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                entity.Query = query;
            }
            else
            {
                throw new Exception("Must specify the query for the stream.");
            }
        }

        /// <inheritdoc />
        protected override void ReverseChanges(Stream entity, PipelineContext context)
        {
            entity.Query = this.OldEntityData.Query;
        }
    }
}
