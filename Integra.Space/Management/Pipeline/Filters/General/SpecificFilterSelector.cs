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
        private static Dictionary<SpecificFilterKey, Filter<PipelineContext, PipelineContext>> filterDictionary;

        /// <summary>
        /// Dictionary of filters for permissions.
        /// </summary>
        private static Dictionary<SystemObjectEnum, Filter<PipelineContext, PipelineContext>> filterDictionaryForPermissions;
        
        /// <summary>
        /// Initializes static members of the <see cref="SpecificFilterSelector"/> class.
        /// </summary>
        static SpecificFilterSelector()
        {
            filterDictionary = new Dictionary<SpecificFilterKey, Filter<PipelineContext, PipelineContext>>(new Comparer());
            filterDictionaryForPermissions = new Dictionary<SystemObjectEnum, Filter<PipelineContext, PipelineContext>>();

            // add the specific filters.
            // create
            // level 1
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Source), new CreateSourceFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Stream), new ParseQueryForCreateStreamFilter().AddStep(new CreateStreamFilter()));
            /* filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.View), new CreateViewFilter() /* .AddStep(new FilterQueryParser()) ); */

            // level 2
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.DatabaseUser), new CreateUserFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.DatabaseRole), new CreateRoleFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Schema), new CreateSchemaFilter());

            // level 3
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Database), new CreateDatabaseFilter());
            /* filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Endpoint), new CreateEndpointFilter()); */
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Login), new CreateLoginFilter());

            // level 4
            /* filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Create, SystemObjectEnum.Server), new CreateServerFilter()); */

            // alter
            // level 1
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Stream), new ParseQueryForAlterStreamFilter().AddStep(new AlterStreamFilter()));
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Source), new AlterSourceFilter());
            /* filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.View), new AlterViewFilter() /* .AddStep(new FilterQueryParser()) );*/

            // level 2
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.DatabaseUser), new AlterUserFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.DatabaseRole), new AlterRoleFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Schema), new AlterSchemaFilter());

            // level 3
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Login), new AlterLoginFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Alter, SystemObjectEnum.Database), new AlterDatabaseFilter());

            // drop
            // level 1
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Source), new DropSourceFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Stream), new DropStreamFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.View), new DropViewFilter());

            // level 2
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.DatabaseRole), new DropDatabaseRoleFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.DatabaseUser), new DropDatabaseUserFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Schema), new DropSchemaFilter());

            // level 3
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Database), new DropDatabaseFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Endpoint), new DropEndpointFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Login), new DropLoginFilter());

            // level 4
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Drop, SystemObjectEnum.Server), new DropServerFilter());

            // permission
            filterDictionaryForPermissions.Add(SystemObjectEnum.Database, new PermissionOnDatabaseFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.DatabaseRole, new PermissionOnDBRoleFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Endpoint, new PermissionOnEndpointFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Login, new PermissionOnLoginFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Schema, new PermissionOnSchemaFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Server, new PermissionOnServerFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Source, new PermissionOnSourceFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.Stream, new PermissionOnStreamFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.DatabaseUser, new PermissionOnUserFilter());
            filterDictionaryForPermissions.Add(SystemObjectEnum.View, new PermissionOnViewFilter());

            // take ownership
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.DatabaseRole), new TakeOwnershipOfDbRoleFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.Database), new TakeOwnershipOfDatabaseFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.Endpoint), new TakeOwnershipOfEndpointFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.Schema), new TakeOwnershipOfSchemaFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.Source), new TakeOwnershipOfSourceFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.Stream), new TakeOwnershipOfStreamFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.TakeOwnership, SystemObjectEnum.View), new TakeOwnershipOfViewFilter());

            // view definition
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Database), new DatabaseMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.DatabaseRole), new DatabaseRoleMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.DatabaseUser), new UserMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Endpoint), new EndpointMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Login), new LoginMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Schema), new SchemaMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Server), new ServerMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.ServerRole), new ServerRoleMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Source), new SourceMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.Stream), new StreamMetadataQueryFilter());
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.ViewDefinition, SystemObjectEnum.View), new ViewMetadataQueryFilter());

            // truncate source
            filterDictionary.Add(new SpecificFilterKey(ActionCommandEnum.Truncate, SystemObjectEnum.Source), new TruncateFilter());
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
        /// Gets the specific filter based on the specified key.
        /// </summary>
        /// <param name="key">Key of the dictionary.</param>
        /// <returns>The filter assigned for the specified key.</returns>
        public static Filter<PipelineContext, PipelineContext> GetSpecificFilter(SystemObjectEnum key)
        {
            if (filterDictionaryForPermissions.ContainsKey(key))
            {
                return filterDictionaryForPermissions[key];
            }
            else
            {
                throw new System.Exception(string.Format("Not implement command. Command: '{0}'", key));
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
