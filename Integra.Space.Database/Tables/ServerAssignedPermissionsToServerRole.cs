namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.servers_assigned_permissions_to_server_roles")]
    public partial class ServerAssignedPermissionsToServerRole
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
        [Column("srv_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        public virtual Login Login { get; set; }

        public virtual PermissionBySecurable PermissionsBySecurable { get; set; }

        public virtual Server Server { get; set; }
    }
}
