using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using chat_site_server.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using chat_site_server;

class ChatServer
{
    private TcpListener _tcpListener;
    private bool _isRunning;
    private Dictionary<string, TcpClient> _clients; // Store userId and corresponding TcpClient

    public ChatServer(string ipAddress, int port)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _clients = new Dictionary<string, TcpClient>(); // Initialize the dictionary
    }

    public void Start()
    {
        _isRunning = true;
        _tcpListener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        while (_isRunning)
        {
            try
            {
                var client = _tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        int bytesRead;

        string userId = null;

        try
        {
            // Read the userId sent by the client upon connection
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                userId = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"User {userId} connected.");

                if (!_clients.ContainsKey(userId))
                {
                    _clients.Add(userId, client);
                }
            }

            // Send missed messages to the user
            await SendMissedMessagesToUser(userId);

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessAndStoreMessage(message, userId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling client: " + ex.Message);
        }
        finally
        {
            if (userId != null && _clients.ContainsKey(userId))
            {
                _clients.Remove(userId);
                Console.WriteLine($"User {userId} disconnected.");
            }
            client.Close();
        }
    }

    private async Task SendMissedMessagesToUser(string userId)
    {
        using (var context = CreateDbContext())
        {
            var missedMessages = await context.Messages
                .Where(m => m.ReceiverId == userId && !m.Status)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            foreach (var message in missedMessages)
            {
                await SendMessageToPrivate(message);
            }
        }
    }

    private async Task ProcessAndStoreMessage(string receivedMessage, string senderId)
    {
        using (var context = CreateDbContext())
        {
            Message message = JsonConvert.DeserializeObject<Message>(receivedMessage);

            if (message.Type.Equals("Private", StringComparison.OrdinalIgnoreCase))
            {
                await SendMessageToPrivate(message);
            }
            else if (message.Type.Equals("Group", StringComparison.OrdinalIgnoreCase))
            {
                await SendMessageToGroup(message);
            }
            else if (message.Type.Equals("Broadcast", StringComparison.OrdinalIgnoreCase))
            {
                await BroadcastMessage(message.MessageContent);
            }
        }
    }

    private async Task SendMessageToPrivate(Message message)
    {
        using (var context = CreateDbContext())
        {
            string receiverId = message.ReceiverId;
            if (_clients.ContainsKey(receiverId))
            {
                var targetClient = _clients[receiverId];
                var targetSocket = targetClient.Client;
                string messageJson = JsonConvert.SerializeObject(message);
                var responseBytes = Encoding.UTF8.GetBytes(messageJson);

                await targetSocket.SendAsync(responseBytes);

                var deliveredMessage = await context.Messages.FirstOrDefaultAsync(m => m.ReceiverId == receiverId);
                if (deliveredMessage != null)
                {
                    context.Messages.Remove(deliveredMessage);
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                try
                {
                    await context.Messages.AddAsync(message);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error saving user: {ex.InnerException?.Message}");
                    throw;
                }

            }
        }

    }

    private async Task SendMessageToGroup(Message message)
    {
        using (var context = CreateDbContext())
        {
            var groupMembers = await context.GroupMembers
                .Where(gm => gm.GroupId == Guid.Parse(message.GroupId) && gm.UserId.ToString() != message.SenderId)
                .ToListAsync();

            foreach (var member in groupMembers)
            {
                message.ReceiverId = member.UserId.ToString();
                message.MessageId = new Guid();
                await SendMessageToPrivate(message);
            }
        }
    }

    private async Task BroadcastMessage(string messageContent)
    {
        foreach (var client in _clients.Values)
        {
            var targetSocket = client.Client;
            var responseBytes = Encoding.UTF8.GetBytes(messageContent);
            await targetSocket.SendAsync(responseBytes);
        }
    }

    private ChatContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-P6DGD12\\SQLEXPRESS;Database=chat_site_istemci;Trusted_Connection=True;TrustServerCertificate=True;");
        return new ChatContext(optionsBuilder.Options);
    }

    public void Stop()
    {
        _isRunning = false;
        _tcpListener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
