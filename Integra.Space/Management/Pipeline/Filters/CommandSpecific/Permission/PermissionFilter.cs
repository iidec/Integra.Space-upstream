//-----------------------------------------------------------------------
// <copyright file="PermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Integra.Space.Language;
    using Integra.Space.Models;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    /// <typeparam name="TPermission">Permission type.</typeparam>
    internal abstract class PermissionFilter<TPermission> : CommandFilter where TPermission : PermissionAssigned
    {
        /// <summary>
        /// List of permission with the value before the modification.
        /// </summary>
        private List<Tuple<TPermission, bool>> oldPermissions;

        /// <summary>
        /// Gets the old permission list.
        /// </summary>
        protected List<Tuple<TPermission, bool>> OldPermissions
        {
            get
            {
                return this.oldPermissions;
            }
        }

        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            PermissionCacheRepository<TPermission> pr = context.Kernel.Get<PermissionCacheRepository<TPermission>>();
            this.oldPermissions = new List<Tuple<TPermission, bool>>();
            PermissionAssigned actualPermission = null;
            IEnumerable<PermissionAssigned> listOfPermissions = this.GetPermissionsToAssing(context).Where(x => x is TPermission);
            foreach (TPermission p in listOfPermissions)
            {
                actualPermission = pr.GetPermission(p);

                if (actualPermission != null)
                {
                    this.oldPermissions.Add(Tuple.Create(p, false));
                }
                else
                {
                    this.oldPermissions.Add(Tuple.Create(p, true));
                }

                this.ExecutePermissionAction(pr, p);
            }

            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext context)
        {
            if (this.OldPermissions != null)
            {
                PermissionCacheRepository<TPermission> pr = context.Kernel.Get<PermissionCacheRepository<TPermission>>();
                foreach (Tuple<TPermission, bool> p in this.OldPermissions)
                {
                    if (p.Item2)
                    {
                        pr.Revoke(p.Item1);
                    }
                    else
                    {
                        this.ExecuteReverse(pr, p.Item1);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the permission to assign from the command.
        /// </summary>
        /// <param name="context">Pipeline context.</param>
        /// <returns>The list of permission specified in the command.</returns>
        public List<PermissionAssigned> GetPermissionsToAssing(PipelineContext context)
        {
            SpacePermissionsCommandNode permissionCommand = (SpacePermissionsCommandNode)context.Command;
            Schema schema = this.GetSchema(context);

            Principal principal = null;
            if (permissionCommand.SpaceObjectType == SystemObjectEnum.User)
            {
                IRepository<User> repo = context.Kernel.Get<IRepository<User>>();
                principal = repo.FindByName(permissionCommand.ToIdentifier);
            }
            else if (permissionCommand.SpaceObjectType == SystemObjectEnum.Role)
            {
                IRepository<Role> repo = context.Kernel.Get<IRepository<Role>>();
                principal = repo.FindByName(permissionCommand.ToIdentifier);
            }
            else
            {
                throw new Exception("User o role required to assign permissions.");
            }

            List<PermissionAssigned> listOfPermissions = new List<PermissionAssigned>();

            IEnumerable<IGrouping<dynamic, PermissionOverObjectType>> g1 = permissionCommand.Permissions
                .Where(x => x.ObjectName == null)
                .Select(x =>
                {
                    if (permissionCommand.Action == ActionCommandEnum.Deny)
                    {
                        return new PermissionOverObjectType(principal, x.ObjectType, 0, (int)x.Permission, schema);
                    }
                    else
                    {
                        return new PermissionOverObjectType(principal, x.ObjectType, (int)x.Permission, 0, schema);
                    }
                })
                .GroupBy(x => new { ObjectType = x.SpaceObjectType, SchemaName = x.Schema.Name });

            foreach (IGrouping<dynamic, PermissionOverObjectType> g in g1)
            {
                int permissionValue = 0;

                if (permissionCommand.Action == ActionCommandEnum.Deny)
                {
                    g.Select(x => x.DenyValue).Cast<int>().ToList().ForEach(x => permissionValue = permissionValue | x);
                    listOfPermissions.Add(new PermissionOverObjectType(principal, g.Key.ObjectType, 0, permissionValue, schema));
                }
                else
                {
                    g.Select(x => x.GrantValue).Cast<int>().ToList().ForEach(x => permissionValue = permissionValue | x);
                    listOfPermissions.Add(new PermissionOverObjectType(principal, g.Key.ObjectType, permissionValue, 0, schema));
                }
            }

            IEnumerable<IGrouping<SystemObject, PermissionOverSpecificObject>> g2 = permissionCommand.Permissions
                .Where(x => x.ObjectName != null)
                .Select(x =>
                {
                    MethodInfo method2 = typeof(ResolutionExtensions).GetMethods()
                    .First(m => m.Name == "Get" && m.GetParameters()[1].ParameterType.Equals(typeof(Ninject.Parameters.IParameter).MakeArrayType()))
                    .MakeGenericMethod(new Type[] { typeof(IRepository<>).MakeGenericType(Type.GetType("Integra.Space.Models." + x.ObjectType.ToString())) });

                    var repo = method2.Invoke(null, new object[] { context.Kernel, new Ninject.Parameters.IParameter[] { } });
                    SystemObject systemObject = (SystemObject)repo.GetType().GetMethod("FindByName").Invoke(repo, new object[] { x.ObjectName });

                    if (permissionCommand.Action == ActionCommandEnum.Deny)
                    {
                        return new PermissionOverSpecificObject(principal, systemObject, 0, (int)x.Permission);
                    }
                    else
                    {
                        return new PermissionOverSpecificObject(principal, systemObject, (int)x.Permission, 0);
                    }
                })
                .GroupBy(x => x.SpaceObject, new SystemObjectComparer());

            foreach (IGrouping<SystemObject, PermissionOverSpecificObject> g in g2)
            {                
                PermissionOverSpecificObject permission = g.First();
                int permissionValue = 0;

                if (permissionCommand.Action == ActionCommandEnum.Deny)
                {
                    g.Where(x => x is PermissionOverSpecificObject).Select(x => x.DenyValue).Cast<int>().ToList().ForEach(x => permissionValue = permissionValue | x);
                    listOfPermissions.Add(new PermissionOverSpecificObject(permission.Principal, g.Key, 0, permissionValue));
                }
                else
                {
                    g.Where(x => x is PermissionOverSpecificObject).Select(x => x.GrantValue).Cast<int>().ToList().ForEach(x => permissionValue = permissionValue | x);
                    listOfPermissions.Add(new PermissionOverSpecificObject(permission.Principal, g.Key, permissionValue, 0));
                }
            }

            return listOfPermissions;
        }

        /// <summary>
        /// Execute the reverse operation of the command.
        /// </summary>
        /// <param name="repo">Permission repository.</param>
        /// <param name="permission">Permission to reverse.</param>
        protected abstract void ExecuteReverse(PermissionCacheRepository<TPermission> repo, TPermission permission);

        /// <summary>
        /// Execute the operation to the command.
        /// </summary>
        /// <param name="repo">Permission repository.</param>
        /// <param name="permission">Permission to reverse.</param>
        protected abstract void ExecutePermissionAction(PermissionCacheRepository<TPermission> repo, TPermission permission);
    }
}
