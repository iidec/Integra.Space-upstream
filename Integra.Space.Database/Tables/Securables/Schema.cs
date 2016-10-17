namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.schemas")]
    public partial class Schema
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Schema()
        {
            SchemaAssignedPermissionsToDBRoles = new HashSet<SchemaAssignedPermissionsToDBRole>();
            SchemaAssignedPermissionsToUsers = new HashSet<SchemaAssignedPermissionsToUser>();
            Sources = new HashSet<Source>();
            Streams = new HashSet<Stream>();
            Views = new HashSet<View>();
            DatabaseUsers = new HashSet<DatabaseUser>();
        }

        [Key]
        [Column("sch_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SchemaId { get; set; }

        [Required]
        [Column("sch_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string SchemaName { get; set; }

        [Key]
        [Column("srv_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Key]
        [Column("db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        [Column("owner_id")]
        public System.Guid? OwnerId { get; set; }

        [Column("owner_db_id")]
        public System.Guid? OwnerDatabaseId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid? OwnerServerId { get; set; }
        
        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual Database Database { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseUser> DatabaseUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToDBRole> SchemaAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SchemaAssignedPermissionsToUser> SchemaAssignedPermissionsToUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Source> Sources { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Stream> Streams { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<View> Views { get; set; }
    }
}
