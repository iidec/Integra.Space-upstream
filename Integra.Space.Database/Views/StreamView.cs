namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_streams")]
    public partial class StreamView
    {
        [Key]
        [Column("StreamId", Order = 3)]
        public string StreamId { get; set; }

        [Column("StreamName")]
        public string StreamName { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Key]
        [Column("DatabaseId", Order = 1)]
        public string DatabaseId { get; set; }

        [Key]
        [Column("SchemaId", Order = 2)]
        public string SchemaId { get; set; }

        [Column("OwnerId")]
        public string OwnerId { get; set; }

        [Column("OwnerDatabaseId")]
        public string OwnerDatabaseId { get; set; }

        [Column("OwnerServerId")]
        public string OwnerServerId { get; set; }

        [Column("Query")]
        public string Query { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
