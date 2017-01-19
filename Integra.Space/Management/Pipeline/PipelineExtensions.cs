//-----------------------------------------------------------------------
// <copyright file="PipelineExtensions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Pipeline extensions class.
    /// </summary>
    internal static class PipelineExtensions
    {
        /// <summary>
        /// Add a new filter in the pipeline.
        /// </summary>
        /// <typeparam name="TIn">Input type.</typeparam>
        /// <typeparam name="TLink">Link type.</typeparam>
        /// <typeparam name="TOut">Output type.</typeparam>
        /// <param name="currentFilter">Current filter.</param>
        /// <param name="nextFilter">Filter to add to the pipeline.</param>
        /// <returns>Pipeline with the new filter added.</returns>
        public static Filter<TIn, TOut> AddStep<TIn, TLink, TOut>(this Filter<TIn, TLink> currentFilter, Filter<TLink, TOut> nextFilter)
        {
            return new Pipeline<TIn, TLink, TOut>(currentFilter, nextFilter);
        }

        /// <summary>
        /// Execute the pipeline.
        /// </summary>
        /// <typeparam name="TIn">Input type.</typeparam>
        /// <typeparam name="TOut">Output type.</typeparam>
        /// <param name="currentFilter">Current filter.</param>
        /// <param name="input">Pipeline input.</param>
        /// <returns>Output of the pipeline.</returns>
        public static TOut Run<TIn, TOut>(this Filter<TIn, TOut> currentFilter, TIn input)
        {
            return currentFilter.Execute(input);
        }
    }
}
