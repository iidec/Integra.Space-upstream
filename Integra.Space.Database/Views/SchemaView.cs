namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_schemas")]
    public partial class SchemaView
    {
        [Key]
        [Column("SchemaId", Order = 2)]
        public string SchemaId { get; set; }

        [Column("SchemaName")]
        public string SchemaName { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Key]
        [Column("DatabaseId", Order = 1)]
        public string DatabaseId { get; set; }

        [Column("OwnerId")]
        public string OwnerId { get; set; }

        [Column("OwnerDatabaseId")]
        public string OwnerDatabaseId { get; set; }

        [Column("OwnerServerId")]
        public string OwnerServerId { get; set; }
    }
}
