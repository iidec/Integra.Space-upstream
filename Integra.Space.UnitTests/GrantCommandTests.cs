﻿// <copyright file="GrantCommandTests.cs" company="ARITEC">
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
    /// A class containing the tests for grant permissions.
    /// </summary>
    [TestClass]
    public class GrantCommandTests
    {
        private string loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;

        #region grant

        #region grant alter

        /// <summary>
        /// Grants alter permission on a specific database to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseNewName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseNewName && x.DbUsrName == userName);

                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == newRoleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.DatabaseUser dbUser = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple roles to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser1.DbUsrName);
                        Space.Database.DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName);
                        Assert.AreEqual(userName, dbUser2.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to multiple users.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin1;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser2.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to multiple users.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin2;
                    FirstLevelPipelineContext result3 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser2.DbUsrName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple database roles to multiple users.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin1;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser2.DbUsrName);

                        DatabaseUser dbUser3 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser3.DbUsrName);
                        DatabaseUser dbUser4 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser4.DbUsrName);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple database roles to multiple users.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleAddUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin2;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser dbUser1 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser1.DbUsrName);
                        DatabaseUser dbUser2 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, dbUser2.DbUsrName);

                        DatabaseUser dbUser3 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser3.DbUsrName);
                        DatabaseUser dbUser4 = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Single(x => x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, dbUser4.DbUsrName);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName}";
            command2 += $"; remove {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool existe = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        Assert.IsFalse(existe);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple database role to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName1}, {roleName2}";
            command2 += $"; remove {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to multiple user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";
            command2 += $"; remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin1;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on a specific database role to multiple user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";
            command2 += $"; remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin2;
                    FirstLevelPipelineContext result3 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple database roles to multiple user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            command2 += $"; remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin1;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on multiple database roles to multiple user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseRoleRemoveUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string otherLogin2 = DatabaseConstants.NORMAL_LOGIN_2_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            command2 += $"; remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin2;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        bool exists = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName1);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName1).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        exists = exists || dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName2).DatabaseUsers.Any(x => x.DbUsrName == userName2);
                        Assert.IsFalse(exists);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Usuario '{userName1}' o '{userName2}' no fue agregado al rol '{roleName1}' o '{roleName2}'");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on specific database to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == newUserName);

                        bool exists = dbContext.UserAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.OnDbUsrServerId == user.ServerId && x.OnDbUsrDatabaseId == user.DatabaseId && x.OnDbUsrId == user.DbUsrId
                                                                            && x.Granted);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on specific login to login.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnLogin1()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter on login {existingLogin} to login {newLogin};
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

                    this.loginName = newLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);
                    try
                    {
                        Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                        Login onLogin = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                        SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("login", StringComparison.InvariantCultureIgnoreCase));
                        bool exists = dbContext.LoginsAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                                && x.OnLoginServerId == onLogin.ServerId && x.OnLoginId == onLogin.LoginId
                                                                                && x.GranularPermissionId == gp.GranularPermissionId
                                                                                && x.SecurableClassId == sc.SecurableClassId
                                                                                && x.Granted == true);
                        Assert.IsTrue(exists);

                        Login login = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        Assert.AreEqual(newLoginName, login.LoginName);

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al conceder permisos al login '{newLogin}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on specific schema to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = loginNameAux;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == newSchemaName);
                        Assert.AreEqual(newSchemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSchemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on specific stream to user and; read and write on specific source.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnStreamAndReadSource()
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

            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create source {sourceForInto} (c1 string(4000), c3 string(4000)); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter stream {oldStreamName} with name = {newStreamName}";

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

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == newStreamName);
                        Assert.AreEqual(newStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter permission on specific source to user.
        /// </summary>
        [TestMethod]
        public void GrantAlterOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, alter on source {oldSourceName} to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter source {oldSourceName} with name = {newSourceName}";

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
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    SpaceAssemblyBuilder sasmBuilder2 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder2 = sasmBuilder2.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder2 = new SpaceModuleBuilder(asmBuilder2);
                    smodBuilder2.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder2);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == newSourceName);
                        Assert.AreEqual(newSourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant alter

        #region grant alter any

        /// <summary>
        /// Grants alter any database permission.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnyDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string databaseNewName = "newDatabaseName";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any database to login {otherLogin}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseNewName && x.DbUsrName == userName);

                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                        && x.LoginServerId == onLogin.ServerId && x.LoginId == onLogin.LoginId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants alter any database role permission.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnyDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter any role to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants alter any user.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnyDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any user to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == newUserName);
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any user", StringComparison.InvariantCultureIgnoreCase));
                        SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("database", StringComparison.InvariantCultureIgnoreCase));
                        bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.GranularPermissionId == gp.GranularPermissionId
                                                                        && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted);
                        Assert.IsTrue(exists);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter any login.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnyLogin()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin};
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

                    this.loginName = newLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);
                    try
                    {
                        Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                        Login onLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                        GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any login", StringComparison.InvariantCultureIgnoreCase));
                        SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                        bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                                && x.LoginServerId == onLogin.ServerId && x.LoginId == onLogin.LoginId
                                                                                && x.GranularPermissionId == gp.GranularPermissionId
                                                                                && x.SecurableClassId == sc.SecurableClassId
                                                                                && x.Granted == true);
                        Assert.IsTrue(exists);

                        Login login = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        Assert.AreEqual(newLoginName, login.LoginName);

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al conceder permisos al login '{newLogin}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter any login.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnyLogin2()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin};
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

                    this.loginName = newLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);
                    try
                    {
                        Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                        Server server = dbContext.Servers.Single(x => x.ServerName == DatabaseConstants.TEST_SERVER_NAME);
                        GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter any login", StringComparison.InvariantCultureIgnoreCase));
                        SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("server", StringComparison.InvariantCultureIgnoreCase));
                        bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                                && x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                                && x.GranularPermissionId == gp.GranularPermissionId
                                                                                && x.SecurableClassId == sc.SecurableClassId
                                                                                && x.Granted == true);
                        Assert.IsTrue(exists);

                        Login login = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        Assert.AreEqual(newLoginName, login.LoginName);

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al conceder permisos al login '{newLogin}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter any schema.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnySchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter any schema to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = loginNameAux;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == newSchemaName);
                        Assert.AreEqual(newSchemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSchemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter any schema and; read and write source.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnySchemaAndReadSource()
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

            string command = $"use {databaseName}; create source {sourceNameTest} (SourceTimestamp datetime, MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000)); create source {sourceForInto} (c1 string(4000), c3 string(4000)); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter any schema, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}";
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

                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == newStreamName);
                        Assert.AreEqual(newStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants alter any schema.
        /// </summary>
        [TestMethod]
        public void GrantAlterAnySchema2()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, alter any schema to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter source {oldSourceName} with name = {newSourceName}";

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
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    SpaceAssemblyBuilder sasmBuilder2 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder2 = sasmBuilder2.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder2 = new SpaceModuleBuilder(asmBuilder2);
                    smodBuilder2.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder2);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == newSourceName);
                        Assert.AreEqual(newSourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant alter any

        #region grant control

        /// <summary>
        /// Grant control on database.
        /// </summary>
        [TestMethod]
        public void GrantControlOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseNewName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseNewName && x.DbUsrName == userName);

                    bool exists = dbContext.DatabaseAssignedPermissionsToUsers.Any(x => x.DatabaseServerId == database.ServerId && x.DatabaseId == database.DatabaseId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant control on database role.
        /// </summary>
        [TestMethod]
        public void GrantControlOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant control on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == newRoleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role.ServerId && x.DbRoleDatabaseId == role.DatabaseId && x.DbRoleId == role.DbRoleId
                                                                        && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                        && x.Granted);
                    Assert.IsTrue(exists);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant control on user.
        /// </summary>
        [TestMethod]
        public void GrantControlOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == newUserName);

                        bool exists = dbContext.UserAssignedPermissionsToUsers.Any(x => x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.OnDbUsrServerId == user.ServerId && x.OnDbUsrDatabaseId == user.DatabaseId && x.OnDbUsrId == user.DbUsrId
                                                                            && x.Granted);

                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{userName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grant control on login.
        /// </summary>
        [TestMethod]
        public void GrantControlOnLogin()
        {
            string existingLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant control on login {existingLogin} to login {newLogin};
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

                    this.loginName = newLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);
                    try
                    {
                        Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                        Login onLogin = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                        SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("login", StringComparison.InvariantCultureIgnoreCase));
                        bool exists = dbContext.LoginsAssignedPermissionsToLogins.Any(x => x.LoginServerId == toLogin.ServerId && x.LoginId == toLogin.LoginId
                                                                                && x.OnLoginServerId == onLogin.ServerId && x.OnLoginId == onLogin.LoginId
                                                                                && x.GranularPermissionId == gp.GranularPermissionId
                                                                                && x.SecurableClassId == sc.SecurableClassId
                                                                                && x.Granted == true);
                        Assert.IsTrue(exists);

                        Login login = dbContext.Logins.Single(x => x.LoginName == newLoginName);
                        Assert.AreEqual(newLoginName, login.LoginName);

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al conceder permisos al login '{newLogin}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant control on schema.
        /// </summary>
        [TestMethod]
        public void GrantControlOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string loginNameAux = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant control on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = loginNameAux;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == newSchemaName);
                        Assert.AreEqual(newSchemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSchemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant control on stream and; read and write source.
        /// </summary>
        [TestMethod]
        public void GrantControlOnStreamAndReadSource()
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

            string command = $"use {databaseName}; create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create source {sourceForInto} (c1 string(4000), c3 string(4000)); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant control on stream {oldStreamName}, read on source {sourceNameTest}, write on source {sourceForInto} to user {userName}";
            string command2 = $"use {databaseName}; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == newStreamName);
                        Assert.AreEqual(newStreamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newStreamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant control on source.
        /// </summary>
        [TestMethod]
        public void GrantControlOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, control on source {oldSourceName} to user {userName}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; alter source {oldSourceName} with name = {newSourceName}";

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
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    SpaceAssemblyBuilder sasmBuilder2 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder2 = sasmBuilder2.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder2 = new SpaceModuleBuilder(asmBuilder2);
                    smodBuilder2.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder2);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == newSourceName);
                        Assert.AreEqual(newSourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{newSourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant control

        #region grant take ownership

        /// <summary>
        /// Grants take ownership permission on database role.
        /// </summary>
        [TestMethod]
        public void GrantTakeOwnershipOnDbRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant take ownership on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on role {roleName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.DbRoleName == roleName);
                    Assert.AreEqual<string>(userName, role.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants take ownership permission on database.
        /// </summary>
        [TestMethod]
        public void GrantTakeOwnershipOnDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant take ownership on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on database {databaseName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Assert.AreEqual<string>(otherLogin, database.Login.LoginName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants take ownership permission on schema.
        /// </summary>
        [TestMethod]
        public void GrantTakeOwnershipOnSchema()
        {
            string schemaName = "oldSchema";
            string existingUserName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {databaseName}; create schema {schemaName}; grant connect on database {databaseName} to user {existingUserName}; grant take ownership on schema {schemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; take ownership on schema {schemaName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Assert.AreEqual<string>(existingUserName, schema.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants take ownership permission on source.
        /// </summary>
        [TestMethod]
        public void GrantTakeOwnershipOnSource()
        {
            string oldSourceName = "oldSourceName";
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string databaseName = DatabaseConstants.MASTER_DATABASE_NAME;
            string command = $"create source {oldSourceName} (column1 int, column2 double, column3 string(4000)); grant connect on database {databaseName}, take ownership on source {oldSourceName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on source {oldSourceName}";
            string schemaName = DatabaseConstants.DBO_SCHEMA_NAME;

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
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Source source = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == oldSourceName);
                    Assert.AreEqual<string>(userName, source.DatabaseUser.DbUsrName);

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants take ownership permission on stream.
        /// </summary>
        [TestMethod]
        public void GrantTakeOwnershipOnStream()
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

            string command = $"use {databaseName}; create source {sourceForInto} (c1 string(4000), c3 string(4000)); create source {sourceNameTest} (MessageType string(4000), PrimaryAccountNumber string(4000), RetrievalReferenceNumber string(4000), SourceTimestamp datetime); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant take ownership on stream {oldStreamName}, write on source {sourceForInto}, read on source {sourceNameTest} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on stream {oldStreamName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == DatabaseConstants.TEST_SERVER_NAME && x.LoginName == this.loginName);
                    Space.Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == DatabaseConstants.DBO_SCHEMA_NAME);
                    Stream stream = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == oldStreamName);
                    Assert.AreEqual<string>(userName, stream.DatabaseUser.DbUsrName);

                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                    Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c3" && x.ColumnType == typeof(string).AssemblyQualifiedName));

                    Console.WriteLine();
                    tran.Rollback();
                }
            }
        }
        #endregion grant take ownership

        #region grant view any definition

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromServerRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.serverroles select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromEndpoints()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.endpoints select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromLogins()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.logins select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromDatabases()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.databases select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromUsers()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.users select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromDatabaseRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.databaseroles select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromSchemas()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.schemas select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromSources()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.sources select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any definition permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDefinitionAndFromStreams()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any definition to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.streams select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        #endregion grant view any definition

        #region grant view any database

        /// <summary>
        /// Grants view any database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDatabaseAndFromUsers()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.users select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDatabaseAndFromDatabaseRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.databaseroles select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDatabaseAndFromSchemas()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.schemas select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDatabaseAndFromSources()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.sources select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grants view any database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewAnyDatabaseAndFromStreams()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view any database to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.streams select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        #endregion grant view any database

        #region grant view definition on

        /// <summary>
        /// Grant view definition on endpoint permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromEndpoints()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on endpoint {DatabaseConstants.TCP_ENDPOINT_NAME} to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.endpoints select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on login permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromLogins()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on login {DatabaseConstants.NORMAL_LOGIN_2_NAME} to login {otherLogin}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.logins select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on database permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromDatabases()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on database {DatabaseConstants.MASTER_DATABASE_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.databases select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on user permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromUsers()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"use {DatabaseConstants.TEST_DATABASE_NAME}; grant view definition on user {DatabaseConstants.DBO_USER_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.users select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on database role permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromDatabaseRoles()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on role {DatabaseConstants.ROLE_1_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.databaseroles select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on schema permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromSchemas()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on schema {DatabaseConstants.DBO_SCHEMA_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.schemas select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on source permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromSources()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on source {DatabaseConstants.INPUT_SOURCE_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.sources select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// Grant view definition on stream permission.
        /// </summary>
        [TestMethod]
        public void GrantViewOnDefinitionAndFromStreams()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant view definition on stream {DatabaseConstants.TEST_STREAM_NAME} to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.TEST_DATABASE_NAME}; from sys.streams select ServerId as serverId into {DatabaseConstants.METADATA_OUTPUT_SOURCE_NAME}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    tran.Rollback();
                }
            }
        }

        #endregion grant view definition on

        #region grant create

        #region grant create any database

        /// <summary>
        /// Grants create any database.
        /// </summary>
        [TestMethod]
        public void GrantCreateAnyDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {DatabaseConstants.NORMAL_LOGIN_1_NAME}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create any database.
        /// </summary>
        [TestMethod]
        public void GrantCreateAnyDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {DatabaseConstants.NORMAL_LOGIN_1_NAME}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsTrue(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create any database.
        /// </summary>
        [TestMethod]
        public void GrantCreateAnyDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create any database to login {DatabaseConstants.NORMAL_LOGIN_1_NAME}";
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

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsFalse(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        #endregion grant create any database

        #region grant create database

        /// <summary>
        /// Grants create database permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateDatabase()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create database permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsTrue(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create database permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; grant create database to user {DatabaseConstants.NORMAL_USER_1_NAME}";
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

                    this.loginName = DatabaseConstants.NORMAL_LOGIN_1_NAME;
                    FirstLevelPipelineContext result = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Space.Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                        Assert.AreEqual(databaseName, database.DatabaseName);
                        Assert.IsFalse(database.IsActive);
                        tran.Rollback();
                    }
                    catch
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear la base de datos '{databaseName}'.");
                    }
                }
            }
        }

        #endregion grant create database

        #region grant create role

        /// <summary>
        /// Grants create role permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateRole()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create role {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create role permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateRoleWithStatusOn()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create role {roleName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create role permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateRoleWithStatusOff()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create role {roleName} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsFalse(role.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create role permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateRoleAddUser()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName = DatabaseConstants.NORMAL_USER_1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"Create role {roleName} with add = {userName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName);
                        Assert.AreEqual(userName, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create role permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateRoleAddUsers()
        {
            string roleName = DatabaseConstants.INEXISTENT_ROLE_NAME_1;
            string userName1 = DatabaseConstants.NORMAL_USER_1_NAME;
            string userName2 = DatabaseConstants.NORMAL_USER_2_NAME;
            string userName3 = DatabaseConstants.ADMIN_USER1_NAME;
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create role to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"Create role {roleName} with add = {userName1} {userName2} {userName3}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName);
                        Assert.AreEqual(roleName, role.DbRoleName);
                        Assert.IsTrue(role.IsActive);

                        DatabaseUser user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName1);
                        Assert.AreEqual(userName1, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName2);
                        Assert.AreEqual(userName2, user.DbUsrName);
                        user = role.DatabaseUsers.Single(x => x.ServerId == role.ServerId && x.DatabaseId == role.DatabaseId && x.DbUsrName == userName3);
                        Assert.AreEqual(userName3, user.DbUsrName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{roleName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant create role

        #region grant create schema

        /// <summary>
        /// Grants create schema permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateSchema()
        {
            string schemaName = "newSchema";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create schema to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create schema {schemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Schema schema = dbContext.Schemas.Single(x => x.SchemaName == schemaName);
                        Assert.AreEqual(schemaName, schema.SchemaName);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{schemaName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant create schema

        #region grant create source

        /// <summary>
        /// Grants create source permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateSource()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create source to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create source {sourceName} (column1 int, column2 double, column3 string(4000))";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    SpaceAssemblyBuilder sasmBuilder1 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder1 = sasmBuilder1.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder1 = new SpaceModuleBuilder(asmBuilder1);
                    smodBuilder1.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder1);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create source permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create source to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    SpaceAssemblyBuilder sasmBuilder1 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder1 = sasmBuilder1.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder1 = new SpaceModuleBuilder(asmBuilder1);
                    smodBuilder1.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder1);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsTrue(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grants create source permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string command = $"grant create source to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string command2 = $"create source {sourceName} (column1 int, column2 double, column3 string(4000)) with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    SpaceAssemblyBuilder sasmBuilder1 = new SpaceAssemblyBuilder("Test");
                    AssemblyBuilder asmBuilder1 = sasmBuilder1.CreateAssemblyBuilder();
                    SpaceModuleBuilder smodBuilder1 = new SpaceModuleBuilder(asmBuilder1);
                    smodBuilder1.CreateModuleBuilder();
                    kernel.Bind<AssemblyBuilder>().ToConstant(asmBuilder1);
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                        Assert.AreEqual(sourceName, source.SourceName);
                        Assert.IsFalse(source.IsActive);
                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{sourceName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant create source

        #region grant create stream

        /// <summary>
        /// Grant create stream permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateStream()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string sourceForInto = "sourceForInto";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create source {sourceForInto} (c1 string(4000), c2 string(4000), numeroXXX int); grant read on source {DatabaseConstants.INPUT_SOURCE_NAME}, write on source {sourceForInto}, create stream to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceForInto} ";

            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create stream {streamName} {{ {eql} }}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant create stream permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateStreamWithStatusOn()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string sourceForInto = "sourceForInto";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create source {sourceForInto} (c1 string(4000), c2 string(4000), numeroXXX int); grant write on source {sourceForInto}, read on source {DatabaseConstants.INPUT_SOURCE_NAME}, create stream to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceForInto} ";

            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create stream {streamName} {{ {eql} }} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsTrue(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Grant create stream permission.
        /// </summary>
        [TestMethod]
        public void GrantCreateStreamWithStatusOff()
        {
            string otherLogin = DatabaseConstants.NORMAL_LOGIN_1_NAME;
            string sourceForInto = "sourceForInto";
            string command = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create source {sourceForInto} (c1 string(4000), c2 string(4000), numeroXXX int); grant write on source {sourceForInto}, read on source {DatabaseConstants.INPUT_SOURCE_NAME}, create stream to user {DatabaseConstants.NORMAL_USER_1_NAME}";
            string streamName = "newStream";
            string eql = "cross " +
                                   $"JOIN {DatabaseConstants.INPUT_SOURCE_NAME} as t1 WHERE t1.PrimaryAccountNumber == \"9999941616073663_1\" " +
                                   $"WITH {DatabaseConstants.INPUT_SOURCE_NAME} as t2 WHERE t2.PrimaryAccountNumber == \"9999941616073663_2\" " +
                                   "ON t1.AcquiringInstitutionIdentificationCode == t2.AcquiringInstitutionIdentificationCode " +
                                   "TIMEOUT '00:00:02' " +
                                   $"SELECT (string)t1.PrimaryAccountNumber as c1, t2.PrimaryAccountNumber as c2, 1 as numeroXXX into {sourceForInto} ";

            string command2 = $"use {DatabaseConstants.MASTER_DATABASE_NAME}; create stream {streamName} {{ {eql} }} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext(initializer: null))
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    this.loginName = DatabaseConstants.SA_LOGIN_NAME;
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    this.loginName = otherLogin;
                    kernel = new StandardKernel();
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    kernel.Bind<ISourceTypeFactory>().ToConstructor(x => new SourceTypeFactory());
                    kernel.Bind<ISource>().ToConstructor(x => new ConcreteSource());
                    FirstLevelPipelineContext result2 = this.ProcessCommand(command2, kernel);

                    try
                    {
                        Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                        Assert.AreEqual(streamName, stream.StreamName);
                        Assert.IsFalse(stream.IsActive);
                        Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c1" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "c2" && x.ColumnType == typeof(string).AssemblyQualifiedName));
                        Assert.IsTrue(stream.ProjectionColumns.Any(x => x.ColumnName == "numeroXXX" && x.ColumnType == typeof(int).AssemblyQualifiedName));

                        tran.Rollback();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Assert.Fail($"Error al crear el rol de base de datos '{streamName}'. Mensaje: {e.Message}");
                    }
                }
            }
        }

        #endregion grant create stream

        #endregion grant create

        #endregion grant

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
