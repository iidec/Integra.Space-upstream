//-----------------------------------------------------------------------
// <copyright file="Filter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Language.Pipeline
{
    /// <summary>
    /// Filter class.
    /// </summary>
    /// <typeparam name="TIn">Input type.</typeparam>
    /// <typeparam name="TOut">Output type.</typeparam>
    internal abstract class Filter<TIn, TOut>
    {
        /// <summary>
        /// Allow to implement the logic related with the filter or step.
        /// </summary>
        /// <param name="input">A enumerable of inputs.</param>
        /// <returns>A enumerable of outputs.</returns>
        public abstract TOut Execute(TIn input);        
    }
}
