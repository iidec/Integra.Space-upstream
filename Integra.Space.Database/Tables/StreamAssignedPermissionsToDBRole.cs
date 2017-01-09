namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.stream_assigned_permissions_to_dbroles")]
    public partial class StreamAssignedPermissionsToDBRole
    {
        [Key]
        [Column("dbr_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleId { get; set; }

        [Key]
        [Column("dbr_db_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DbRoleDatabaseId { get; set; }

        [Key]
        [Column("dbr_srv_id", Order = 1)]
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
        [Column("st_id", Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamId { get; set; }

        [Key]
        [Column("st_srv_id", Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamServerId { get; set; }

        [Key]
        [Column("st_db_id", Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamDatabaseId { get; set; }

        [Key]
        [Column("st_sch_id", Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamSchemaId { get; set; }

        [Column("granted")]
        [DefaultValue(false)]
        public bool Granted { get; set; }

        [Column("denied")]
        [DefaultValue(false)]
        public bool Denied { get; set; }

        [Column("with_grant_option")]
        [DefaultValue(false)]
        public bool WithGrantOption { get; set; }

        public virtual DatabaseRole DatabaseRole { get; set; }

        public virtual PermissionBySecurable PermissionBySecurable { get; set; }

        public virtual Stream Stream { get; set; }
    }
}
