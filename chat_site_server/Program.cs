class Program
{
    static void Main(string[] args)
    {
        ChatServer server = new ChatServer("127.0.0.1", 5000);
        server.Start();

        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();
        server.Stop();
    }
}
