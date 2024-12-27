using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace chat_site_server
{
    public class ChatContextFactory : IDesignTimeDbContextFactory<ChatContext>
    {
        public ChatContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-P6DGD12\\SQLEXPRESS;Database=chat_site_istemci;Trusted_Connection=True;TrustServerCertificate=True;");

            return new ChatContext(optionsBuilder.Options);
        }
    }
}
