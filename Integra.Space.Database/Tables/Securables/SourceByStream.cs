namespace Integra.Space.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.sources_by_streams")]
    public partial class SourceByStream
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SourceByStream()
        {
        }

        [Key]
        [Column("relationship_id", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid RelationshipId { get; set; }

        [Column("src_id", Order = 1)]
        [Required]
        public Guid SourceId { get; set; }

        [Column("src_sch_id", Order = 4)]
        [Required]
        public Guid SourceSchemaId { get; set; }

        [Column("src_db_id", Order = 3)]
        [Required]
        public Guid SourceDatabaseId { get; set; }
        
        [Column("src_srv_id", Order = 2)]
        [Required]
        public Guid SourceServerId { get; set; }
        
        [Column("st_id", Order = 5)]
        [Required]
        public Guid StreamId { get; set; }

        [Column("st_sch_id", Order = 8)]
        [Required]
        public Guid StreamSchemaId { get; set; }

        [Column("st_db_id", Order = 7)]
        [Required]
        public Guid StreamDatabaseId { get; set; }

        [Column("st_srv_id", Order = 6)]
        [Required]
        public Guid StreamServerId { get; set; }

        [Column("is_input_source", Order = 9)]
        [Required]
        public bool IsInputSource { get; set; }

        public virtual Source Source { get; set; }

        public virtual Stream Stream { get; set; }
    }
}
