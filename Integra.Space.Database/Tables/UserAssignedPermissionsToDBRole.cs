namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.user_assigned_permissions_to_dbroles")]
    public partial class UserAssignedPermissionsToDBRole
    {
        [Key]
        [Column("dbr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleId { get; set; }

        [Key]
        [Column("dbr_db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleDatabaseId { get; set; }

        [Key]
        [Column("dbr_srv_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleServerId { get; set; }

        [Key]
        [Column("sc_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("gp_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }

        [Key]
        [Column("dbusr_srv_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrServerId { get; set; }

        [Key]
        [Column("dbusr_db_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrDatabaseId { get; set; }

        [Key]
        [Column("dbusr_id", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbUsrId { get; set; }

        public virtual DatabaseRole DatabaseRole { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }
    }
}
