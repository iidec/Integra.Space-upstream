namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.database_users")]
    public partial class DatabaseUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DatabaseUser()
        {
            DatabaseAssignedPermissionsToUsers = new HashSet<DatabaseAssignedPermissionsToUser>();
            DatabaseRoles = new HashSet<DatabaseRole>();
            DBRolesAssignedPermissionsToUsers = new HashSet<DBRoleAssignedPermissionsToUser>();
            SchemaAssignedPermissionsToUsers = new HashSet<SchemaAssignedPermissionsToUser>();
            Schemas = new HashSet<Schema>();
            SourceAssignedPermissionsToUsers = new HashSet<SourceAssignedPermissionsToUser>();
            Sources = new HashSet<Source>();
            StreamAssignedPermissionsToUsers = new HashSet<StreamAssignedPermissionsToUser>();
            Streams = new HashSet<Stream>();
            UserAssignedPermissionsToDBRoles = new HashSet<UserAssignedPermissionsToDBRole>();
            UserAssignedPermissionsToUsers = new HashSet<UserAssignedPermissionsToUsers>();
            UserAssignedPermissionsToUsers1 = new HashSet<UserAssignedPermissionsToUsers>();
            ViewAssignedPermissionsToUsers = new HashSet<ViewAssignedPermissionsToUser>();
            Views = new HashSet<View>();
            DatabaseRoles1 = new HashSet<DatabaseRole>();
        }

        [Key]
        [Column("dbusr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrId { get; set; }

        [Required]
        [Column("dbusr_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string DbUsrName { get; set; }
        
        [Key]
        [Column("db_id", Order = 2)]
        [Index(IsUnique = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        [Key]
        [Column("srv_id", Order = 1)]
        [Index(IsUnique = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }
        
        [Column("lg_id")]
        [Index(IsUnique = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid LoginId { get; set; }
        
        [Column("lg_srv_id")]
        [Index(IsUnique = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid LoginServerId { get; set; }

        [Column("default_sch_id")]
        public System.Guid DefaultSchemaId { get; set; }

        [Column("default_sch_db_id")]
        public System.Guid DefaultSchemaDatabaseId { get; set; }

        [Column("default_sch_srv_id")]
        public System.Guid DefaultSchemaServerId { get; set; }

        [Column("is_active")]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToUser> DatabaseAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseRole> DatabaseRoles { get; set; }
        
        public virtual Database Database { get; set; }

        public virtual Schema DefaultSchema { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DBRoleAssignedPermissionsToUser> DBRolesAssignedPermissionsToUsers { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToUser> SchemaAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schema> Schemas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToUser> SourceAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Source> Sources { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToUser> StreamAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Stream> Streams { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToDBRole> UserAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToUsers> UserAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAssignedPermissionsToUsers> UserAssignedPermissionsToUsers1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToUser> ViewAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<View> Views { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseRole> DatabaseRoles1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual Login Login { get; set; }

        //public virtual Securable securables { get; set; }
    }
}
