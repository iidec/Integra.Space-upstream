namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.databases")]
    public partial class Database
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Database()
        {
            DatabaseAssignedPermissionsToDBRoles = new HashSet<DatabaseAssignedPermissionsToDBRole>();
            DatabaseAssignedPermissionsToUsers = new HashSet<DatabaseAssignedPermissionsToUser>();
            DatabaseRoles = new HashSet<DatabaseRole>();
            DatabaseUsers = new HashSet<DatabaseUser>();
            Schemas = new HashSet<Schema>();
            Logins = new HashSet<Login>();
        }

        [Key]
        [Column("db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid DatabaseId { get; set; }

        [Required]
        [Column("db_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string DatabaseName { get; set; }

        [Key]
        [Column("srv_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }
        
        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        [Column("is_active")]
        [System.ComponentModel.DefaultValue(true)]
        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToDBRole> DatabaseAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Login> Logins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseAssignedPermissionsToUser> DatabaseAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseRole> DatabaseRoles { get; set; }

        public virtual Login Login { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseUser> DatabaseUsers { get; set; }

        public virtual Server Server { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schema> Schemas { get; set; }
    }
}
