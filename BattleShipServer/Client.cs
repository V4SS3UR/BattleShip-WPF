using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BattleShipServer
{
    public class Client
    {
        // [PlaceShip] X Y Direction Size
        //      [ShipPlaced]
        //      [InvalidShipPlacement]

        // [FireShot] X Y
        //      [Hit] X Y
        //      [Sunk] X Y Direction Size
        //      [Miss] X Y

        // [GameStart]
        // [NewTurn]
        // [Winned]
        // [Lost]
        // [NewGame]

        // [Message] (Sender) Message

        public event Action GameStart;
        public event Action Winned;
        public event Action Lost;
        public event Action NewGame;
        public event Action<bool> NewTurn;
        public event Action ShipPlacing;
        public event Action<int, int, bool, int> ShipPlaced;
        public event Action InvalidShipPlacement;
        public event Action<int, int> Hit;
        public event Action<int, int, bool, int> Sunk;
        public event Action<int, int> Miss;
        public event Action<string, MessageSenderType> MessageReceived;

        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private const int Port = 5000;

        public async Task StartClient(string ip)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ip, Port);
                Console.WriteLine("Connected to the server!");

                _reader = new StreamReader(_client.GetStream(), Encoding.UTF8);
                _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };

                // Listen for messages from the server
                _ = Task.Run(() => ListenForMessages());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw ex;
            }
        }

        private async Task ListenForMessages()
        {
            while (true)
            {
                try
                {
                    string message = await _reader.ReadLineAsync();
                    ProcessMessages(message);
                }
                catch(Exception ex) 
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Disconnected from the server.");
                    break;
                }
            }
        }

        // Process messages from the server
        public void ProcessMessages(string action)
        {
            if (action == null) return;

            if (action.Contains("[Message]"))
            {
                // [Message] (PlayerName) Message...

                bool isMine = action.Replace("[Message] ", "").StartsWith("(Player)");
                bool isServer = action.Replace("[Message] ", "").StartsWith("(Server)");
                string message = action.Substring(action.IndexOf(')') + 2); // Remove "... (PlayerName) "
                MessageSenderType senderType = isMine ? MessageSenderType.Player : isServer ? MessageSenderType.Server : MessageSenderType.Opponent;
                MessageReceived?.Invoke(message, senderType);
                return;
            }

            string[] parts = action.Split(' ');
            string command = parts[0];
            
            if (command.Contains("[ShipPlacing]"))
            {
                ShipPlacing?.Invoke();
            }
            else if (command.Contains("[ShipPlaced]"))
            {
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                bool isHorizontal = parts[3] == "H";
                int size = int.Parse(parts[4]);

                ShipPlaced?.Invoke(x, y, isHorizontal, size);
            }
            else if (command.Contains("[InvalidShipPlacement]"))
            {
                InvalidShipPlacement?.Invoke();
            }
            else if (command.Contains("[Hit]"))
            {
                // [Hit] X Y
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                Hit?.Invoke(x, y);
            }
            else if (command.Contains("[Sunk]"))
            {
                // [Sunk] X Y
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                bool isHorizontal = parts[3] == "H";
                int size = int.Parse(parts[4]);

                Sunk?.Invoke(x, y, isHorizontal, size);
            }
            else if (command.Contains("[Miss]"))
            {
                // [Miss] X Y
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                Miss?.Invoke(x, y);
            }
            else if (command.Contains("[GameStart]"))
            {
                GameStart?.Invoke();
            }
            else if (command.Contains("[NewTurn]"))
            {
                // Try parse 1st argument as bool
                bool.TryParse(parts[1], out bool myTurn);

                NewTurn?.Invoke(myTurn);
            }
            else if (command.Contains("[Winned]"))
            {
                Winned?.Invoke();
            }
            else if (command.Contains("[Lost]"))
            {
                Lost?.Invoke();
            }
            else if (command.Contains("[NewGame]"))
            {
                NewGame?.Invoke();
            }
        }

        // [PlaceShip] X Y Direction Size
        public void PlaceShip(int x, int y, bool isHorizontal, int size)
        {
            SendMessage($"[PlaceShip] {x} {y} {(isHorizontal ? "H" : "V")} {size}");
        }

        // [FireShot] X Y
        public void FireShot(int x, int y)
        {
            SendMessage($"[FireShot] {x} {y}");
        }

        // [NewGame]
        public void RequestNewGame()
        {
            SendMessage("[NewGame]");
        }

        public void SendChatMessage(string message)
        {
            SendMessage($"[Message] {message}");
        }

        private void SendMessage(string message)
        {
            _writer?.WriteLine(message);            
        }

        public void Close()
        {
            _reader.Dispose();
            _writer.Dispose();
            _client.Close();
        }
    }
}
