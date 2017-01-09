namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.logins")]
    public partial class Login
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Login()
        {
            EndpointsAssignedPermissionsToLogins = new HashSet<EndpointAssignedPermissionsToLogin>();
            LoginsAssignedPermissionsToLogins = new HashSet<LoginAssignedPermissionsToLogin>();
            LoginsAssignedPermissionsToLogins1 = new HashSet<LoginAssignedPermissionsToLogin>();
            LoginsAssignedPermissionsToServerRoles = new HashSet<LoginAssignedPermissionsToServerRole>();
            ServersAssignedPermissionsToLogins = new HashSet<ServerAssignedPermissionsToLogin>();
            Endpoints = new HashSet<Endpoint>();
            DatabaseUsers = new HashSet<DatabaseUser>();
            ServerRoles = new HashSet<ServerRole>();
            Databases = new HashSet<Database>();
        }

        [Key]
        [Column("lg_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid LoginId { get; set; }

        [Required]
        [Column("lg_name")]
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string LoginName { get; set; }

        [Required]
        [Column("lg_password")]
        [StringLength(50)]
        public string LoginPassword { get; set; }

        [Key]
        [Column("srv_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ServerId { get; set; }

        [Column("default_db_id")]
        public Guid DefaultDatabaseId { get; set; }

        [Column("default_db_srv_id")]
        public Guid DefaultDatabaseServerId { get; set; }

        [Column("is_active")]
        [System.ComponentModel.DefaultValue(true)]
        public bool IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Database> Databases { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EndpointAssignedPermissionsToLogin> EndpointsAssignedPermissionsToLogins { get; set; }
        
        public virtual Server Server { get; set; }

        public virtual Database Database { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToLogin> LoginsAssignedPermissionsToLogins { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToLogin> LoginsAssignedPermissionsToLogins1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginAssignedPermissionsToServerRole> LoginsAssignedPermissionsToServerRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerAssignedPermissionsToLogin> ServersAssignedPermissionsToLogins { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatabaseUser> DatabaseUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServerRole> ServerRoles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Endpoint> Endpoints { get; set; }
    }
}
