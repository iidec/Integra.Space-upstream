namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.logins_assigned_permissions_to_logins")]
    public partial class LoginAssignedPermissionsToLogin
    {
        [Key]
        [Column("lg_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid lg_id { get; set; }

        [Key]
        [Column("lg_srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid lg_ser_id { get; set; }

        [Key]
        [Column("sc_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("gc_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Key]
        [Column("on_lg_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid OnLoginId { get; set; }

        [Key]
        [Column("on_lg_srv_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid OnLoginServerId { get; set; }

        public virtual Login Login { get; set; }

        public virtual Login LoginOn { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }
    }
}
