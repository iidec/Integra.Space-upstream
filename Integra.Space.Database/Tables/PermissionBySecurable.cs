namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.permissions_by_securables")]
    public partial class PermissionBySecurable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PermissionBySecurable()
        {
            DatabaseAssignedPermissionsToDBRoles = new HashSet<DatabaseAssignedPermissionsToDBRole>();
            DatabaseAssignedPermissionsToUsers = new HashSet<DatabaseAssignedPermissionsToUser>();
            DBRolesAssignedPermissionsToDBRoles = new HashSet<DBRoleAssignedPermissionsToDBRole>();
            DBRolesAssignedPermissionsToUsers = new HashSet<DBRoleAssignedPermissionsToUser>();
            EndpointsAssignedPermissionsToLogins = new HashSet<EndpointAssignedPermissionsToLogin>();
            EndpointsAssignedPermissionsToServerRoles = new HashSet<EndpointAssignedPermissionsToServerRole>();
            LoginsAssignedPermissionsToLogins = new HashSet<LoginAssignedPermissionsToLogin>();
            LoginsAssignedPermissionsToServerRoles = new HashSet<LoginAssignedPermissionsToServerRole>();
            SchemaAssignedPermissionsToDBRoles = new HashSet<SchemaAssignedPermissionsToDBRole>();
            SchemaAssignedPermissionsToUsers = new HashSet<SchemaAssignedPermissionsToUser>();
            ServersAssignedPermissionsToLogins = new HashSet<ServerAssignedPermissionsToLogin>();
            ServersAssignedPermissionsToServerRoles = new HashSet<ServerAssignedPermissionsToServerRole>();
            SourceAssignedPermissionsToDBRoles = new HashSet<SourceAssignedPermissionsToDBRole>();
            SourceAssignedPermissionsToUsers = new HashSet<SourceAssignedPermissionsToUser>();
            StreamAssignedPermissionsToDBRoles = new HashSet<StreamAssignedPermissionsToDBRole>();
            stream_assigned_permissions_to_users = new HashSet<StreamAssignedPermissionsToUser>();
            UserAssignedPermissionsToDBRoles = new HashSet<UserAssignedPermissionsToDBRole>();
            UserAssignedPermissionsToUsers = new HashSet<UserAssignedPermissionsToUsers>();
            ViewAssignedPermissionsToDBRoles = new HashSet<ViewAssignedPermissionsToDBRole>();
            ViewAssignedPermissionsToUsers = new HashSet<ViewAssignedPermissionsToUser>();
        }

        [Key]
        [Column("sc_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("gp_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Column("sc_id_parent")]
        public System.Guid? ParentSecurableClassId { get; set; }

        [Column("gp_id_parent")]
        public System.Guid? ParentGranularPermissionId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToDBRole> DatabaseAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToUser> DatabaseAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToDBRole> DBRolesAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToUser> DBRolesAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EndpointAssignedPermissionsToLogin> EndpointsAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EndpointAssignedPermissionsToServerRole> EndpointsAssignedPermissionsToServerRoles { get; set; }

        public virtual GranularPermission GranularPermission { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToLogin> LoginsAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToServerRole> LoginsAssignedPermissionsToServerRoles { get; set; }

        public virtual SecurableClass SecurableClass { get; set; }

        [InverseProperty("Children")]
        public virtual PermissionBySecurable Parent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PermissionBySecurable> Children { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToDBRole> SchemaAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToUser> SchemaAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerAssignedPermissionsToLogin> ServersAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerAssignedPermissionsToServerRole> ServersAssignedPermissionsToServerRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToDBRole> SourceAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToUser> SourceAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToDBRole> StreamAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToUser> stream_assigned_permissions_to_users { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToDBRole> UserAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToUsers> UserAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToDBRole> ViewAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToUser> ViewAssignedPermissionsToUsers { get; set; }
    }
}
