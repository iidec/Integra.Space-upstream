//-----------------------------------------------------------------------
// <copyright file="SpecificFilterSelector.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using Integra.Space.Common;
    using Integra.Space.Pipeline;

    /// <summary>
    /// Specific filter selector class.
    /// </summary>
    internal static class SpecificFilterSelector
    {
        /// <summary>
        /// Dictionary of filters.
        /// </summary>
        private static Dictionary<SpecificFilterKey, Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext>> filterDictionary;

        /// <summary>
        /// Initializes static members of the <see cref="SpecificFilterSelector"/> class.
        /// </summary>
        static SpecificFilterSelector()
        {
            filterDictionary = new Dictionary<SpecificFilterKey, Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext>>(new Comparer());

            // add the specific filters.
            filterDictionary.Add(new SpecificFilterKey(SpaceActionCommandEnum.Create, SpaceObjectEnum.Source), new CreateSourceFilter());
            filterDictionary.Add(new SpecificFilterKey(SpaceActionCommandEnum.Create, SpaceObjectEnum.Stream), new CreateStreamFilter().AddStep(new FilterQueryParser()));
        }

        /// <summary>
        /// Gets the specific filter based on the specified key.
        /// </summary>
        /// <param name="key">Key of the dictionary.</param>
        /// <returns>The filter assigned for the specified key.</returns>
        public static Filter<PipelineExecutionCommandContext, PipelineExecutionCommandContext> GetSpecificFilter(SpecificFilterKey key)
        {
            if (filterDictionary.ContainsKey(key))
            {
                return filterDictionary[key];
            }
            else
            {
                throw new System.Exception(string.Format("Not implement command. Command: '{0}', Object type: '{1}'", key.Action, key.SpaceObjectType));
            }
        }

        /// <summary>
        /// Comparer for the key of the filters dictionary.
        /// </summary>
        private class Comparer : EqualityComparer<SpecificFilterKey>
        {
            /// <inheritdoc />
            public override bool Equals(SpecificFilterKey x, SpecificFilterKey y)
            {
                if (x.Action == y.Action && x.SpaceObjectType == y.SpaceObjectType)
                {
                    return true;
                }

                return false;
            }

            /// <inheritdoc />
            public override int GetHashCode(SpecificFilterKey obj)
            {
                return obj.Action.GetHashCode() + obj.SpaceObjectType.GetHashCode();
            }
        }
    }
}
