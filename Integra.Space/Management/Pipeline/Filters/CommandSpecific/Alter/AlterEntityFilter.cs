//-----------------------------------------------------------------------
// <copyright file="AlterEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    internal abstract class AlterEntityFilter<TEntity> : CommandFilter where TEntity : SystemObject
    {
        /// <summary>
        /// Entity to used for reverse changes.
        /// </summary>
        private TEntity oldEntityData;

        /// <summary>
        /// Gets the entity with 
        /// </summary>
        public TEntity OldEntityData
        {
            get
            {
                return this.oldEntityData;
            }
        }

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (entity != null)
            {
                this.oldEntityData = this.CloneEntity(entity);
                this.DoChanges(entity, context);
            }
            else
            {
                throw new System.Exception(string.Format("The entity '{0}' does not exist.", context.Command.ObjectName));
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            // siempre se reversan los cambios aunque no se haya ejecutado el paso actual.
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (this.oldEntityData != null)
            {
                this.ReverseChanges(entity, context);
            }
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">Entity to edit.</param>
        /// <param name="context">Context of the pipeline.</param>
        protected abstract void DoChanges(TEntity entity, PipelineContext context);

        /// <summary>
        /// Reverse the changes of the entity.
        /// </summary>
        /// <param name="entity">Entity edited.</param>
        /// <param name="context">Context of the pipeline.</param>
        protected abstract void ReverseChanges(TEntity entity, PipelineContext context);

        /// <summary>
        /// Clone the entity that going to be modify.
        /// </summary>
        /// <param name="entityToClone">Entity to clone.</param>
        /// <returns>The clone of the specify entity.</returns>
        protected abstract TEntity CloneEntity(TEntity entityToClone);
    }
}
