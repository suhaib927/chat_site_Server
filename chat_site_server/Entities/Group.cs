using System.ComponentModel.DataAnnotations;
using chat_site_server.Entities;

namespace chat_site_server.Entities
{
    public class Group
    {
        [Key]
        public Guid GroupId { get; set; }
        [Required]
        [StringLength(50)]
        public string GroupName { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string GroupImageUrl { get; set; } = "images.jpg";
        public int MaxMembers { get; set; }

        public ICollection<GroupMember> Members { get; set; } // Members of the group

    }
}
