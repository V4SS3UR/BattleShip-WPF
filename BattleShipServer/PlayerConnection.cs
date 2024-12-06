using BattleShipServer;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PlayerConnection
{
    public TcpClient TcpClient { get; private set; }
    public Player Player { get; private set; }
    public bool IsReleased { get; set; }

    private NetworkStream Stream { get; set; }
    private StreamReader Reader { get; set; }
    private StreamWriter Writer { get; set; }

    public PlayerConnection(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        Player = new Player(Guid.NewGuid().ToString());
        Stream = tcpClient.GetStream();
        Reader = new StreamReader(Stream, Encoding.UTF8);
        Writer = new StreamWriter(Stream, Encoding.UTF8) { AutoFlush = true };
    }

    public async Task<string> ReadLineAsync()
    {
        return await Reader.ReadLineAsync();
    }
    public async Task<string> ReadLineWithTimeoutAsync(int timeoutMilliseconds)
    {
        var readLineTask = Reader.ReadLineAsync();
        var timeoutTask = Task.Delay(timeoutMilliseconds);

        var completedTask = await Task.WhenAny(readLineTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("ReadLineAsync timed out.");
        }

        // Ensure to return the result of the completed ReadLineAsync task
        return await readLineTask;
    }

    public void SendMessage(string message)
    {
        Writer.WriteLine(message);
    }

    public void Close()
    {
        IsReleased = true;
        Reader.Dispose();
        Writer.Dispose();
        TcpClient.Close();
    }
}
