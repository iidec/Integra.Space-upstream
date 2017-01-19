namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_logins")]
    public partial class LoginView
    {
        [Key]
        [Column("LoginId", Order = 1)]
        public string LoginId { get; set; }

        [Column("LoginName")]
        public string LoginName { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Column("DefaultDatabaseId")]
        public string DefaultDatabaseId { get; set; }

        [Column("DefaultDatabaseServerId")]
        public string DefaultDatabaseServerId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
