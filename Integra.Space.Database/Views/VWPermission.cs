namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.vw_permissions")]
    public partial class VWPermission
    {
        [Column("serverIdOfSecurable")]
        public System.Guid? ServerIdOfSecurable { get; set; }

        [Column("databaseIdOfSecurable")]
        public System.Guid? DatabaseIdOfSecurable { get; set; }

        [Column("schemaIdOfSecurable")]
        public System.Guid? SchemaIdOfSecurable { get; set; }

        [Key]
        [Column("securableId", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableId { get; set; }

        [Key]
        [Column("serverIdOfPrincipal", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerIdOfPrincipal { get; set; }

        [Column("databaseIdOfPrincipal")]
        public System.Guid? DatabaseIdOfPrincipal { get; set; }

        [Key]
        [Column("principalId", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid PrincipalId { get; set; }

        [Key]
        [Column("securableClassId", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SecurableClassId { get; set; }

        [Key]
        [Column("granularPermissionId", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid GranularPermissionId { get; set; }
    }
}
