namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_stream_columns")]
    public partial class StreamColumnView
    {
        [Column("ServerId")]
        public string ServerId { get; set; }

        [Column("DatabaseId")]
        public string DatabaseId { get; set; }

        [Column("SchemaId")]
        public string SchemaId { get; set; }

        [Column("StreamId")]
        public string StreamId { get; set; }

        [Key]
        [Column("ColumnId", Order = 0)]
        public string ColumnId { get; set; }

        [Column("ColumnName")]
        public string ColumnName { get; set; }

        [Column("ColumnType")]
        public string ColumnType { get; set; }
    }
}
