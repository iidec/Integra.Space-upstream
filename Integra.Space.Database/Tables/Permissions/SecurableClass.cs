namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.securable_classes")]
    public partial class SecurableClass
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SecurableClass()
        {
            PermissionsBySecurables = new HashSet<PermissionBySecurable>();
            //securables = new HashSet<Securable>();
        }

        [Key]
        [Column("sc_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Required]
        [Column("sc_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string SecurableName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PermissionBySecurable> PermissionsBySecurables { get; set; }
        
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<Securable> securables { get; set; }
    }
}
