namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.servers")]
    public partial class Server
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Server()
        {
            Databases = new HashSet<Database>();
            Endpoints = new HashSet<Endpoint>();
            Logins = new HashSet<Login>();
            ServerRoles = new HashSet<ServerRole>();
            ServersAssignedPermissionsToLogins = new HashSet<ServerAssignedPermissionsToLogin>();
            ServersAssignedPermissionsToServerRoles = new HashSet<ServerAssignedPermissionsToServerRole>();
        }

        [Key]
        [Column("srv_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ServerId { get; set; }

        [Required]
        [Column("srv_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string ServerName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Database> Databases { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Endpoint> Endpoints { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Login> Logins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerRole> ServerRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerAssignedPermissionsToLogin> ServersAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerAssignedPermissionsToServerRole> ServersAssignedPermissionsToServerRoles { get; set; }
    }
}
