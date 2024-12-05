using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageApp.Core
{
    class MessageClient
    {
        private const string ServerAddress = "127.0.0.1"; // Localhost
        private const int Port = 5000;

        public static async Task StartClient()
        {
            TcpClient client = new TcpClient();

            try
            {
                await client.ConnectAsync(ServerAddress, Port);
                Console.WriteLine("Connected to server.");
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                _ = ReceiveMessagesAsync(reader);

                while (true)
                {
                    string message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message)) break;

                    await writer.WriteLineAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Disconnected from server.");
            }
        }

        private static async Task ReceiveMessagesAsync(StreamReader reader)
        {
            try
            {
                while (true)
                {
                    string response = await reader.ReadLineAsync();
                    if (response == null) break;

                    Console.WriteLine($"Server: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving messages: {ex.Message}");
            }
        }
    } 
}
