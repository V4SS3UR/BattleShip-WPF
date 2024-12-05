using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ChatServer
{
    private const int Port = 5000;
    private static readonly List<TcpClient> Clients = new List<TcpClient>();

    public static async Task StartServer()
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();
        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        lock (Clients) { Clients.Add(client); }
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (true)
            {
                string message = await reader.ReadLineAsync();
                if (message == null) break;

                Console.WriteLine($"Received: {message}");
                BroadcastMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            lock (Clients) { Clients.Remove(client); }
            client.Close();
        }
    }

    private static void BroadcastMessage(string message)
    {
        lock (Clients)
        {
            foreach (var client in Clients)
            {
                try
                {
                    var writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    writer.WriteLine(message); // Broadcast the message to all clients
                }
                catch
                {
                    // Handle errors (e.g., disconnected clients)
                }
            }
        }
    }
}
