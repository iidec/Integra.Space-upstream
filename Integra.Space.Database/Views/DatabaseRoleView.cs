namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_database_roles")]
    public partial class DatabaseRoleView
    {
        [Key]
        [Column("DatabaseRoleId", Order = 2)]
        public string DatabaseRoleId { get; set; }
        
        [Column("DatabaseRoleName")]
        public string DatabaseRoleName { get; set; }

        [Key]
        [Column("DatabaseId", Order = 1)]
        public string DatabaseId { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Column("OwnerId")]
        public string OwnerId { get; set; }

        [Column("OwnerDatabaseId")]
        public string OwnerDatabaseId { get; set; }

        [Column("OwnerServerId")]
        public string OwnerServerId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
