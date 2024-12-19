using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace chat_site_server.Entities
{
    [Table("FailedMessages")]
    public class FailedMessage
    {
        [Key]
        public Guid FailedMessageId { get; set; }

        [ForeignKey(nameof(Message))]
        public Guid MessageId { get; set; }
        public Message Message { get; set; } // Reference to the original message

        public string FailureReason { get; set; }
        public DateTime FailedAt { get; set; } = DateTime.Now;
    }
}
