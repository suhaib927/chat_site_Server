using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace chat_site_server.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }

        public string? SenderId { get; set; }

        public string? ReceiverId { get; set; }

        public string? GroupId { get; set; }


        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public string Type { get; set; }

        public bool Status { get; set; } = false; // Delivered or not

    }
}
