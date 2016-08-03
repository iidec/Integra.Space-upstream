namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.database_assigned_permissions_to_dbroles")]
    public partial class DatabaseAssignedPermissionsToDBRole
    {
        [Key]
        [Column("dbrole_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRole_id { get; set; }

        [Key]
        [Column("dbrole_db_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleDatabaseId { get; set; }

        [Key]
        [Column("dbrole_srv_id", Order = 2)]
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
        [Column("db_srv_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseServerId { get; set; }

        [Key]
        [Column("db_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        public virtual DatabaseRole DatabaseRole { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual Database Database { get; set; }
    }
}
