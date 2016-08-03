namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.view_assigned_permissions_to_users")]
    public partial class ViewAssignedPermissionsToUser
    {
        [Key]
        [Column("dbusr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrId { get; set; }

        [Key]
        [Column("dbusr_db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrDatabaseId { get; set; }

        [Key]
        [Column("dbusr_srv_id", Order = 2)]
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
        [Column("vw_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ViewId { get; set; }

        [Key]
        [Column("vw_srv_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ViewServerId { get; set; }

        [Key]
        [Column("vw_db_id", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ViewDatabaseId { get; set; }

        [Key]
        [Column("vw_sch_id", Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ViewSchemaId { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual View View { get; set; }
    }
}
