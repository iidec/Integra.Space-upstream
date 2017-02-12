//-----------------------------------------------------------------------
// <copyright file="CreateLoginFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Language;

    /// <summary>
    /// Filter create source class.
    /// </summary>
    internal class CreateLoginFilter : CreateEntityFilter<CreateLoginNode, Common.LoginOptionEnum>
    {
        /// <inheritdoc />
        protected override void CreateEntity(CreateLoginNode command, Dictionary<Common.LoginOptionEnum, object> options, Login login, DatabaseUser user, Schema schema, SpaceDbContext databaseContext)
        {
            Login newLogin = new Login();
            newLogin.ServerId = schema.ServerId;
            newLogin.LoginId = Guid.NewGuid();
            newLogin.LoginName = command.MainCommandObject.Name;
            newLogin.LoginPassword = options[Common.LoginOptionEnum.Password].ToString();

            // se le establece la base de datos por defecto
            if (command.Options.ContainsKey(Common.LoginOptionEnum.Default_Database))
            {
                string databaseName = options[Common.LoginOptionEnum.Default_Database].ToString();
                Database defaultDb = databaseContext.Databases.Single(x => x.ServerId == schema.ServerId && x.DatabaseName == databaseName);
                newLogin.DefaultDatabaseServerId = defaultDb.ServerId;
                newLogin.DefaultDatabaseId = defaultDb.DatabaseId;
            }
            else
            {
                newLogin.DefaultDatabaseServerId = schema.ServerId;
                newLogin.DefaultDatabaseId = schema.DatabaseId;
            }

            newLogin.IsActive = true;
            if (command.Options.ContainsKey(Common.LoginOptionEnum.Status))
            {
                newLogin.IsActive = (bool)command.Options[Common.LoginOptionEnum.Status];
            }

            // almaceno la nueva entidad y guardo los cambios
            databaseContext.Logins.Add(newLogin);
            databaseContext.SaveChanges();

            // creo el permiso de conexión 'connect sql' para el login
            ServerAssignedPermissionsToLogin newPermission = new ServerAssignedPermissionsToLogin();
            newPermission.Login = newLogin;
            newPermission.SecurableClassId = databaseContext.SecurableClasses.Single(x => x.SecurableName.ToLower() == Common.SystemObjectEnum.Server.ToString().ToLower()).SecurableClassId;
            newPermission.GranularPermissionId = databaseContext.GranularPermissions.Single(x => x.GranularPermissionName.Replace(" ", string.Empty).ToLower() == Common.PermissionsEnum.ConnectSQL.ToString().ToLower()).GranularPermissionId;
            newPermission.WithGrantOption = false;
            newPermission.ServerId = schema.ServerId;
            newPermission.Granted = true;

            databaseContext.ServersAssignedPermissionsToLogins.Add(newPermission);

            databaseContext.SaveChanges();
        }
    }
}
