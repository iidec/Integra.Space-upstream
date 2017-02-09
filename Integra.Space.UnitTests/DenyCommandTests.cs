// <copyright file="DenyCommandTests.cs" company="ARITEC">
// Copyright (c) ARITEC. All rights reserved.
// </copyright>

namespace Integra.Space.UnitTests
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection.Emit;
    using Compiler;
    using Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;
    using Ninject.Planning.Bindings;
    using Pipeline;

    /// <summary>
    /// A class containing the tests for denying permissions.
    /// </summary>
    [TestClass]
    public class DenyCommandTests
    {
        /// <summary>
        /// Login to use in the tests.
        /// </summary>
        private string loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;

        #region deny

        #region deny alter

        /// <summary>
        /// Deny editing a specific database.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on database {databaseName} to user {userName}; deny alter on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific database role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    this.loginName = otherLogin;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific database role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    this.loginName = otherLogin;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific database role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName1);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName2);
                    user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific database role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);
                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin1;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);
                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin2;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role1 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName1);
                    DatabaseRole role2 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName2);
                    DatabaseUser dbUser1 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    DatabaseUser dbUser2 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser2.ServerId && x.DbUsrDatabaseId == dbUser2.DatabaseId && x.DbUsrId == dbUser2.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser2.ServerId && x.DbUsrDatabaseId == dbUser2.DatabaseId && x.DbUsrId == dbUser2.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin2;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role1 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName1);
                    DatabaseRole role2 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName2);
                    DatabaseUser dbUser1 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    DatabaseUser dbUser2 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser2.ServerId && x.DbUsrDatabaseId == dbUser2.DatabaseId && x.DbUsrId == dbUser2.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser2.ServerId && x.DbUsrDatabaseId == dbUser2.DatabaseId && x.DbUsrId == dbUser2.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    this.loginName = otherLogin2;
                    try
                    {
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            command += $"; use {databaseName}; add {userName} to {roleName}";
            string command2 = $"remove {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool existe = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName);
                    Assert.IsTrue(existe);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            command += $"; use {databaseName}; add {userName} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName}";
            string command2 = $"remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin1;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName}";
            string command2 = $"remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin2;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin1;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific databse role.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin2;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific database user.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on user {userName} to user {userName}; deny alter on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.UserAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.OnDbUsrServerId == user.ServerId && x.OnDbUsrDatabaseId == user.DatabaseId && x.OnDbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a login.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnLogin1()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter on login {existingLogin} to login {newLogin}; deny alter on login {existingLogin} to login {newLogin};
                                    use {DatabaseConstants.MASTER_DATABASE_NAME};
                                    create user bar with login = {newLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == existingLogin);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("login", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.LoginsAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.OnLoginServerId == onLogin.ServerId && x.OnLoginId == onLogin.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = newLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific schema.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter on schema {oldSchemaName} to user {existingUserName}; deny alter on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == oldSchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DatabaseId == schema.DatabaseId && x.DbUsrName == existingUserName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("schema", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SchemaAssignedPermissionsToUsers.Any(x => x.SchemaServerId == schema.ServerId && x.SchemaDatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);
                    try
                    {
                        this.loginName = loginNameAux;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific stream.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnStream()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string sourceForInto = "SourceForInto";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3  into {sourceForInto} ";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}; deny alter on stream {oldStreamName} to user {userName}";
            string command2 = $"use {databaseName}; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    try
                    {
                        kernel = new StandardKernel();
                        kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                        kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny write on a specific source.
        /// </summary>
        [TestMethod]
        public void DenyReadOnSource()
        {
            string oldStreamName = "oldStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string sourceForInto = "sourceForInto";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3 into {sourceForInto} ";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}; deny read on source {sourceNameTest} to user {userName}";

            string newEql = "inner " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:03.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t2.PrimaryAccountNumber as c1, " +
                                          $@"t1.PrimaryAccountNumber as c3 into {sourceForInto}";

            string command2 = $"use {databaseName}; alter stream {oldStreamName} with query = {newEql}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    Source source = dbContext.Sources.Single(x => x.SourceName == sourceNameTest);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("read", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    try
                    {
                        SpaceAssemblyBuilder sasmBuilder2 = new SpaceAssemblyBuilder("Test");
                        AssemblyBuilder asmBuilder2 = sasmBuilder2.CreateAssemblyBuilder();
                        SpaceModuleBuilder smodBuilder2 = new SpaceModuleBuilder(asmBuilder2);
                        smodBuilder2.CreateModuleBuilder();
                        kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder2);
                        kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                        kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny write on a specific source.
        /// </summary>
        [TestMethod]
        public void DenyWriteOnSource()
        {
            string oldStreamName = "oldStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string sourceForInto = "sourceForInto";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3 into {sourceForInto} ";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}; deny write on source {sourceForInto} to user {userName}";

            string newEql = "inner " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:03.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t2.PrimaryAccountNumber as c1, " +
                                          $@"t1.PrimaryAccountNumber as c3 into {sourceForInto}";

            string command2 = $"use {databaseName}; alter stream {oldStreamName} with query = {newEql}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    Source source = dbContext.Sources.Single(x => x.SourceName == sourceForInto);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("write", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    try
                    {
                        SpaceAssemblyBuilder sasmBuilder2 = new SpaceAssemblyBuilder("Test");
                        AssemblyBuilder asmBuilder2 = sasmBuilder2.CreateAssemblyBuilder();
                        SpaceModuleBuilder smodBuilder2 = new SpaceModuleBuilder(asmBuilder2);
                        smodBuilder2.CreateModuleBuilder();
                        kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder2);
                        kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                        kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing a specific source.
        /// </summary>
        [TestMethod]
        public void DenyAlterOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, alter on source {oldSourceName} to user {userName}; deny connect on database {databaseName}, alter on source {oldSourceName} to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny alter

        #region deny alter any

        /// <summary>
        /// Deny editing any database.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnyDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string databaseNewName = "newDatabaseName";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any database to login {otherLogin}; deny alter any database to login {otherLogin}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                        && x.LoginServerId == onLogin.ServerId && x.LoginId == onLogin.LoginId
                                                                        && x.Granted == true && x.Denied == true);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any database role.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnyDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter any role to user {userName}; deny alter any role to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any database user.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnyDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any user to user {userName}; deny alter any user to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any user", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any login.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnyLogin()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin}; deny alter any login to login {newLogin};
                                    use {DatabaseConstants.MASTER_DATABASE_NAME};
                                    create user bar with login = {newLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any login", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                            && x.LoginServerId == onLogin.ServerId && x.LoginId == onLogin.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = newLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any login.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnyLogin2()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin}; deny alter any login to login {newLogin};
                                    use {DatabaseConstants.MASTER_DATABASE_NAME};
                                    create user bar with login = {newLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any login", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                            && x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = newLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any schema.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnySchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter any schema to user {existingUserName}; deny alter any schema to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == oldSchemaName);
                    Assert.AreEqual(oldSchemaName, schema.SchemaName);

                    try
                    {
                        this.loginName = loginNameAux;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any schema and; read and write an specific source.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnySchemaAndReadSource()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string sourceForInto = "sourceForInto";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3 into {sourceForInto}";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter any schema, read on source {sourceNameTest} to user {userName}; deny alter any schema, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}";
            string command2 = $"use {databaseName}; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny editing any schema.
        /// </summary>
        [TestMethod]
        public void DenyAlterAnySchema2()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, alter any schema to user {userName}; deny connect on database {databaseName}, alter any schema to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny alter any

        #region deny control

        /// <summary>
        /// Deny control on a specific database.
        /// </summary>
        [TestMethod]
        public void DenyControlOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on database {databaseName} to user {userName}; deny control on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific database role.
        /// </summary>
        [TestMethod]
        public void DenyControlOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant control on role {roleName} to user {userName}; deny control on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("databaserole", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific database user.
        /// </summary>
        [TestMethod]
        public void DenyControlOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on user {userName} to user {userName}; deny control on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("databaseuser", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.UserAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.OnDbUsrServerId == user.ServerId && x.OnDbUsrDatabaseId == user.DatabaseId && x.OnDbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific login.
        /// </summary>
        [TestMethod]
        public void DenyControlOnLogin()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant control on login {existingLogin} to login {newLogin}; deny control on login {existingLogin} to login {newLogin};
                                    use {DatabaseConstants.MASTER_DATABASE_NAME};
                                    create user bar with login = {newLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == existingLogin);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("login", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.LoginsAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.OnLoginServerId == onLogin.ServerId && x.OnLoginId == onLogin.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Login login = dbContext.Logins.Single(x => x.LoginName == existingLogin);
                    Assert.AreEqual(existingLogin, login.LoginName);

                    try
                    {
                        this.loginName = newLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific schema.
        /// </summary>
        [TestMethod]
        public void DenyControlOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant control on schema {oldSchemaName} to user {existingUserName}; deny control on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.Database.DatabaseName == databaseName && x.SchemaName == oldSchemaName);
                    Assert.AreEqual(oldSchemaName, schema.SchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == existingUserName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("schema", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SchemaAssignedPermissionsToUsers.Any(x => x.SchemaServerId == schema.ServerId && x.SchemaDatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = loginNameAux;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific stream and; read and write a specific source.
        /// </summary>
        [TestMethod]
        public void DenyControlOnStreamAndReadSource()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string sourceForInto = "sourceForInto";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3 into {sourceForInto} ";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant control on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}; deny control on stream {oldStreamName} to user {userName}";
            string command2 = $"use {databaseName}; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny control on a specific source.
        /// </summary>
        [TestMethod]
        public void DenyControlOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"use {databaseName}; create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, control on source {oldSourceName} to user {userName}; deny control on source {oldSourceName} to user {userName}";
            string command2 = $"use {databaseName}; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny control

        #region deny take ownership

        /// <summary>
        /// Deny take ownership on a specific datbase role.
        /// </summary>
        [TestMethod]
        public void DenyTakeOwnershipOnDbRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant take ownership on role {roleName} to user {userName}; deny take ownership on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on role {roleName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.DatabaseName == databaseName);
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.DbRoleName == roleName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, role.DatabaseUser.DbUsrName);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));

                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny take ownership on a specific database.
        /// </summary>
        [TestMethod]
        public void DenyTakeOwnershipOnDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant take ownership on database {databaseName} to user {userName}; deny take ownership on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on database {databaseName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Assert.AreEqual<string>(DatabaseConstants.SA_LOGIN_NAME, database.Login.LoginName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny take ownership on a specific schema.
        /// </summary>
        [TestMethod]
        public void DenyTakeOwnershipOnSchema()
        {
            string schemaName = "oldSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {schemaName}; grant connect on database {databaseName} to user {existingUserName}; grant take ownership on schema {schemaName} to user {existingUserName}; deny take ownership on schema {schemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; take ownership on schema {schemaName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DatabaseId == database.DatabaseId && x.DbUsrName == existingUserName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("schema", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SchemaAssignedPermissionsToUsers.Any(x => x.SchemaServerId == schema.ServerId && x.SchemaDatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny take ownership on a specific source.
        /// </summary>
        [TestMethod]
        public void DenyTakeOwnershipOnSource()
        {
            string oldSourceName = "oldSourceName";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, take ownership on source {oldSourceName} to user {userName}; deny take ownership on source {oldSourceName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on source {oldSourceName}";
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    SpaceAssemblyBuilder sasmBuilder = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder = sasmBuilder.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder = new SpaceModuleBuilder(asmBuilder);
                    smodBuilder.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Source source = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == oldSourceName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, source.DatabaseUser.DbUsrName);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DatabaseId == database.DatabaseId && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny take ownership on a specific stream.
        /// </summary>
        [TestMethod]
        public void DenyTakeOwnershipOnStream()
        {
            string oldStreamName = "oldStream";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string sourceNameTest = "source1234";
            string sourceForInto = "sourceForInto";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.MessageType == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.MessageType == ""0110""" +
                                  $@"ON (string)t1.PrimaryAccountNumber == (string)t2.PrimaryAccountNumber and (string)t1.RetrievalReferenceNumber == (string)t2.RetrievalReferenceNumber " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.SourceTimestamp, '01/01/2017') - isnull(t1.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.PrimaryAccountNumber as c1, " +
                                          $@"t2.PrimaryAccountNumber as c3 into {sourceForInto} ";

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant take ownership on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}; deny take ownership on stream {oldStreamName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on stream {oldStreamName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    SpaceAssemblyBuilder sasmBuilder1 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder1 = sasmBuilder1.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder1 = new SpaceModuleBuilder(asmBuilder1);
                    smodBuilder1.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder1);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == DatabaseConstants.DBO_SCHEMA_NAME);
                    Stream stream = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == oldStreamName);
                    Assert.AreEqual<string>(DatabaseConstants.DBO_USER_NAME, stream.DatabaseUser.DbUsrName);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DatabaseId == database.DatabaseId && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    try
                    {
                        this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny take ownership

        #region deny view any definition

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromServerRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromEndpoints()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromLogins()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromDatabases()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromUsers()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromDatabaseRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromSchemas()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromSources()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromStreams()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any definition.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDefinitionAndFromViews()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny view any definition

        #region deny view any database

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromUsers()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromDatabaseRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromSchemas()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromSources()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromStreams()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view any database.
        /// </summary>
        [TestMethod]
        public void DenyViewAnyDatabaseAndFromViews()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny view any database

        #region deny view definition on

        /// <summary>
        /// Deny view definition on a specific endpoint.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromEndpoints()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string endpointName = DatabaseConstants.TCP_ENDPOINT_NAME;
            string command = $"grant view definition on endpoint {endpointName} to login {otherLogin}; deny view definition on endpoint {endpointName} to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Endpoint endpoint = dbContext.Endpoints.Single(x => x.EnpointName == endpointName);
                    Login login = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.EndpointsAssignedPermissionsToLogins.Any(x => x.EndpointServerId == endpoint.ServerId && x.EndpointId == endpoint.EndpointId
                                                                            && x.LoginServerId == login.ServerId && x.LoginId == login.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific login.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromLogins()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string loginNameOn = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"grant view definition on login {loginNameOn} to login {otherLogin}; deny view definition on login {loginNameOn} to login {otherLogin}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login loginTo = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Login loginOn = dbContext.Logins.Single(x => x.LoginName == loginNameOn);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("login", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.LoginsAssignedPermissionsToLogins.Any(x => x.LoginServerId == loginTo.ServerId && x.LoginId == loginTo.LoginId
                                                                            && x.OnLoginServerId == loginOn.ServerId && x.OnLoginId == loginOn.LoginId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific database.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromDatabases()
        {
            string databaseName = DatabaseConstants.TEST_DATABASE_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant view definition on database {databaseName} to user {userName}; deny view definition on database {databaseName} to user {userName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific users.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromUsers()
        {
            string userNameTo = DatabaseConstants.NORMAL_USER_1_NAME;
            string userNameOn = DatabaseConstants.NORMAL_USER_2_NAME;
            string command = $"grant view definition on user {userNameOn} to user {userNameTo}; deny view definition on user {userNameOn} to user {userNameTo}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser userTo = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbUsrName == userNameTo);
                    DatabaseUser userOn = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbUsrName == userNameOn);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("databaseuser", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.UserAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == userTo.ServerId && x.DbUsrDatabaseId == userTo.DatabaseId && x.DbUsrId == userTo.DbUsrId
                                                                            && x.OnDbUsrServerId == userOn.ServerId && x.OnDbUsrDatabaseId == userOn.DatabaseId && x.OnDbUsrId == userOn.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific database roles.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromDatabaseRoles()
        {
            string roleName = DatabaseConstants.ROLE_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant view definition on role {roleName} to user {userName}; deny view definition on role {roleName} to user {userName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("databaserole", StringComparison.InvariantCultureIgnoreCase));

                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific schema.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromSchemas()
        {
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"use {databaseName}; grant view definition on schema {schemaName} to user {userName}; deny view definition on schema {schemaName} to user {userName}";
            string command2 = $"use {databaseName}; from sys.schemas select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.Database.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.Database.DatabaseName == databaseName && x.SchemaName == schemaName);
                    Assert.AreEqual(schemaName, schema.SchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("schema", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SchemaAssignedPermissionsToUsers.Any(x => x.SchemaServerId == schema.ServerId && x.SchemaDatabaseId == schema.DatabaseId && x.SchemaId == schema.SchemaId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific source.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromSources()
        {
            string sourceName = DatabaseConstants.INPUT_SOURCE_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"use {databaseName}; grant view definition on source {sourceName} to user {userName}; deny view definition on source {sourceName} to user {userName}";
            string command2 = $"use {databaseName}; from sys.sources select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                    Assert.AreEqual(sourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny view definition on a specific stream.
        /// </summary>
        [TestMethod]
        public void DenyViewOnDefinitionAndFromStreams()
        {
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string streamName = DatabaseConstants.TEST_STREAM_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant view definition on stream {DatabaseConstants.DBO_SCHEMA_NAME}.{streamName} to user {DatabaseConstants.TEST_DATABASE_NAME}.{userName}; deny view definition on stream {DatabaseConstants.DBO_SCHEMA_NAME}.{streamName} to user {DatabaseConstants.TEST_DATABASE_NAME}.{userName}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.streams select ServerId as serverId into {DatabaseConstants.DBO_SCHEMA_NAME}.{DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                    Assert.AreEqual(streamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName && x.Database.DatabaseName == DatabaseConstants.TEST_DATABASE_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny view definition on

        #region deny create

        #region deny create any database

        /// <summary>
        /// Deny create any database.
        /// </summary>
        [TestMethod]
        public void DenyCreateAnyDatabase()
        {
            string databaseName = "newDatabase";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create any database.
        /// </summary>
        [TestMethod]
        public void DenyCreateAnyDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create any database.
        /// </summary>
        [TestMethod]
        public void DenyCreateAnyDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create any database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                            && x.ServerId == server.ServerId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create any database

        #region deny create database

        /// <summary>
        /// Deny create database.
        /// </summary>
        [TestMethod]
        public void DenyCreateDatabase()
        {
            string databaseName = "newDatabase";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {userName}; deny create database to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database.
        /// </summary>
        [TestMethod]
        public void DenyCreateDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {userName}; deny create database to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database.
        /// </summary>
        [TestMethod]
        public void DenyCreateDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {userName}; deny create database to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create database", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create database

        #region deny create role

        /// <summary>
        /// Deny create database role.
        /// </summary>
        [TestMethod]
        public void DenyCreateRole()
        {
            string roleName = "role1";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database role.
        /// </summary>
        [TestMethod]
        public void DenyCreateRoleWithStatusOn()
        {
            string roleName = "role1";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database role.
        /// </summary>
        [TestMethod]
        public void DenyCreateRoleWithStatusOff()
        {
            string roleName = "role1";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database role.
        /// </summary>
        [TestMethod]
        public void DenyCreateRoleAddUser()
        {
            string roleName = "role1";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"Create role {roleName} with add = {userName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create database role.
        /// </summary>
        [TestMethod]
        public void DenyCreateRoleAddUsers()
        {
            string roleName = "role1";
            string userName1 = DatabaseConstants.ADMIN_LOGIN_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string userName3 = DatabaseConstants.NORMAL_USER_3_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"Create role {roleName} with add = {userName1} {userName2} {userName3}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create role", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create role

        #region deny create schema

        /// <summary>
        /// Deny create schema.
        /// </summary>
        [TestMethod]
        public void DenyCreateSchema()
        {
            string schemaName = "newSchema";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create schema to user {userName}; deny create schema to user {userName}";
            string command2 = $"create schema {schemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create schema", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create schema

        #region deny create source

        /// <summary>
        /// Deny create source.
        /// </summary>
        [TestMethod]
        public void DenyCreateSource()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create source", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create source.
        /// </summary>
        [TestMethod]
        public void DenyCreateSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create source", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create source.
        /// </summary>
        [TestMethod]
        public void DenyCreateSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create source", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create source

        #region deny create stream

        /// <summary>
        /// Deny create stream.
        /// </summary>
        [TestMethod]
        public void DenyCreateStream()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant read on source {DatabaseConstants.INPUT_SOURCE_NAME}, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create stream", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create stream.
        /// </summary>
        [TestMethod]
        public void DenyReadSource()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string sourceName = DatabaseConstants.INPUT_SOURCE_NAME;
            string command = $"grant read on source {sourceName}, create stream to user {userName}; deny read on source {sourceName} to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {sourceName} as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   $"WITH {sourceName} as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);
                    Source source = dbContext.Sources.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.Schema.SchemaName == DatabaseConstants.DBO_SCHEMA_NAME && x.SourceName == sourceName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("read", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("source", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.SourceAssignedPermissionsToUsers.Any(x => x.SourceServerId == source.ServerId && x.SourceDatabaseId == source.DatabaseId && x.SourceSchemaId == source.SchemaId && x.SourceId == source.SourceId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUserDatabaseId == user.DatabaseId && x.DbUserId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create stream.
        /// </summary>
        [TestMethod]
        public void DenyCreateStreamWithStatusOn()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant read on source {DatabaseConstants.INPUT_SOURCE_NAME}, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create stream", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// Deny create stream.
        /// </summary>
        [TestMethod]
        public void DenyCreateStreamWithStatusOff()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string command = $"grant read on source {DatabaseConstants.INPUT_SOURCE_NAME}, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }} with status = off";
            this.loginName = DatabaseConstants.SA_LOGIN_NAME;

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == DatabaseConstants.MASTER_DATABASE_NAME);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("create stream", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DbUsrDatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted && x.Denied);

                    try
                    {
                        Assert.IsTrue(exists);
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        #endregion deny create stream

        #endregion deny create

        #endregion deny

        /// <summary>
        /// This method create a pipeline context and execute the specified command.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <param name="kernel">DI kernel.</param>
        /// <returns>Pipeline context.</returns>
        private FirstLevelPipelineContext ProcessCommand(string command, IKernel kernel)
        {
            IBinding binding = kernel.GetBindings(typeof(Language.IGrammarRuleValidator)).FirstOrDefault();
            if (binding != null)
            {
                kernel.RemoveBinding(binding);
            }

            kernel.Bind<Language.IGrammarRuleValidator>().ToConstant(new TestRuleValidator());
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> pipeline = cpb.Build();

            FirstLevelPipelineExecutor cpe = new FirstLevelPipelineExecutor(pipeline);
            FirstLevelPipelineContext context = new FirstLevelPipelineContext(command, this.loginName, kernel);
            FirstLevelPipelineContext result = cpe.Execute(context);
            return result;
        }
    }
}
