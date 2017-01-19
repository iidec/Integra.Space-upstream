namespace Integra.Space.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("space.vw_users")]
    public partial class UserView
    {
        [Key]
        [Column("UserId", Order = 2)]
        public string UserId { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }

        [Key]
        [Column("DatabaseId", Order = 1)]
        public string DatabaseId { get; set; }

        [Key]
        [Column("ServerId", Order = 0)]
        public string ServerId { get; set; }

        [Column("DefaultSchemaId")]
        public string DefaultSchemaId { get; set; }

        [Column("DefaultSchemaDatabaseId")]
        public string DefaultSchemaDatabaseId { get; set; }

        [Column("DefaultSchemaServerId")]
        public string DefaultSchemaServerId { get; set; }

        [Column("LoginId")]
        public string LoginId { get; set; }

        [Column("LoginServerId")]
        public string LoginServerId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
