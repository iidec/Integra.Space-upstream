namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.endpoints")]
    public partial class Endpoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Endpoint()
        {
            EndpointsAssignedPermissionsToLogins = new HashSet<EndpointAssignedPermissionsToLogin>();
            EndpointsAssignedPermissionsToServerRoles = new HashSet<EndpointAssignedPermissionsToServerRole>();
        }

        [Key]
        [Column("ep_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid EndpointId { get; set; }

        [Required]
        [Column("ep_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string EnpointName { get; set; }

        [Key]
        [Column("srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Column("owner_id")]
        public System.Guid OwnerId { get; set; }

        [Column("owner_srv_id")]
        public System.Guid OwnerServerId { get; set; }

        public virtual Login Login { get; set; }

        public virtual Server Server { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EndpointAssignedPermissionsToLogin> EndpointsAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EndpointAssignedPermissionsToServerRole> EndpointsAssignedPermissionsToServerRoles { get; set; }

        //public virtual Securable securables { get; set; }
    }
}
