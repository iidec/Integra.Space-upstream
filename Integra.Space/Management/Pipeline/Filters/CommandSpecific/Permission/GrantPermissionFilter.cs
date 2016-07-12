//-----------------------------------------------------------------------
// <copyright file="GrantPermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Integra.Space.Language;
    using Integra.Space.Models;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class GrantPermissionFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            SpacePermissionsCommandNode permissionCommand = (SpacePermissionsCommandNode)context.Command;

            PermissionAssignableObject pao = null;
            if (permissionCommand.SpaceObjectType == SpaceObjectEnum.User)
            {
                IRepository<User> repo = context.Kernel.Get<IRepository<User>>();
                pao = repo.FindByName(permissionCommand.ToIdentifier);
            }
            else if (permissionCommand.SpaceObjectType == SpaceObjectEnum.Role)
            {
                IRepository<Role> repo = context.Kernel.Get<IRepository<Role>>();
                pao = repo.FindByName(permissionCommand.ToIdentifier);
            }
            else
            {
                throw new System.Exception("User o role required to assign permissions.");
            }

            List<Permission> listOfPermissions = new List<Permission>();
            IEnumerable<IGrouping<SpaceObjectEnum, SpacePermission>> pg = permissionCommand.Permissions.GroupBy(x => x.ObjectType);

            foreach (IGrouping<SpaceObjectEnum, SpacePermission> g in pg)
            {
                if (g.Key == SpaceObjectEnum.Source || g.Key == SpaceObjectEnum.Stream)
                {
                    int permissionValue = g.Select(x => x.Permission).Where(x => x != SpacePermissionsEnum.Read).Cast<int>().Count();
                    listOfPermissions.Add(new Permission(pao, permissionCommand.SpaceObjectType, permissionValue));

                    g.Where(x => x.Permission == SpacePermissionsEnum.Read).ToList().ForEach(x =>
                    {
                        if (x.ObjectType == SpaceObjectEnum.Source)
                        {
                            IRepository<Source> sources = context.Kernel.Get<IRepository<Source>>();
                            Source source = sources.FindByName(x.ObjectName);
                            if (source != null)
                            {
                                listOfPermissions.Add(new Permission(pao, permissionCommand.SpaceObjectType, permissionValue, source));
                            }
                            else
                            {
                                throw new System.Exception(string.Format("The source '{0}' does not exist.", x.ObjectName));
                            }
                        }
                        else if (x.ObjectType == SpaceObjectEnum.Stream)
                        {
                            IRepository<Stream> sources = context.Kernel.Get<IRepository<Stream>>();
                            Stream stream = sources.FindByName(x.ObjectName);
                            if (stream != null)
                            {
                                listOfPermissions.Add(new Permission(pao, permissionCommand.SpaceObjectType, permissionValue, stream));
                            }
                            else
                            {
                                throw new System.Exception(string.Format("The stream '{0}' does not exist.", x.ObjectName));
                            }
                        }
                    });
                }
                else
                {
                    int permissionValue = g.Select(x => x.Permission).Cast<int>().Count();
                    listOfPermissions.Add(new Permission(pao, permissionCommand.SpaceObjectType, permissionValue));
                }
            }

            PermissionCacheRepository pr = (PermissionCacheRepository)context.Kernel.Get<IRepository<Permission>>();
            foreach (Permission p in listOfPermissions)
            {
                pr.Grant(p);
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (!this.Executed)
            {
                return;
            }
        }
    }
}
