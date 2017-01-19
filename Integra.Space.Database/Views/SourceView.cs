namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_sources")]
    public partial class SourceView
    {
        [Key]
        [Column("SourceId", Order = 3)]
        public string SourceId { get; set; }

        [Column("SourceName")]
        public string SourceName { get; set; }

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

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("CacheDurability")]
        public uint CacheDurability { get; set; }

        [Column("CacheSize")]
        public uint CacheSize { get; set; }
    }
}
