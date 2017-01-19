namespace Integra.Space.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.SqlClient;

    public partial class SpaceDbContext : DbContext
    {
        public SpaceDbContext()
            : base("SpaceConnection"
                  /*"data source=OSCARCANEK;initial catalog=SpaceCodeFirstTest;persist security info=True;user id=sa;password=Abc123$;MultipleActiveResultSets=True;App=EntityFramework"*/)
        {
            System.Data.Entity.Database.SetInitializer<SpaceDbContext>(null);
        }

        // securables
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
        public virtual DbSet<Endpoint> Endpoints { get; set; }
        public virtual DbSet<ServerRole> ServerRoles { get; set; }
        public virtual DbSet<Database> Databases { get; set; }
        public virtual DbSet<DatabaseRole> DatabaseRoles { get; set; }
        public virtual DbSet<DatabaseUser> DatabaseUsers { get; set; }
        public virtual DbSet<Schema> Schemas { get; set; }
        public virtual DbSet<Source> Sources { get; set; }
        public virtual DbSet<SourceColumn> SourceColumns { get; set; }
        public virtual DbSet<Stream> Streams { get; set; }
        public virtual DbSet<StreamColumn> StreamColumns { get; set; }
        public virtual DbSet<View> Views { get; set; }
        public virtual DbSet<SourceByStream> SourcesByStreams { get; set; }

        // permissions
        public virtual DbSet<GranularPermission> GranularPermissions { get; set; }
        public virtual DbSet<SecurableClass> SecurableClasses { get; set; }

        public virtual DbSet<DatabaseAssignedPermissionsToDBRole> DatabaseAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<DatabaseAssignedPermissionsToUser> DatabaseAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<DBRoleAssignedPermissionsToDBRole> DBRolesAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<DBRoleAssignedPermissionsToUser> DBRolesAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<EndpointAssignedPermissionsToLogin> EndpointsAssignedPermissionsToLogins { get; set; }
        public virtual DbSet<EndpointAssignedPermissionsToServerRole> EndpointsAssignedPermissionsToServerRoles { get; set; }
        public virtual DbSet<LoginAssignedPermissionsToLogin> LoginsAssignedPermissionsToLogins { get; set; }
        public virtual DbSet<LoginAssignedPermissionsToServerRole> LoginsAssignedPermissionsToServerRoles { get; set; }
        public virtual DbSet<PermissionBySecurable> PermissionsBySecurables { get; set; }
        public virtual DbSet<HierarchyPermissions> HierarchyPermissions { get; set; }
        public virtual DbSet<SchemaAssignedPermissionsToDBRole> SchemasAssignedPermissionsToDbRoles { get; set; }
        public virtual DbSet<SchemaAssignedPermissionsToUser> SchemaAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<ServerAssignedPermissionsToLogin> ServersAssignedPermissionsToLogins { get; set; }
        public virtual DbSet<ServerAssignedPermissionsToServerRole> ServersAssignedPermissionsToServerRoles { get; set; }
        public virtual DbSet<SourceAssignedPermissionsToDBRole> SourceAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<SourceAssignedPermissionsToUser> SourceAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<StreamAssignedPermissionsToDBRole> StreamAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<StreamAssignedPermissionsToUser> StreamAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<UserAssignedPermissionsToDBRole> UserAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<UserAssignedPermissionsToUsers> UserAssignedPermissionsToUsers { get; set; }
        public virtual DbSet<ViewAssignedPermissionsToDBRole> ViewAssignedPermissionsToDBRoles { get; set; }
        public virtual DbSet<ViewAssignedPermissionsToUser> ViewAssignedPermissionsToUsers { get; set; }

        // Vistas
        public virtual DbSet<PermissionView> VWPermissions { get; set; }

        public virtual DbSet<DatabaseRoleView> DatabaseRolesView { get; set; }
        public virtual DbSet<DatabaseView> DatabasesView { get; set; }
        public virtual DbSet<EndpointView> EndpointsView { get; set; }
        public virtual DbSet<LoginView> LoginsView { get; set; }
        public virtual DbSet<SchemaView> SchemasView { get; set; }
        public virtual DbSet<ServerRoleView> ServerRolesView { get; set; }
        public virtual DbSet<ServerView> ServersView { get; set; }
        public virtual DbSet<SourceColumnView> SourcecolumnsView { get; set; }
        public virtual DbSet<SourceView> SourcesView { get; set; }
        public virtual DbSet<StreamColumnView> StreamColumnsView { get; set; }
        public virtual DbSet<StreamView> StreamsView { get; set; }
        public virtual DbSet<UserView> UsersView { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.DatabaseAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRole_id, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.DBRolesAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.DBRolesAssignedPermissionsToDBRolesOn)
                .WithRequired(e => e.DatabaseRole1)
                .HasForeignKey(e => new { e.OnDbRoleId, e.OnDbRoleServerId, e.OnDbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.DBRolesAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.SchemaAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.SourceAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.StreamAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.UserAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.ViewAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseRole)
                .HasForeignKey(e => new { e.DbRoleId, e.DbRoleServerId, e.DbRoleDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseRole>()
                .HasMany(e => e.DatabaseUsers)
                .WithMany(e => e.DatabaseRoles1)
                .Map(m => m.ToTable("database_roles_by_users", "space").MapLeftKey(new[] { "dbr_id", "dbr_srv_id", "dbr_db_id" }).MapRightKey(new[] { "dbusr_id", "dbusr_srv_id", "dbusr_db_id" }));

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.DatabaseAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.DatabaseRoles)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.OwnerId, e.OwnerServerId, e.OwnerDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.Databases)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.OwnerServerId, e.OwnerId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.Logins)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.DBRolesAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.Endpoints)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.OwnerServerId, e.OwnerId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.SchemaAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.Schemas)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.OwnerId, e.OwnerServerId, e.OwnerDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.SourceAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUserId, e.DbUsrServerId, e.DbUserDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.Sources)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.OwnerId, e.OwnerServerId, e.OwnerDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.StreamAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.Streams)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.OwnerId, e.OwnerServerId, e.OwnerDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.UserAssignedPermissionsToDBRoles)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.UserAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.UserAssignedPermissionsToUsers1)
                .WithRequired(e => e.DatabaseUser1)
                .HasForeignKey(e => new { e.OnDbUsrId, e.OnDbUsrServerId, e.OnDbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.ViewAssignedPermissionsToUsers)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.DbUsrId, e.DbUsrServerId, e.DbUsrDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DatabaseUser>()
                .HasMany(e => e.Views)
                .WithRequired(e => e.DatabaseUser)
                .HasForeignKey(e => new { e.OwnerId, e.OwnerServerId, e.OwnerDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.DatabaseUsers)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.LoginServerId, e.LoginId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.DatabaseAssignedPermissionsToDBRoles)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.DatabaseServerId, e.DatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.Logins)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.DefaultDatabaseServerId, e.DefaultDatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.DatabaseAssignedPermissionsToUsers)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.DatabaseServerId, e.DatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.DatabaseRoles)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.DatabaseUsers)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Database>()
                .HasMany(e => e.Schemas)
                .WithRequired(e => e.Database)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Endpoint>()
                .HasMany(e => e.EndpointsAssignedPermissionsToLogins)
                .WithRequired(e => e.Endpoint)
                .HasForeignKey(e => new { e.EndpointId, e.EndpointServerId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Endpoint>()
                .HasMany(e => e.EndpointsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.Endpoint)
                .HasForeignKey(e => new { e.EndpointId, e.EndpointServerId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GranularPermission>()
                .HasMany(e => e.PermissionsBySecurables)
                .WithRequired(e => e.GranularPermission)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.LoginsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.ServerRoleServerId, e.ServerRoleId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.EndpointsAssignedPermissionsToLogins)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.LoginServerId, e.LoginId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.LoginsAssignedPermissionsToLogins)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.LoginServerId, e.LoginId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.LoginsAssignedPermissionsToLogins1)
                .WithRequired(e => e.LoginOn)
                .HasForeignKey(e => new { e.OnLoginServerId, e.OnLoginId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.ServersAssignedPermissionsToLogins)
                .WithRequired(e => e.Login)
                .HasForeignKey(e => new { e.LoginServerId, e.LoginId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Login>()
                .HasMany(e => e.ServerRoles)
                .WithMany(e => e.Logins)
                .Map(m => m.ToTable("server_roles_by_logins", "space").MapLeftKey(new[] { "lg_srv_id", "lg_id" }).MapRightKey(new[] { "sr_srv_id", "sr_id" }));

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.DatabaseAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.HierarchyPermissionsForChilds)
                .WithRequired(e => e.Permission)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.HierarchyPermissionsForParents)
                .WithRequired(e => e.ParentPermission)
                .HasForeignKey(e => new { e.ParentSecurableClassId, e.ParentGranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.DatabaseAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.DBRolesAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.DBRolesAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.EndpointsAssignedPermissionsToLogins)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.EndpointsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.LoginsAssignedPermissionsToLogins)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.LoginsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<PermissionBySecurable>()
            //    .HasOptional(e => e.Parent)
            //    .WithMany(x => x.Children)
            //    .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId });

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.SchemaAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.SchemaAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.ServersAssignedPermissionsToLogins)
                .WithRequired(e => e.PermissionsBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.ServersAssignedPermissionsToServerRoles)
                .WithRequired(e => e.PermissionsBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.SourceAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.SourceAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionsBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.StreamAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.stream_assigned_permissions_to_users)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.UserAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.UserAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.ViewAssignedPermissionsToDBRoles)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PermissionBySecurable>()
                .HasMany(e => e.ViewAssignedPermissionsToUsers)
                .WithRequired(e => e.PermissionBySecurable)
                .HasForeignKey(e => new { e.SecurableClassId, e.GranularPermissionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.SchemaAssignedPermissionsToDBRoles)
                .WithRequired(e => e.Schema)
                .HasForeignKey(e => new { e.SchemaServerId, e.SchemaDatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.DatabaseUsers)
                .WithRequired(e => e.DefaultSchema)
                .HasForeignKey(e => new { e.DefaultSchemaServerId, e.DefaultSchemaDatabaseId, e.DefaultSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.SchemaAssignedPermissionsToUsers)
                .WithRequired(e => e.Schema)
                .HasForeignKey(e => new { e.SchemaServerId, e.SchemaDatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.Sources)
                .WithRequired(e => e.Schema)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.Streams)
                .WithRequired(e => e.Schema)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Schema>()
                .HasMany(e => e.Views)
                .WithRequired(e => e.Schema)
                .HasForeignKey(e => new { e.ServerId, e.DatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SecurableClass>()
                .HasMany(e => e.PermissionsBySecurables)
                .WithRequired(e => e.SecurableClass)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerRole>()
                .HasMany(e => e.ServersAssignedPermissionsToServeRoles)
                .WithRequired(e => e.ServerRole)
                .HasForeignKey(e => new { e.ServerRoleServerId, e.ServerRoleId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerRole>()
                .HasMany(e => e.EndpointsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.ServerRole)
                .HasForeignKey(e => new { e.ServerRoleServerId, e.ServerRoleId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServerRole>()
                .HasMany(e => e.LoginsAssignedPermissionsToServerRoles)
                .WithRequired(e => e.ServerRole)
                .HasForeignKey(e => new { e.ServerRoleServerId, e.ServerRoleId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.Databases)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.Endpoints)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.ServerRoles)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.ServersAssignedPermissionsToLogins)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Server>()
                .HasMany(e => e.ServersAssignedPermissionsToServerRoles)
                .WithRequired(e => e.Server)
                .HasForeignKey(e => e.ServerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Source>()
                .HasMany(e => e.SourceAssignedPermissionsToDBRoles)
                .WithRequired(e => e.Source)
                .HasForeignKey(e => new { e.SourceId, e.SourceServerId, e.SourceDatabaseId, e.SourceSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Source>()
                .HasMany(e => e.SourceAssignedPermissionsToUsers)
                .WithRequired(e => e.Source)
                .HasForeignKey(e => new { e.SourceId, e.SourceServerId, e.SourceDatabaseId, e.SourceSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Source>()
                .HasMany(e => e.Columns)
                .WithRequired(e => e.Source)
                .HasForeignKey(e => new { e.SourceId, e.ServerId, e.DatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Source>()
                .HasMany(e => e.Streams)
                .WithRequired(e => e.Source)
                .HasForeignKey(e => new { e.SourceId, e.SourceServerId, e.SourceDatabaseId, e.SourceSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stream>()
                .Property(e => e.Query)
                .IsUnicode(false);

            modelBuilder.Entity<Stream>()
                .HasMany(e => e.StreamAssignedPermissionsToDBRoles)
                .WithRequired(e => e.Stream)
                .HasForeignKey(e => new { e.StreamId, e.StreamServerId, e.StreamDatabaseId, e.StreamSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stream>()
                .HasMany(e => e.StreamAssignedPermissionsToUsers)
                .WithRequired(e => e.Stream)
                .HasForeignKey(e => new { e.StreamId, e.StreamServerId, e.StreamDatabaseId, e.StreamSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stream>()
                .HasMany(e => e.Sources)
                .WithRequired(e => e.Stream)
                .HasForeignKey(e => new { e.StreamId, e.StreamServerId, e.StreamDatabaseId, e.StreamSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Stream>()
                .HasMany(e => e.ProjectionColumns)
                .WithRequired(e => e.Stream)
                .HasForeignKey(e => new { e.StreamId, e.ServerId, e.DatabaseId, e.SchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<View>()
                .Property(e => e.Predicate)
                .IsUnicode(false);

            modelBuilder.Entity<View>()
                .HasMany(e => e.ViewAssignedPermissionsToDBRoles)
                .WithRequired(e => e.View)
                .HasForeignKey(e => new { e.ViewId, e.ViewServerId, e.ViewDatabaseId, e.ViewSchemaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<View>()
                .HasMany(e => e.ViewAssignedPermissionsToUsers)
                .WithRequired(e => e.View)
                .HasForeignKey(e => new { e.ViewId, e.ViewServerId, e.ViewDatabaseId, e.ViewSchemaId })
                .WillCascadeOnDelete(false);
        }
    }
}
