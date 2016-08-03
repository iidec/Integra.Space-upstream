namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.views")]
    public partial class View
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public View()
        {
            ViewAssignedPermissionsToDBRoles = new HashSet<ViewAssignedPermissionsToDBRole>();
            ViewAssignedPermissionsToUsers = new HashSet<ViewAssignedPermissionsToUser>();
        }

        [Key]
        [Column("vw_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ViewId { get; set; }

        [Required]
        [Column("vw_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string ViewName { get; set; }

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

        [Column("vw_predicate", TypeName = "text")]
        [Required]
        public string Predicate { get; set; }

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }

        [Column("owner_db_id")]
        public System.Guid OwnerDatabaseId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual Schema Schema { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToDBRole> ViewAssignedPermissionsToDBRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ViewAssignedPermissionsToUser> ViewAssignedPermissionsToUsers { get; set; }
    }
}
