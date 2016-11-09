using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integra.Space.Pipeline;
using Ninject;
using Integra.Space.Database;
using System.Data.Entity;
using System.Linq;

namespace Integra.Space.UnitTests
{
    [TestClass]
    public class DenyCommandTests
    {
        private string loginName = "LoginAux";

        private FirstLevelPipelineContext ProcessCommand(string command, IKernel kernel)
        {
            CommandPipelineBuilder cpb = new CommandPipelineBuilder();
            Filter<FirstLevelPipelineContext, FirstLevelPipelineContext> pipeline = cpb.Build();

            FirstLevelPipelineExecutor cpe = new FirstLevelPipelineExecutor(pipeline);
            FirstLevelPipelineContext context = new FirstLevelPipelineContext(command, loginName, kernel);
            FirstLevelPipelineContext result = cpe.Execute(context);
            return result;
        }

        #region deny

        #region deny alter

        [TestMethod]
        public void DenyAlterOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on database {databaseName} to user {userName}; deny alter on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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
                    catch(AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch(Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            string command2 = $"use {databaseName}; add {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role1 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName1);
                    DatabaseRole role2 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName2);
                    Database.DatabaseUser dbUser1 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Database.DatabaseUser dbUser2 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);

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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleAddUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            string command2 = $"use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseRole role1 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName1);
                    DatabaseRole role2 = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == databaseName && x.DbRoleName == roleName2);
                    Database.DatabaseUser dbUser1 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName1);
                    bool exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role1.ServerId && x.DbRoleDatabaseId == role1.DatabaseId && x.DbRoleId == role1.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    exists = dbContext.DBRolesAssignedPermissionsToUsers.Any(x => x.DbRoleServerId == role2.ServerId && x.DbRoleDatabaseId == role2.DatabaseId && x.DbRoleId == role2.DbRoleId
                                                                        && x.DbUsrServerId == dbUser1.ServerId && x.DbUsrDatabaseId == dbUser1.DatabaseId && x.DbUsrId == dbUser1.DbUsrId
                                                                        && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    Database.DatabaseUser dbUser2 = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName2);

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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserToRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter on role {roleName} to user {userName}; deny alter on role {roleName} to user {userName}";
            command += $"; use {databaseName}; add {userName} to {roleName}";
            string command2 = $"remove {userName} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    bool existe = dbContext.DatabaseRoles.Single(x => x.DbRoleName == roleName).DatabaseUsers.Any(x => x.DbUsrName == userName);
                    Assert.IsTrue(existe);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserToRoles()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName} with login = {otherLogin}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName}";
            command += $"; use {databaseName}; add {userName} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRole1()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName}";
            string command2 = $"remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRole2()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName} to user {userName1}, user {userName2}; deny alter on role {roleName} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName}";
            string command2 = $"remove {userName1}, {userName2} to {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRoles1()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseRoleRemoveUserListToRoles2()
        {
            string roleName1 = "newRole1";
            string roleName2 = "newRole2";
            string databaseName = "newDatabase";
            string userName1 = "newUser1";
            string userName2 = "newUser2";
            string otherLogin1 = "LoginAux";
            string otherLogin2 = "LoginForTest";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName1}; create role {roleName2}; create user {userName1} with login = {otherLogin1}; create user {userName2} with login = {otherLogin2}; grant alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}; deny alter on role {roleName1}, alter on role {roleName2} to user {userName1}, user {userName2}";
            command += $"; use {databaseName}; add {userName1}, {userName2} to {roleName1}, {roleName2}";
            string command2 = $"remove {userName1}, {userName2} to {roleName1}, {roleName2}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter on user {userName} to user {userName}; deny alter on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnLogin1()
        {
            string existingLogin = "LoginAux";
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter on login {existingLogin} to login {newLogin}; deny alter on login {existingLogin} to login {newLogin};
                                    use Database1;
                                    create user bar with login = {newLogin}";
            string command2 = $"use Database1; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = "UserAux";
            string databaseName = "Database1";
            string loginNameAux = "LoginAux";
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter on schema {oldSchemaName} to user {existingUserName}; deny alter on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == oldSchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == existingUserName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnStream()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = "UserAux";
            string databaseName = "Database1";
            string sourceNameTest = "source1234";
            string otherLogin = "LoginAux";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.@event.Message.#1.#0 as c1, " +
                                          $@"t2.@event.Message.#1.#0 as c3 ";

            string command = $"create source {sourceNameTest} (column1 int, column2 decimal, column3 string); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest} to user {userName}; deny alter on stream {oldStreamName} to user {userName}";
            string command2 = $"use Database1; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("alter", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyReadOnSource()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = "UserAux";
            string databaseName = "Database1";
            string sourceNameTest = "source1234";
            string otherLogin = "LoginAux";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.@event.Message.#1.#0 as c1, " +
                                          $@"t2.@event.Message.#1.#0 as c3 ";

            string command = $"create source {sourceNameTest} (column1 int, column2 decimal, column3 string); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter on stream {oldStreamName}, read on source {sourceNameTest} to user {userName}; deny read on source {sourceNameTest} to user {userName}";

            string newEql = "inner " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:03.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t2.@event.Message.#1.#0 as c1, " +
                                          $@"t1.@event.Message.#1.#0 as c3 ";

            string command2 = $"use Database1; alter stream {oldStreamName} with query = {newEql}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    Source source = dbContext.Sources.Single(x => x.SourceName == sourceNameTest);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("read", StringComparison.InvariantCultureIgnoreCase));
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = "UserAux";
            string databaseName = "Database1";
            string otherLogin = "LoginAux";
            string command = $"create source {oldSourceName} (column1 int, column2 decimal, column3 string); grant connect on database {databaseName}, alter on source {oldSourceName} to user {userName}; deny connect on database {databaseName}, alter on source {oldSourceName} to user {userName}";
            string command2 = $"use Database1; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);
                    
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyAlterAnyDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string databaseNewName = "newDatabaseName";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any database to login {otherLogin}; deny alter any database to login {otherLogin}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
                    Login onLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);

                    bool exists = dbContext.ServersAssignedPermissionsToLogins.Any(x => x.ServerId == server.ServerId
                                                                        && x.LoginServerId == onLogin.ServerId && x.LoginId == onLogin.LoginId
                                                                        && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = otherLogin;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnyDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant alter any role to user {userName}; deny alter any role to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnyDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant alter any user to user {userName}; deny alter any user to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == databaseName && x.DbUsrName == userName);
                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnyLogin()
        {
            string existingLogin = "LoginAux";
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin}; deny alter any login to login {newLogin};
                                    use Database1;
                                    create user bar with login = {newLogin}";
            string command2 = $"use Database1; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnyLogin2()
        {
            string existingLogin = "LoginAux";
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant alter any login to login {newLogin}; deny alter any login to login {newLogin};
                                    use Database1;
                                    create user bar with login = {newLogin}";
            string command2 = $"use Database1; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == newLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnySchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = "UserAux";
            string databaseName = "Database1";
            string loginNameAux = "LoginAux";
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant alter any schema to user {existingUserName}; deny alter any schema to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == oldSchemaName);
                    Assert.AreEqual(oldSchemaName, schema.SchemaName);
                    
                    try
                    {
                        this.loginName = loginNameAux;
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnySchemaAndReadSource()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = "UserAux";
            string databaseName = "Database1";
            string sourceNameTest = "source1234";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.@event.Message.#1.#0 as c1, " +
                                          $@"t2.@event.Message.#1.#0 as c3 ";

            string command = $"create source {sourceNameTest} (column1 int, column2 decimal, column3 string); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant alter any schema, read on source {sourceNameTest} to user {userName}; deny alter any schema, read on source {sourceNameTest} to user {userName}";
            string command2 = $"use Database1; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    try
                    {
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyAlterAnySchema2()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = "UserAux";
            string databaseName = "Database1";
            string command = $"create source {oldSourceName} (column1 int, column2 decimal, column3 string); grant connect on database {databaseName}, alter any schema to user {userName}; deny connect on database {databaseName}, alter any schema to user {userName}";
            string command2 = $"use Database1; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);
                    
                    try
                    {
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyControlOnDatabase()
        {
            string databaseName = "Database123456789";
            string databaseNewName = "dbnueva";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on database {databaseName} to user {userName}; deny control on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; alter database {databaseName} with name = {databaseNewName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnDatabaseRole()
        {
            string roleName = "roleAux";
            string newRoleName = "newNameRole";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant control on role {roleName} to user {userName}; deny control on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; alter role {roleName} with name = {newRoleName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnDatabaseUser()
        {
            string newUserName = "newNameUser";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant control on user {userName} to user {userName}; deny control on user {userName} to user {userName}";
            string command2 = $"use {databaseName}; alter user {userName} with name = {newUserName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnLogin()
        {
            string existingLogin = "LoginAux";
            string newLogin = "AdminLogin12345";
            string newLoginName = "foo";
            string command = $@"create login {newLogin} with password = ""pass1234""; 
                                    grant control on login {existingLogin} to login {newLogin}; deny control on login {existingLogin} to login {newLogin};
                                    use Database1;
                                    create user bar with login = {newLogin}";
            string command2 = $"use Database1; alter login {existingLogin} with name = {newLoginName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnSchema()
        {
            string oldSchemaName = "oldSchema";
            string newSchemaName = "newSchema";
            string existingUserName = "UserAux";
            string databaseName = "Database1";
            string loginNameAux = "LoginAux";
            string command = $"use {databaseName}; create schema {oldSchemaName}; grant connect on database {databaseName} to user {existingUserName}; grant control on schema {oldSchemaName} to user {existingUserName}; deny control on schema {oldSchemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; alter schema {oldSchemaName} with name = {newSchemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == oldSchemaName);
                    Assert.AreEqual(oldSchemaName, schema.SchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == existingUserName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnStreamAndReadSource()
        {
            string oldStreamName = "oldStream";
            string newStreamName = "newStream";
            string userName = "UserAux";
            string databaseName = "Database1";
            string sourceNameTest = "source1234";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.@event.Message.#1.#0 as c1, " +
                                          $@"t2.@event.Message.#1.#0 as c3 ";

            string command = $"create source {sourceNameTest} (column1 int, column2 decimal, column3 string); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant control on stream {oldStreamName}, read on source {sourceNameTest} to user {userName}; deny control on stream {oldStreamName} to user {userName}";
            string command2 = $"use Database1; alter stream {oldStreamName} with name = {newStreamName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);
                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == oldStreamName);
                    Assert.AreEqual(oldStreamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);
                    Assert.AreEqual(eql.Replace('\n', '\0').Trim(), stream.Query);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("control", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);

                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyControlOnSource()
        {
            string oldSourceName = "oldSourceName";
            string newSourceName = "newSource";
            string userName = "UserAux";
            string databaseName = "Database1";
            string command = $"create source {oldSourceName} (column1 int, column2 decimal, column3 string); grant connect on database {databaseName}, control on source {oldSourceName} to user {userName}; deny control on source {oldSourceName} to user {userName}";
            string command2 = $"use Database1; alter source {oldSourceName} with name = {newSourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == oldSourceName);
                    Assert.AreEqual(oldSourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyTakeOwnershipOnDbRole()
        {
            string roleName = "roleAux";
            string databaseName = "newDatabase";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create role {roleName}; create user {userName} with login = {otherLogin}; grant take ownership on role {roleName} to user {userName}; deny take ownership on role {roleName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on role {roleName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Database.Database database = dbContext.Databases.Single(x => x.Server.ServerName == "Server1" && x.DatabaseName == databaseName);
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.DbRoleName == roleName);
                    Assert.AreEqual<string>("AdminLogin", role.DatabaseUser.DbUsrName);

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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyTakeOwnershipOnDatabase()
        {
            string databaseName = "Database123456789";
            string userName = "newUser";
            string otherLogin = "LoginAux";
            string command = $"create database {databaseName}; use {databaseName}; create user {userName} with login = {otherLogin}; grant take ownership on database {databaseName} to user {userName}; deny take ownership on database {databaseName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on database {databaseName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Assert.AreEqual<string>("AdminLogin", database.Login.LoginName);                    
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyTakeOwnershipOnSchema()
        {
            string schemaName = "oldSchema";
            string existingUserName = "UserAux";
            string databaseName = "Database1";
            string otherLogin = "LoginAux";
            string command = $"use {databaseName}; create schema {schemaName}; grant connect on database {databaseName} to user {existingUserName}; grant take ownership on schema {schemaName} to user {existingUserName}; deny take ownership on schema {schemaName} to user {existingUserName}";
            string command2 = $"use {databaseName}; take ownership on schema {schemaName}";
            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == existingUserName);
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyTakeOwnershipOnSource()
        {
            string oldSourceName = "oldSourceName";
            string userName = "UserAux";
            string databaseName = "Database1";
            string command = $"create source {oldSourceName} (column1 int, column2 decimal, column3 string); grant connect on database {databaseName}, take ownership on source {oldSourceName} to user {userName}; deny take ownership on source {oldSourceName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on source {oldSourceName}";
            string schemaName = "schema1";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == schemaName);
                    Database.Source source = dbContext.Sources.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.SourceName == oldSourceName);
                    Assert.AreEqual<string>("AdminUser", source.DatabaseUser.DbUsrName);
                    
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyTakeOwnershipOnStream()
        {
            string oldStreamName = "oldStream";
            string userName = "UserAux";
            string databaseName = "Database1";
            string sourceNameTest = "source1234";
            string eql = "cross " +
                                  $@"JOIN {sourceNameTest} as t1 WHERE t1.@event.Message.#0.#0 == ""0100""" +
                                  $@"WITH {sourceNameTest} as t2 WHERE t2.@event.Message.#0.#0 == ""0110""" +
                                  $@"ON (string)t1.@event.Message.#1.#0 == (string)t2.@event.Message.#1.#0 and (string)t1.@event.Message.#1.#1 == (string)t2.@event.Message.#1.#1 " +
                                  $@"TIMEOUT '00:00:01.5' " +
                                  $@"WHERE isnull(t2.@event.SourceTimestamp, '01/01/2017') - isnull(t1.@event.SourceTimestamp, '01/01/2016') <= '00:00:01' " +
                                  $@"SELECT " +
                                          $@"t1.@event.Message.#1.#0 as c1, " +
                                          $@"t2.@event.Message.#1.#0 as c3 ";

            string command = $"use {databaseName}; create source {sourceNameTest} (column1 int, column2 decimal, column3 string); create stream {oldStreamName} {{ {eql} }}; grant connect on database {databaseName} to user {userName}; grant take ownership on stream {oldStreamName}, read on source {sourceNameTest} to user {userName}; deny take ownership on stream {oldStreamName} to user {userName}";
            string command2 = $"use {databaseName}; take ownership on stream {oldStreamName}";

            IKernel kernel = new StandardKernel();

            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Login login = dbContext.Logins.Single(x => x.Server.ServerName == "Server1" && x.LoginName == loginName);
                    Database.Database database = dbContext.Databases.Single(x => x.ServerId == login.ServerId && x.DatabaseName == databaseName);
                    Database.Schema schema = dbContext.Schemas.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaName == "Schema1");
                    Database.Stream stream = dbContext.Streams.Single(x => x.ServerId == login.ServerId && x.DatabaseId == database.DatabaseId && x.SchemaId == schema.SchemaId && x.StreamName == oldStreamName);
                    Assert.AreEqual<string>("AdminUser", stream.DatabaseUser.DbUsrName);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("take ownership", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("stream", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.StreamAssignedPermissionsToUsers.Any(x => x.StreamServerId == stream.ServerId && x.StreamDatabaseId == stream.DatabaseId && x.StreamSchemaId == stream.SchemaId && x.StreamId == stream.StreamId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        this.loginName = "LoginAux";
                        this.ProcessCommand(command2, kernel);
                        Assert.Fail();
                    }
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromServerRoles()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.serverroles select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromEndpoints()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.endpoints select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromLogins()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.logins select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromDatabases()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.databases select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromUsers()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.users select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromDatabaseRoles()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.databaseroles select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromSchemas()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.schemas select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromSources()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.sources select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromStreams()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.streams select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDefinitionAndFromViews()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any definition to login {otherLogin}; deny view any definition to login {otherLogin}";
            string command2 = "from sys.views select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromUsers()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.users select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromDatabaseRoles()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.databaseroles select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromSchemas()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.schemas select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromSources()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.sources select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromStreams()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.streams select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewAnyDatabaseAndFromViews()
        {
            string otherLogin = "LoginAux";
            string command = $"grant view any database to login {otherLogin}; deny view any database to login {otherLogin}";
            string command2 = "from sys.views select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyViewOnDefinitionAndFromEndpoints()
        {
            string otherLogin = "LoginAux";
            string endpointName = "EndpointForTest";
            string command = $"grant view definition on endpoint {endpointName} to login {otherLogin}; deny view definition on endpoint {endpointName} to login {otherLogin}";
            string command2 = "from sys.endpoints select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromLogins()
        {
            string otherLogin = "LoginAux";
            string loginNameOn = "LoginForTest";
            string command = $"grant view definition on login {loginNameOn} to login {otherLogin}; deny view definition on login {loginNameOn} to login {otherLogin}";
            string command2 = "from sys.logins select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromDatabases()
        {
            string otherLogin = "LoginAux";
            string databaseName = "Database2";
            string userName = "UserAux";
            string command = $"grant view definition on database {databaseName} to user {userName}; deny view definition on database {databaseName} to user {userName}";
            string command2 = "from sys.databases select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                                        
                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == databaseName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == "Database1" && x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromUsers()
        {
            string otherLogin = "LoginAux";
            string userNameTo = "UserAux";
            string userNameOn = "UserForTest";
            string command = $"grant view definition on user {userNameOn} to user {userNameTo}; deny view definition on user {userNameOn} to user {userNameTo}";
            string command2 = "from sys.users select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    DatabaseUser userTo = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == "Database1" && x.DbUsrName == userNameTo);
                    DatabaseUser userOn = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == "Database1" && x.DbUsrName == userNameOn);
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
                    catch(AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromDatabaseRoles()
        {
            string otherLogin = "LoginAux";
            string roleName = "RoleForTest2";
            string userName = "UserAux";
            string command = $"grant view definition on role {roleName} to user {userName}; deny view definition on role {roleName} to user {userName}";
            string command2 = "from sys.databaseroles select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    DatabaseRole role = dbContext.DatabaseRoles.Single(x => x.Database.DatabaseName == "Database1" && x.DbRoleName == roleName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == "Database1" && x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromSchemas()
        {
            string otherLogin = "LoginAux";
            string schemaName = "Schema1";
            string userName = "UserAux";
            string command = $"grant view definition on schema {schemaName} to user {userName}; deny view definition on schema {schemaName} to user {userName}";
            string command2 = "from sys.schemas select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Schema schema = dbContext.Schemas.Single(x => x.SchemaName == schemaName);
                    Assert.AreEqual(schemaName, schema.SchemaName);
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromSources()
        {
            string otherLogin = "LoginAux";
            string sourceName = "SourceInicial";
            string userName = "UserAux";
            string command = $"grant view definition on source {sourceName} to user {userName}; deny view definition on source {sourceName} to user {userName}";
            string command2 = "from sys.sources select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Source source = dbContext.Sources.Single(x => x.SourceName == sourceName);
                    Assert.AreEqual(sourceName, source.SourceName);
                    Assert.IsTrue(source.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromStreams()
        {
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string streamName = "Stream123";
            string command = $"grant view definition on stream {streamName} to user {userName}; deny view definition on stream {streamName} to user {userName}";
            string command2 = "from sys.streams select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Stream stream = dbContext.Streams.Single(x => x.StreamName == streamName);
                    Assert.AreEqual(streamName, stream.StreamName);
                    Assert.IsTrue(stream.IsActive);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyViewOnDefinitionAndFromViews()
        {
            string otherLogin = "LoginAux";
            string viewName = "ViewForTest";
            string userName = "userAux";
            string command = $"grant view definition on view ViewForTest to user UserAux; deny view definition on view ViewForTest to user UserAux";
            string command2 = "from sys.views select ServerId as servId";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    View view = dbContext.Views.Single(x => x.ViewName == viewName);
                    Assert.AreEqual(viewName, view.ViewName);

                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.DbUsrName == userName);
                    GranularPermission gp = dbContext.GranularPermissions.Single(x => x.GranularPermissionName.Equals("view definition", StringComparison.InvariantCultureIgnoreCase));
                    SecurableClass sc = dbContext.SecurableClasses.Single(x => x.SecurableName.Equals("view", StringComparison.InvariantCultureIgnoreCase));
                    bool exists = dbContext.ViewAssignedPermissionsToUsers.Any(x => x.ViewServerId == view.ServerId && x.ViewDatabaseId == view.DatabaseId && x.ViewSchemaId == view.SchemaId && x.ViewId == view.ViewId
                                                                            && x.DbUsrServerId == user.ServerId && x.DbUsrDatabaseId == user.DatabaseId && x.DbUsrId == user.DbUsrId
                                                                            && x.GranularPermissionId == gp.GranularPermissionId
                                                                            && x.SecurableClassId == sc.SecurableClassId
                                                                            && x.Granted && x.Denied);
                    Assert.IsTrue(exists);

                    try
                    {
                        Assert.IsTrue(exists);
                    }
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateAnyDatabase()
        {
            string databaseName = "newDatabase";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use Database1; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateAnyDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use Database1; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateAnyDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create any database to login {otherLogin}; deny create any database to login {otherLogin}";
            string command2 = $"use Database1; create database {databaseName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Login toLogin = dbContext.Logins.Single(x => x.LoginName == otherLogin);
                    Server server = dbContext.Servers.Single(x => x.ServerName == "Server1");
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
                    catch (AssertFailedException e)
                    {
                        throw new Exception("Error");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateDatabase()
        {
            string databaseName = "newDatabase";
            string userName = "UserAux";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create database to user UserAux; deny create database to user {userName}";
            string command2 = $"use Database1; create database {databaseName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateDatabaseWithStatusOn()
        {
            string databaseName = "newDatabase";
            string userName = "UserAux";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create database to user {userName}; deny create database to user {userName}";
            string command2 = $"use Database1; create database {databaseName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateDatabaseWithStatusOff()
        {
            string databaseName = "newDatabase";
            string userName = "UserAux";
            string otherLogin = "LoginAux";
            string command = $"use Database1; grant create database to user UserAux; deny create database to user UserAux";
            string command2 = $"use Database1; create database {databaseName} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateRole()
        {
            string roleName = "role1";
            string userName = "UserAux";
            string otherLogin = "LoginAux";
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateRoleWithStatusOn()
        {
            string roleName = "role1";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateRoleWithStatusOff()
        {
            string roleName = "role1";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"create role {roleName} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateRoleAddUser()
        {
            string roleName = "role1";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"Create role {roleName} with add = {userName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateRoleAddUsers()
        {
            string roleName = "role1";
            string userName1 = "UserAux";
            string userName2 = "UserForTest";
            string userName3 = "AdminUser";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create role to user {userName}; deny create role to user {userName}";
            string command2 = $"Create role {roleName} with add = {userName1} {userName2} {userName3}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateSchema()
        {
            string schemaName = "newSchema";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create schema to user {userName}; deny create schema to user {userName}";
            string command2 = $"create schema {schemaName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateSource()
        {
            string sourceName = "newSource";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateSourceWithStatusOn()
        {
            string sourceName = "newSource";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateSourceWithStatusOff()
        {
            string sourceName = "newSource";
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant create source to user {userName}; deny create source to user {userName}";
            string command2 = $"create source {sourceName} with status = off";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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

        [TestMethod]
        public void DenyCreateStream()
        {
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant read on source SourceInicial, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);
                    
                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyReadSource()
        {
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string sourceName = "SourceInicial";
            string command = $"grant read on source {sourceName}, create stream to user {userName}; deny read on source {sourceName} to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }}";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
                    DatabaseUser user = dbContext.DatabaseUsers.Single(x => x.Database.DatabaseName == database.DatabaseName && x.DbUsrName == userName);
                    Source source = dbContext.Sources.Single(x => x.ServerId == database.ServerId && x.DatabaseId == database.DatabaseId && x.Schema.SchemaName == "Schema1" && x.SourceName == sourceName);

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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateStreamWithStatusOn()
        {
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant read on source SourceInicial, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }} with status = on";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        [TestMethod]
        public void DenyCreateStreamWithStatusOff()
        {
            string otherLogin = "LoginAux";
            string userName = "UserAux";
            string command = $"grant read on source SourceInicial, create stream to user {userName}; deny create stream to user {userName}";
            string streamName = "newStream";
            string eql = "cross " +
                                   "JOIN SourceInicial as t1 WHERE t1.@event.Message.#1.#2 == \"9999941616073663_1\" " +
                                   "WITH SourceInicial as t2 WHERE t2.@event.Message.#1.#2 == \"9999941616073663_2\" " +
                                   "ON t1.@event.Message.#1.#32 == t2.@event.Message.#1.#32 " +
                                   "TIMEOUT '00:00:02' " +
                                   //"WHERE  t1.@event.Message.#1.#43 == \"Shell El RodeoGUATEMALA    GT\" " +
                                   "SELECT (string)t1.@event.Message.#1.#2 as c1, t2.@event.Message.#1.#2 as c2, 1 as numeroXXX ";

            string command2 = $"create stream {streamName} {{ {eql} }} with status = off";
            this.loginName = "AdminLogin";

            IKernel kernel = new StandardKernel();
            using (SpaceDbContext dbContext = new SpaceDbContext())
            {
                using (DbContextTransaction tran = dbContext.Database.BeginTransaction())
                {
                    kernel.Bind<SpaceDbContext>().ToConstant(dbContext);

                    this.loginName = "AdminLogin";
                    FirstLevelPipelineContext result1 = this.ProcessCommand(command, kernel);

                    Database.Database database = dbContext.Databases.Single(x => x.DatabaseName == "Database1");
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
                    catch (AssertFailedException e)
                    {
                        Assert.Fail("Ejecutó un comando para el cual no tenía permisos.");
                    }
                    catch (Exception e)
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
    }
}
