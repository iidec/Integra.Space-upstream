namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.source_assigned_permissions_to_dbroles")]
    public partial class SourceAssignedPermissionsToDBRole
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
        [Column("so_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SourceId { get; set; }

        [Key]
        [Column("so_srv_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SourceServerId { get; set; }

        [Key]
        [Column("so_db_id", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SourceDatabaseId { get; set; }

        [Key]
        [Column("so_sch_id", Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SourceSchemaId { get; set; }

        public virtual DatabaseRole DatabaseRole { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual Source Source { get; set; }
    }
}
