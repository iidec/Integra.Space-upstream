//-----------------------------------------------------------------------
// <copyright file="SpecificFilterKey.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Common;

    /// <summary>
    /// Specific filter key class.
    /// </summary>
    internal class SpecificFilterKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificFilterKey"/> class.
        /// </summary>
        /// <param name="action">Command action.</param>
        /// <param name="objectType">Space object type.</param>
        public SpecificFilterKey(SpaceActionCommandEnum action, SpaceObjectEnum objectType)
        {
            this.Action = action;
            this.SpaceObjectType = objectType;
        }

        /// <summary>
        /// Gets the command action.
        /// </summary>
        public SpaceActionCommandEnum Action { get; private set; }

        /// <summary>
        /// Gets the object type.
        /// </summary>
        public SpaceObjectEnum SpaceObjectType { get; private set; }
    }
}
