using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using chat_site_server.Entities;


namespace chat_site_server.Entities
{
    [Table("Users")]
    public class User 
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(30)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        public bool IsOnline { get; set; }=false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? ProfileImageFileName { get; set; } = "images.jpg";

        public ICollection<GroupMember> GroupMemberships { get; set; } // Groups the user belongs to

    }
}
