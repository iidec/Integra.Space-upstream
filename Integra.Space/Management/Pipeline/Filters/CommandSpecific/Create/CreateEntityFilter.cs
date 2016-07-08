//-----------------------------------------------------------------------
// <copyright file="CreateEntityFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    internal abstract class CreateEntityFilter<TEntity> : CommandFilter where TEntity : class
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            TEntity entity = this.CreateEntity(context);
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            sr.Add(entity);
            
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }

            this.DeleteSource(context);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        /// <returns>The created entity.</returns>
        protected abstract TEntity CreateEntity(PipelineExecutionCommandContext context);

        /// <summary>
        /// Delete a the source.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        protected virtual void DeleteSource(PipelineExecutionCommandContext context)
        {
            IRepository<TEntity> sr = context.Kernel.Get<IRepository<TEntity>>();
            TEntity entity = sr.FindByName(context.Command.ObjectName);
            if (entity != null)
            {
                sr.Delete(entity);
            }
        }
    }
}
