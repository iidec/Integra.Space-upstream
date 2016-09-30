namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.database_roles")]
    public partial class DatabaseRole
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DatabaseRole()
        {
            DatabaseAssignedPermissionsToDBRoles = new HashSet<DatabaseAssignedPermissionsToDBRole>();
            DBRolesAssignedPermissionsToDBRoles = new HashSet<DBRoleAssignedPermissionsToDBRole>();
            DBRolesAssignedPermissionsToDBRolesOn = new HashSet<DBRoleAssignedPermissionsToDBRole>();
            DBRolesAssignedPermissionsToUsers = new HashSet<DBRoleAssignedPermissionsToUser>();
            SchemaAssignedPermissionsToDBRoles = new HashSet<SchemaAssignedPermissionsToDBRole>();
            SourceAssignedPermissionsToDBRoles = new HashSet<SourceAssignedPermissionsToDBRole>();
            StreamAssignedPermissionsToDBRoles = new HashSet<StreamAssignedPermissionsToDBRole>();
            UserAssignedPermissionsToDBRoles = new HashSet<UserAssignedPermissionsToDBRole>();
            ViewAssignedPermissionsToDBRoles = new HashSet<ViewAssignedPermissionsToDBRole>();
            DatabaseUsers = new HashSet<DatabaseUser>();
        }

        [Key]
        [Column("dbr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid DbRoleId { get; set; }

        [Required]
        [Column("dbr_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string DbRoleName { get; set; }

        [Key]
        [Column("db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        [Key]
        [Column("srv_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }

        [Column("owner_db_id")]
        public System.Guid OwnerDatabaseId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToDBRole> DatabaseAssignedPermissionsToDBRoles { get; set; }

        public virtual Database Database { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToDBRole> DBRolesAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToDBRole> DBRolesAssignedPermissionsToDBRolesOn { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToUser> DBRolesAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToDBRole> SchemaAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToDBRole> SourceAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToDBRole> StreamAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToDBRole> UserAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToDBRole> ViewAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseUser> DatabaseUsers { get; set; }

        //public virtual Securable securable { get; set; }
    }
}
