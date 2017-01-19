namespace Integra.Space.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("space.stream_columns")]
    public partial class StreamColumn
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StreamColumn()
        {
        }

        [Key]
        [Column("st_culumn_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ColumnId { get; set; }

        [Required]
        [Column("srv_id", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid ServerId { get; set; }

        [Required]
        [Column("db_id", Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid DatabaseId { get; set; }

        [Required]
        [Column("sch_id", Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid SchemaId { get; set; }

        [Required]
        [Column("st_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public System.Guid StreamId { get; set; }

        [Required]
        [Column("st_column_name")]
        public string ColumnName { get; set; }

        [Column("st_column_type")]
        public string ColumnType { get; set; }

        public virtual Stream Stream { get; set; }
    }
}
