using chat_site_server.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace chat_site_server.Entities
{
    [Table("GroupMembers")]
    public class GroupMember
    {
        [Key]
        public Guid GroupMemberId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(Group))]
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}
