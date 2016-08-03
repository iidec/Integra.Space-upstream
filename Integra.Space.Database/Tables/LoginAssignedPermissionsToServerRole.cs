namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.logins_assigned_permissions_to_server_roles")]
    public partial class LoginAssignedPermissionsToServerRole
    {
        [Key]
        [Column("lg_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid LoginId { get; set; }

        [Key]
        [Column("lg_srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid LoginServerId { get; set; }

        [Key]
        [Column("sc_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("gp_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Key]
        [Column("sr_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerRoleId { get; set; }

        [Key]
        [Column("sr_srv_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerRoleServerId { get; set; }

        public virtual Login Login { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual ServerRole ServerRole { get; set; }
    }
}
