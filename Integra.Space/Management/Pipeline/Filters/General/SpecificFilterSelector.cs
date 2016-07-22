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
    using Models;

    /// <summary>
    /// Specific filter selector class.
    /// </summary>
    internal static class SpecificFilterSelector
    {
        /// <summary>
        /// Dictionary of filters.
        /// </summary>
        private static Dictionary<SpecificFilterKey, Filter<PipelineContext, PipelineContext>> filterDictionary;

        /// <summary>
        /// Initializes static members of the <see cref="SpecificFilterSelector"/> class.
        /// </summary>
        static SpecificFilterSelector()
        {
            filterDictionary = new Dictionary<SpecificFilterKey, Filter<PipelineContext, PipelineContext>>(new Comparer());

            // add the specific filters.
            // create
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Source), new CreateSourceFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Stream), new CreateStreamFilter().AddStep(new FilterQueryParser()));
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.User), new CreateUserFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Role), new CreateRoleFilter());

            // alter
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.User), new AlterUserFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Stream), new AlterStreamFilter());

            // drop
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Role), new DropEntityFilter<Role>());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Source), new DropEntityFilter<Source>());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Stream), new DropEntityFilter<Stream>());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.User), new DropEntityFilter<User>());

            // permission
            // grant
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Grant, SystemObjectEnum.User), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Grant, SystemObjectEnum.Role), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));
            
            // deny
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Deny, SystemObjectEnum.User), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Deny, SystemObjectEnum.Role), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));

            // revoke
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Revoke, SystemObjectEnum.User), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Revoke, SystemObjectEnum.Role), new GrantPermissionFilter<PermissionOverObjectType>().AddStep(new GrantPermissionFilter<PermissionOverSpecificObject>()));

            // add
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Add, SystemObjectEnum.Role), new AddSecureObjectToRoleFilter());
        }

        /// <summary>
        /// Gets the specific filter based on the specified key.
        /// </summary>
        /// <param name="key">Key of the dictionary.</param>
        /// <returns>The filter assigned for the specified key.</returns>
        public static Filter<PipelineContext, PipelineContext> GetSpecificFilter(SpecificFilterKey key)
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
