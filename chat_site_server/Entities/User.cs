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

        public string Ip { get; set; } = "000.000.000.000";


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
        public ICollection<Message> SentMessages { get; set; } // Messages sent by the user
        public ICollection<Message> ReceivedMessages{ get; set; }
        
    }
}
