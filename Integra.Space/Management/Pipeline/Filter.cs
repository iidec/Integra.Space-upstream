//-----------------------------------------------------------------------
// <copyright file="Filter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline
{
    /// <summary>
    /// Filter class.
    /// </summary>
    /// <typeparam name="TIn">Input type.</typeparam>
    /// <typeparam name="TOut">Output type.</typeparam>
    internal abstract class Filter<TIn, TOut>
    {
        /// <summary>
        /// Flag that indicates whether the filter was executed.
        /// </summary>
        private bool executed;

        /// <summary>
        /// Gets or sets a value indicating whether the filter was executed.
        /// </summary>
        public bool Executed
        {
            get
            {
                return this.executed;
            }

            set
            {
                if (value == true)
                {
                    this.executed = value;
                }
            }
        }

        /// <summary>
        /// Allow to implement the logic related with the filter or step.
        /// </summary>
        /// <param name="input">A enumerable of inputs.</param>
        /// <returns>A enumerable of outputs.</returns>
        public abstract TOut Execute(TIn input);

        /// <summary>
        /// Implements the operations to execute when an error occurs in the Execute method.
        /// </summary>
        /// <param name="e">Exception thrown.</param>
        public abstract void OnError(TIn e);
    }
}
