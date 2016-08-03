namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.server_roles")]
    public partial class ServerRole
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServerRole()
        {
            LoginsAssignedPermissionsToServerRoles = new HashSet<LoginAssignedPermissionsToServerRole>();
            Logins = new HashSet<Login>();
        }

        [Key]
        [Column("sr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerRoleId { get; set; }

        [Required]
        [Column("sr_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string ServerRoleName { get; set; }

        [Key]
        [Column("srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToServerRole> LoginsAssignedPermissionsToServerRoles { get; set; }

        public virtual Server Server { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Login> Logins { get; set; }
    }
}
