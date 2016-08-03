namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.granular_permissions")]
    public partial class GranularPermission
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GranularPermission()
        {
            PermissionsBySecurables = new HashSet<PermissionBySecurable>();
        }

        [Key]
        [Column("gp_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Required]
        [Column("gp_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string GranularPermissionName { get; set; }

        [StringLength(5)]
        public string GranularPermissionCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PermissionBySecurable> PermissionsBySecurables { get; set; }
    }
}
