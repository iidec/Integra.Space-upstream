namespace Integra.Space.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.hierarchy_permissions")]
    public partial class HierarchyPermissions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HierarchyPermissions()
        {
        }

        [Key]
        [Column("hp_id")]
        public Guid Id { get; set; }
        
        [Column("sc_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }
        
        [Column("gp_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Column("sc_id_parent")]
        public System.Guid? ParentSecurableClassId { get; set; }

        [Column("gp_id_parent")]
        public System.Guid? ParentGranularPermissionId { get; set; }

        public virtual PermissionBySecurable Permission { get; set; }

        public virtual PermissionBySecurable ParentPermission { get; set; }
    }
}
