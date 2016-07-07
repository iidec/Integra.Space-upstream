//-----------------------------------------------------------------------
// <copyright file="Pipeline.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    using System;

    /// <summary>
    /// Pipeline class.
    /// </summary>
    /// <typeparam name="TIn">Input type source filter.</typeparam>
    /// <typeparam name="TLink">Output type source filter, Input type destination filter.</typeparam>
    /// <typeparam name="TOut">Output type destination filter.</typeparam>
    internal class Pipeline<TIn, TLink, TOut> : Filter<TIn, TOut>
    {
        /// <summary>
        /// The source filter.
        /// </summary>
        private readonly Filter<TIn, TLink> source;

        /// <summary>
        /// The destination of outputs.
        /// </summary>
        private readonly Filter<TLink, TOut> destination;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline{TIn,TLink,TOut}"/> class.
        /// </summary>
        /// <param name="source">The source of inputs.</param>
        /// <param name="destination">The destination of outputs.</param>
        public Pipeline(Filter<TIn, TLink> source, Filter<TLink, TOut> destination)
        {
            this.source = source;
            this.destination = destination;
        }

        /// <inheritdoc />
        public override TOut Execute(TIn input)
        {
            TLink source = this.source.Execute(input);
            TOut result = this.destination.Execute(source);
            return result;
        }

        /// <inheritdoc />
        public override void OnError(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
