//-----------------------------------------------------------------------
// <copyright file="AddSecureObjectToRoleFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using Integra.Space.Models;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class AddSecureObjectToRoleFilter : CommandFilter
    {
        /// <summary>
        /// Users and roles assigned.
        /// </summary>
        private List<UserXRole> usersAndRolesAssigned;

        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            IRepository<Role> roles = context.Kernel.Get<IRepository<Role>>();
            Role role = roles.FindByName(((Language.AddCommandNode)context.Command).ToIdentifier);

            List<Tuple<string, Common.SpaceObjectEnum>> usersAndRoles = ((Language.AddCommandNode)context.Command).UsersAndRoles;
            IRepository<UserXRole> repo = context.Kernel.Get<IRepository<UserXRole>>();
            this.usersAndRolesAssigned = new List<UserXRole>();
            PermissionAssignableObject permissionAsignableObject;
            foreach (Tuple<string, Common.SpaceObjectEnum> t in usersAndRoles)
            {
                if (t.Item2 == Common.SpaceObjectEnum.User)
                {
                    permissionAsignableObject = context.Kernel.Get<IRepository<User>>().FindByName(t.Item1);
                }
                else if (t.Item2 == Common.SpaceObjectEnum.Role)
                {
                    permissionAsignableObject = context.Kernel.Get<IRepository<Role>>().FindByName(t.Item1);
                }
                else
                {
                    throw new Exception("A secure object is needed.");
                }

                UserXRole uxr = new UserXRole(role, permissionAsignableObject);

                repo.Add(uxr);
                this.usersAndRolesAssigned.Add(uxr);
            }

            // throw new System.Exception("Simulando error");
            return context;
        }

        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (this.usersAndRolesAssigned != null)
            {
                UserXRoleCacheRepository repo = (UserXRoleCacheRepository)context.Kernel.Get<IRepository<UserXRole>>();
                foreach (UserXRole uxr in this.usersAndRolesAssigned)
                {
                    repo.Delete(uxr);
                }
            }
        }
    }
}
