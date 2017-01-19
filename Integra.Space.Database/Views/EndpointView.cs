namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_endpoints")]
    public partial class EndpointView
    {
        [Key]
        [Column("EndpointId", Order = 1)]
        public string EndpointId { get; set; }

        [Column("EndpointName")]
        public string EndpointName { get; set; }

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
