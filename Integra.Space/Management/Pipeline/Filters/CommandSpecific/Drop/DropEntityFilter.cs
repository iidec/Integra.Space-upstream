﻿//-----------------------------------------------------------------------
// <copyright file="DropEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;
    using Integra.Space.Pipeline;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Drop entity class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    internal abstract class DropEntityFilter<TEntity> : CommandFilter where TEntity : SpaceObject
    {
        /// <summary>
        /// Entity to used for reverse changes.
        /// </summary>
        private TEntity oldEntityData;

        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (entity != null)
            {
                sr.Delete(entity);
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }

            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (entity == null)
            {
                this.oldEntityData = this.CloneEntity(entity);
                this.CreateEntity(entity, context);
            }
        }

        /// <summary>
        /// Clone the entity that going to be modify.
        /// </summary>
        /// <param name="entityToClone">Entity to clone.</param>
        /// <returns>The clone of the specify entity.</returns>
        protected abstract TEntity CloneEntity(TEntity entityToClone);

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">Entity to edit.</param>
        /// <param name="context">Context of the pipeline.</param>
        private void CreateEntity(TEntity entity, PipelineExecutionCommandContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            sr.Add(entity);
        }
    }
}
