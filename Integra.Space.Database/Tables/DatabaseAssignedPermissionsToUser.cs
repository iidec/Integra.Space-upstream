namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.database_assigned_permissions_to_users")]
    public partial class DatabaseAssignedPermissionsToUser
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
        [Column("db_srv_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseServerId { get; set; }

        [Key]
        [Column("db_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        public virtual DatabaseUser DatabaseUser { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual Database Database { get; set; }
    }
}
