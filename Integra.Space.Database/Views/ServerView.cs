namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_servers")]
    public partial class ServerView
    {
        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }
        
        [Column("ServerName")]
        public string ServerName { get; set; }
    }
}
