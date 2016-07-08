//-----------------------------------------------------------------------
// <copyright file="CreateStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateStreamFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            this.CreateStream(context);
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }

            this.DeleteStream(context);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        private void CreateStream(PipelineExecutionCommandContext context)
        {
            Stream stream = new Stream(Guid.NewGuid(), context.Command.ObjectName, ((Language.CreateAndAlterStreamNode)context.Command).Query);
            IRepository<Stream> sr = context.Kernel.Get<IRepository<Stream>>();
            sr.Add(stream);
        }

        /// <summary>
        /// Delete a the source.
        /// </summary>
        /// <param name="context">Context of the pipeline.</param>
        private void DeleteStream(PipelineExecutionCommandContext context)
        {
            IRepository<Stream> sr = context.Kernel.Get<IRepository<Stream>>();
            Stream stream = sr.FindByName(context.Command.ObjectName);
            if (stream != null)
            {
                sr.Delete(stream);
            }
        }
    }
}
