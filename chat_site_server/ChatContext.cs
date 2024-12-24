using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System;
using chat_site_server.Entities;


namespace chat_site_server
{
    internal class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring relationships for GroupMember
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId);

            // Configuring relationships for Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Add configuration for Receiver
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages) // Assuming you have `ReceivedMessages` in the User model
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade); // Prevent cascading delete


            modelBuilder.Entity<User>()
    .Property(u => u.Ip)
    .IsRequired(false); // Allow NULL values


            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            // هنا قم بإدخال معلومات الاتصال بقاعدة البيانات
            optionsBuilder.UseSqlServer("Server=DESKTOP-P6DGD12\\SQLEXPRESS;Database=chat_site_istemci;Trusted_Connection=true;TrustServerCertificate=True;");
        }

    }
}
