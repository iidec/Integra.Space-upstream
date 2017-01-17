namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_databases")]
    public partial class DatabaseView
    {
        [Key]
        [Column("DatabaseId", Order = 1)]
        public string DatabaseId { get; set; }

        [Column("DatabaseName")]
        public string DatabaseName { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Column("OwnerId")]
        public string OwnerId { get; set; }

        [Column("OwnerServerId")]
        public string OwnerServerId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
