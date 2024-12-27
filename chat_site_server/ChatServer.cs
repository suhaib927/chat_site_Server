﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using chat_site_server;
using chat_site_server.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

class ChatServer
{
    private TcpListener _tcpListener;
    private bool _isRunning;
    private Dictionary<string, TcpClient> _clients; // Store userId and corresponding TcpClient
    private static ChatContext _context;  // Define the static variable for the DbContext

    public ChatServer(string ipAddress, int port)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _clients = new Dictionary<string, TcpClient>(); // Initialize the dictionary

        // Initialize the static context variable
        if (_context == null)
        {
            var options = new DbContextOptionsBuilder<ChatContext>()
                            .UseSqlServer("Server=your_server_address;Database=your_database_name;Trusted_Connection=True;")
                            .Options;
            _context = new ChatContext(options);
        }
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
                // Accept client connection
                var client = _tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                // Handle the client in a new task (instead of using threads directly)
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

                // Add the client to the dictionary with the userId
                if (!_clients.ContainsKey(userId))
                {
                    _clients.Add(userId, client);
                }
            }

            // Send missed messages to the user
            await SendMissedMessagesToUser(userId);

            // Listen for messages from the client
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Process and store the message, and send it to the receiver
                await ProcessAndStoreMessage(message, userId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling client: " + ex.Message);
        }
        finally
        {
            // Remove client from the dictionary and close connection
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
        // Now using the static _context variable to access the database
        var missedMessages = await _context.Messages
            .Where(m => m.ReceiverId ==userId && m.Status == false)
            .ToListAsync();

        foreach (var message in missedMessages)
        {
            await SendMessageToPrivate(message);
        }
    }

    private async Task ProcessAndStoreMessage(string receivedMessage, string senderId)
    {
        // تحليل الرسالة بناءً على الفاصل |
        Message message = JsonConvert.DeserializeObject<Message>(receivedMessage);


        // إرسال الرسالة بناءً على نوعها
        if (message.Type.Equals("Private", StringComparison.OrdinalIgnoreCase))
        {
            // رسالة فردية (إرسال إلى شخص واحد)
            await SendMessageToPrivate(message);
        }
        else if (message.Type.Equals("Group", StringComparison.OrdinalIgnoreCase))
        {
            // رسالة لمجموعة
            await SendMessageToGroup(message);
        }
        else if (message.Type.Equals("Broadcast", StringComparison.OrdinalIgnoreCase))
        {
            // رسالة للجميع
            await BroadcastMessage(message.MessageContent);
        }
        else
        {
            Console.WriteLine("Unknown message type.");
        }
        
    }

    private async Task SendMessageToPrivate(Message message)
    {
        string receiverId = message.ReceiverId.ToString();
        if (_clients.ContainsKey(receiverId))
        {
            var targetClient = _clients[receiverId];
            var targetSocket = targetClient.Client;  
            string messageJson = JsonConvert.SerializeObject(message);
            var responseBytes = Encoding.UTF8.GetBytes(messageJson);

            await targetSocket.SendAsync(responseBytes);

            var deliveredMessage = await _context.Messages.FirstOrDefaultAsync(m => m.ReceiverId == receiverId);
            if (deliveredMessage != null)
            {
                _context.Messages.Remove(deliveredMessage); 
                await _context.SaveChangesAsync();
            }
        } else
        {
            try
            {
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving user: {ex.InnerException?.Message}");
                throw;
            }
        }
    }


    private async Task SendMessageToGroup(Message message)
    {
        // استرجاع جميع الأعضاء في المجموعة
        var groupMembers = await _context.GroupMembers
        .Where(gm => gm.GroupId == Guid.Parse(message.GroupId) && gm.UserId != message.SenderId)
        .ToListAsync();

        foreach (var groupMember in groupMembers)
        {
            Guid memberId = groupMember.UserId;
            message.ReceiverId = memberId.ToString();
            SendMessageToPrivate(message);
        }
    }

    private async Task BroadcastMessage(string messageContent)
    {
        foreach (var client in _clients.Values)
        {
            var targetSocket = client.Client;
            var responseBytes = Encoding.UTF8.GetBytes(messageContent);
            await targetSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _tcpListener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
