using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ChatClient
{
    private TcpClient _client;
    private StreamReader _reader;
    private StreamWriter _writer;

    public string Username { get; set; } // Add Username property

    public event Action<string> MessageReceived; // Event for received messages
    public event Action<string> ConnectionStatusChanged; // Event for connection status updates

    public bool IsConnected => _client?.Connected ?? false;

    private const string ServerAddress = "127.0.0.1";
    private const int Port = 5000;

    public async Task ConnectAsync()
    {
        if (string.IsNullOrEmpty(Username))
        {
            ConnectionStatusChanged?.Invoke("Error: Username is required.");
            return;
        }

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ServerAddress, Port);

            _reader = new StreamReader(_client.GetStream(), Encoding.UTF8);
            _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };

            ConnectionStatusChanged?.Invoke("Connected to server.");
            _ = ListenForMessagesAsync(); // Start listening for messages
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (IsConnected && _writer != null)
        {
            try
            {
                // Include the username with the message
                string formattedMessage = $"{Username}: {message}";
                await _writer.WriteLineAsync(formattedMessage);
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke($"Error sending message: {ex.Message}");
            }
        }
        else
        {
            ConnectionStatusChanged?.Invoke("Not connected to the server.");
        }
    }

    private async Task ListenForMessagesAsync()
    {
        try
        {
            while (IsConnected)
            {
                string message = await _reader.ReadLineAsync();
                if (message == null) break;

                MessageReceived?.Invoke(message); // Trigger event for new messages
            }
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error receiving messages: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Close();
        ConnectionStatusChanged?.Invoke("Disconnected from server.");
    }
}
