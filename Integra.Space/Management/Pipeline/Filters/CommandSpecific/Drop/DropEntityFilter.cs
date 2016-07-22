//-----------------------------------------------------------------------
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
    internal class DropEntityFilter<TEntity> : CommandFilter where TEntity : SystemObject
    {
        /// <summary>
        /// Entity to used for reverse changes.
        /// </summary>
        private TEntity oldEntity;

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);

            if (entity != null)
            {
                this.oldEntity = entity;
                sr.Delete(entity);
            }

            // throw new System.Exception("Test");
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            if (!this.Executed)
            {
                return;
            }

            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (entity == null)
            {
                // this.oldEntityData = this.CloneEntity(entity);
                this.CreateEntity(entity, context);
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">Entity to edit.</param>
        /// <param name="context">Context of the pipeline.</param>
        private void CreateEntity(TEntity entity, PipelineContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            sr.Add(entity);
        }
    }
}
