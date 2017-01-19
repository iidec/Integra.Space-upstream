namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_server_roles")]
    public partial class ServerRoleView
    {
        [Key]
        [Column("ServerRoleId", Order = 1)]
        public string ServerRoleId { get; set; }

        [Column("ServerRoleName")]
        public string ServerRoleName { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }
    }
}
