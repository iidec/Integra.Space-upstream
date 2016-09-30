namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.streams")]
    public partial class Stream
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stream()
        {
            StreamAssignedPermissionsToDBRoles = new HashSet<StreamAssignedPermissionsToDBRole>();
            StreamAssignedPermissionsToUsers = new HashSet<StreamAssignedPermissionsToUser>();
        }

        [Key]
        [Column("st_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamId { get; set; }

        [Required]
        [Column("st_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string StreamName { get; set; }

        [Key]
        [Column("srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Key]
        [Column("db_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        [Key]
        [Column("sch_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SchemaId { get; set; }

        [Column("st_query", TypeName = "text")]
        [Required]
        public string Query { get; set; }

        [Column("st_assembly", TypeName = "varbinary(max)")]
        [Required]
        public byte[] Assembly { get; set; }

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }

        [Column("owner_db_id")]
        public System.Guid OwnerDatabaseId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual Schema Schema { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToDBRole> StreamAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreamAssignedPermissionsToUser> StreamAssignedPermissionsToUsers { get; set; }

        //public virtual Securable securables { get; set; }
    }
}
