namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.sources")]
    public partial class Source
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Source()
        {
            SourceAssignedPermissionsToDBRoles = new HashSet<SourceAssignedPermissionsToDBRole>();
            SourceAssignedPermissionsToUsers = new HashSet<SourceAssignedPermissionsToUser>();
        }

        [Key]
        [Column("so_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SourceId { get; set; }

        [Required]
        [Column("so_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string SourceName { get; set; }

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

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }

        [Column("owner_db_id")]
        public System.Guid OwnerDatabaseId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        [Column("is_active")]
        [System.ComponentModel.DefaultValue(true)]
        public bool IsActive { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual Schema Schema { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToDBRole> SourceAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceAssignedPermissionsToUser> SourceAssignedPermissionsToUsers { get; set; }

        //public virtual Securable securables { get; set; }
    }
}
