namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.user_assigned_permissions_to_users")]
    public partial class UserAssignedPermissionsToUsers
    {
        [Key]
        [Column("dbusr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrId { get; set; }

        [Key]
        [Column("dbusr_db_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrDatabaseId { get; set; }

        [Key]
        [Column("dbusr_srv_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrServerId { get; set; }

        [Key]
        [Column("sc_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("gp_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Key]
        [Column("on_dbusr_srv_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid OnDbUsrServerId { get; set; }

        [Key]
        [Column("on_dbusr_db_id", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid OnDbUsrDatabaseId { get; set; }

        [Key]
        [Column("on_dbusr_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid OnDbUsrId { get; set; }

        [Column("granted")]
        [DefaultValue(false)]
        public bool Granted { get; set; }

        [Column("denied")]
        [DefaultValue(false)]
        public bool Denied { get; set; }

        [Column("with_grant_option")]
        [DefaultValue(false)]
        public bool WithGrantOption { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual DatabaseUser DatabaseUser1 { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }
    }
}
