using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer
{
    public class Server
    {
        private const int Port = 5000;
        private List<PlayerConnection> playerConnections = new List<PlayerConnection>();
        private BattleShipGame game;

        public async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            while (true)
            {
                // Wait for two players to connect
                if (playerConnections.Count < 2)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    var playerConnection = new PlayerConnection(client);
                    playerConnections.Add(playerConnection);
                    _ = HandleClientAsync(playerConnection);

                    playerConnection.SendMessage("[Message] (Server) Player connected. Waiting for other player...");
                }

                // Start a new game if two players are connected and there is no game
                if (playerConnections.Count == 2 && game == null)
                {
                    // Start a new game
                    game = new BattleShipGame(playerConnections[0].Player, playerConnections[1].Player);
                    game.gameState = GameState.PlacingShips;

                    BroadcastMessage("[ShipPlacing]");
                    BroadcastMessage("[Message] (Server) Both players connected. Place your ships.");
                }

                // If it's a new game, reset the game state
                if (game != null && game.gameState == GameState.Ending && 
                    playerConnections[0].Player.State == PlayerState.Idle && 
                    playerConnections[1].Player.State == PlayerState.Idle)
                {
                    game = null;
                }

                await Task.Delay(100); // Avoid tight loop, give CPU breathing room
            }
        }

        private async Task HandleClientAsync(PlayerConnection playerConnection)
        {
            while (true)
            {
                int timeoutMilliseconds = 3000;

                try
                {
                    // Detect if client disconnected
                    if (playerConnection.TcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (playerConnection.TcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            playerConnection.Close();
                            break;
                        }
                    }

                    string message = await playerConnection.ReadLineAsync();
                    await GameLoop(playerConnection, message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    playerConnection.Close();
                    playerConnections.Remove(playerConnection);
                    break;
                }
            }
        }

        // Game loop that handles the game state
        private async Task GameLoop(PlayerConnection playerConnection, string action)
        {
            // If action is empty
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            // If action is message
            if (action.Contains("[Message]"))
            {
                string message = action.Substring(9);

                playerConnections.ForEach(p => SendMessage(p, $"[Message] ({(p == playerConnection ? "Player" : "Opponent")}) {message}"));
                return;
            }

            switch (game.gameState)
            {
                case GameState.PlacingShips:
                    if(playerConnection.Player.State == PlayerState.ShipsPlaced)
                    {
                        return;
                    }

                    await TryPlaceShip(playerConnection, action);

                    //Check if player has placed all ships
                    if(playerConnection.Player.AreAllShipsPlaced())
                    {
                        playerConnection.Player.State = PlayerState.ShipsPlaced;
                        BroadcastMessage($"[Message] (Server) A player has placed all ships.");
                    }
                    
                    if(playerConnection.Player.State == PlayerState.ShipsPlaced)
                    {
                        // Check if all players are ready to start the game
                        if (game.ArePlayerReady() && game.gameState != GameState.Playing)
                        {
                            game.gameState = GameState.Playing;
                            BroadcastMessage("[GameStart]");
                            BroadcastMessage("[Message] (Server) The game has started!");

                            // Notify the first player that it's their turn or not
                            playerConnections[0].Player.State = game.CurrentPlayer == playerConnections[0].Player ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;
                            playerConnections[0].SendMessage($"[NewTurn] {(game.CurrentPlayer == playerConnections[0].Player ? "true" : "false")}");

                            playerConnections[1].Player.State = game.CurrentPlayer == playerConnections[1].Player ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;
                            playerConnections[1].SendMessage($"[NewTurn] {(game.CurrentPlayer == playerConnections[1].Player ? "true" : "false")}");
                        }
                    }
                    
                    break;

                case GameState.Playing:
                    if (playerConnection.Player.State == PlayerState.TakingTurn)
                    {
                        await TryHit(playerConnection, action);
                    }
                    break;

                case GameState.Ending:
                    if (action.Contains("[NewGame]"))
                    {
                        playerConnection.Player.Reset();

                        playerConnection.SendMessage("[NewGame]");
                        BroadcastMessage("[Message] (Server) A player has requested a new game.");
                    }
                    break;
            }
        }

        private async Task TryPlaceShip(PlayerConnection playerConnection, string action)
        {
            var player = playerConnection.Player;

            if (player.State == PlayerState.ShipsPlaced) return;        

            // [PlaceShip] X Y Direction Size
            if (action.Contains("[PlaceShip]"))
            {
                string[] parts = action.Split(' ');
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                bool isHorizontal = parts[3] == "H";
                int size = int.Parse(parts[4]);

                if (game.PlaceShip(player, x, y, isHorizontal, size))
                {
                    Console.WriteLine($"Player {player.Name} placed ship at {x}, {y} with size {size}");
                    SendMessage(playerConnection, $"[ShipPlaced] {x} {y} {(isHorizontal ? "H" : "V")} {size}");
                }
                else
                {
                    Console.WriteLine($"Player {player.Name} tried to place ship at {x}, {y} with size {size} but it was invalid");
                    SendMessage(playerConnection, "[InvalidShipPlacement]");
                }
            }
        }

        private async Task TryHit(PlayerConnection playerConnection, string action)
        {
            // [FireShot] X Y
            if (action.Contains("[FireShot]"))
            {
                string[] parts = action.Split(' ');
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                if (game.FireShot(x, y))
                {
                    Console.WriteLine($"Player {playerConnection.Player.Name} hit at {x}, {y}");
                    BroadcastMessage($"[Hit] {x} {y}");

                    // Check if the ship of opponent is sunk
                    var cell = game.Opponent.Grid.GetCell(x, y);
                    var opponentShip = game.Opponent.Ships.Find(s => s.Cells.Contains(cell));

                    if (opponentShip.IsSunk())
                    {
                        var shipFirstCell = opponentShip.Cells[0];
                        var isHorizontal = opponentShip.Cells[1].Y == shipFirstCell.Y;

                        Console.WriteLine($"Player {playerConnection.Player.Name} sunk a ship");
                        BroadcastMessage($"[Sunk] {shipFirstCell.X} {shipFirstCell.Y} {(isHorizontal ? "H" : "V")} {opponentShip.Size}");

                        // Check if all ships are sunk
                        if (game.Opponent.IsAllShipsSunk())
                        {
                            Console.WriteLine($"Player {playerConnection.Player.Name} won the game!");

                            var opponentConnection = playerConnections.Find(p => p.Player != playerConnection.Player);

                            playerConnection.SendMessage("[Winned]");
                            opponentConnection.SendMessage("[Lost]");

                            game.gameState = GameState.Ending;

                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Player {playerConnection.Player.Name} missed at {x}, {y}");
                    BroadcastMessage($"[Miss] {x} {y}");

                    game.SwitchTurn();

                    playerConnections[0].SendMessage($"[NewTurn] {(game.CurrentPlayer == playerConnections[0].Player ? "true" : "false")}");
                    playerConnections[1].SendMessage($"[NewTurn] {(game.CurrentPlayer == playerConnections[1].Player ? "true" : "false")}");
                }                
            }
        }

        private void BroadcastMessage(string message)
        {
            foreach (var playerConnection in playerConnections)
            {
                SendMessage(playerConnection, message);
            }
        }

        private void SendMessage(PlayerConnection playerConnection, string message)
        {
            var writer = new StreamWriter(playerConnection.TcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
            writer.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
