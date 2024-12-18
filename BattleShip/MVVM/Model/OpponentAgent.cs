using BattleShipServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WPF_App.Core;

namespace BattleShip.MVVM.Model
{
    internal class OpponentAgent
    {
        private static Random _random = new Random();

        public event Action<double[,]> ProbabilityMapUpdated;

        private Client _client;
        private Player _opponentPlayer;
        private BattleGrid _battleGrid;

        // Random random seed

        private bool _isMyTurn;

        private double[,] _probabilityMap;

        public OpponentAgent()
        {
            _opponentPlayer = new Player("Opponent");
            _battleGrid = new BattleGrid();

            _probabilityMap = new double[10, 10];
                        
            _client = new Client();
            _client.ShipPlacing += _client_ShipPlacing;
            _client.NewTurn += _client_NewTurn;
            _client.Hit += _client_Hit;
            _client.Sunk += _client_Sunk;
            _client.Miss += _client_Miss;
            _client.Winned += Client_Winned;
            _client.Lost += Client_Lost;
        } 

        public Client GetClient() => this._client;
        public Player GetPlayer() => this._opponentPlayer;

        public async Task ConnectToServer()
        {
            try
            {
                await _client.StartClient(Network.GetLocalIPAddress());
            }
            catch (System.Exception ex)
            {
                // Handle connection errors
            }
        }

        private void _client_ShipPlacing()
        {            
            // Place ships randomly
            foreach (var shipSize in _opponentPlayer.ShipRules)
            {
                Ship ship = new Ship(shipSize);
                bool isHorizontal = _random.Next(2) == 0;
                int x = _random.Next(0, 9);
                int y = _random.Next(0, 9);
                while (!_opponentPlayer.PlaceShip(ship, x, y, isHorizontal))
                {
                    isHorizontal = _random.Next(2) == 0;
                    x = _random.Next(10);
                    y = _random.Next(10);
                }

                _client.PlaceShip(x, y, isHorizontal, shipSize);
            }
        }
        private void _client_NewTurn(bool myTurn)
        {
            _isMyTurn = myTurn;
            _opponentPlayer.State = myTurn ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;

            // If it's my turn => I play
            if (myTurn)
            {
                ThinkAndMove();
            }
        }
        private void _client_Hit(int x, int y)
        {
            if (!_isMyTurn) return;

            _battleGrid.Cells[x, y].Status = CellStatus.Hit;

            UpdateProbabilityMap(); // Recalculate probabilities based on the new hit

            ThinkAndMove(); // Continue to play
        }
        private void _client_Sunk(int x, int y, bool isHorizontal, int size)
        {
            if(!_isMyTurn) return;

            for (int i = 0; i < size; i++)
            {
                if (isHorizontal)
                {
                    _battleGrid.Cells[x + i, y].Status = CellStatus.Ship;                    
                }
                else
                {
                     _battleGrid.Cells[x, y + i].Status = CellStatus.Ship;
                }
            }

            UpdateProbabilityMap(); // Recalculate probabilities based on the sunk ship

            ThinkAndMove(); // Continue to play
        }
        private void _client_Miss(int x, int y)
        {
            if (!_isMyTurn) return;

            _battleGrid.Cells[x, y].Status = CellStatus.Missed;       

            UpdateProbabilityMap(); // Recalculate probabilities based on the missed shot
        }
        private void Client_Winned()
        {
            ResetGame();
        }
        private void Client_Lost()
        {
            ResetGame();
        }

        private void ResetGame()
        {
            _battleGrid = new BattleGrid();
            _opponentPlayer.Reset();
            Array.Clear(_probabilityMap, 0, _probabilityMap.Length);
            _client.RequestNewGame();
        }



        internal void ThinkAndMove()
        {
            System.Threading.Thread.Sleep(300);

            // Recalculate probabilities if necessary
            if (_probabilityMap.Cast<double>().Sum() == 0)
            {
                UpdateProbabilityMap();
            }

            var bestCell = FindBestCell();
            if (bestCell != null)
            {
                FireShot(bestCell);
                return;
            }

            // Fall back to random shots
            FireShot(FindRandomCell());
        }

        private Cell FindRandomCell()
        {
            int x, y;

            // Fire only even cells
            do
            {
                x = _random.Next(0, 9);
                y = _random.Next(0, 9);
            } while (_battleGrid.Cells[x, y].Status != CellStatus.Empty || (x + y) % 2 != 0);

            return _battleGrid.Cells[x, y];
        }

        private void UpdateProbabilityMap()
        {
            Array.Clear(_probabilityMap, 0, _probabilityMap.Length);

            // Track all hit cells
            var hitCells = _battleGrid.GetCells().Where(c => c.Status == CellStatus.Hit);

            // Adjust probabilities based on ship placements
            foreach (int shipSize in _opponentPlayer.ShipRules)
            {
                int useSize = shipSize - 1;

                for (int row = 0; row < 10; row++)
                {
                    for (int col = 0; col < 10; col++)
                    {
                        if (_battleGrid.Cells[row, col].Status != CellStatus.Empty) continue;

                        // Horizontal placement
                        if (col + useSize < 10 && CanPlaceShip(row, col, true, useSize))
                        {
                            for (int i = 0; i <= useSize; i++)
                            {
                                _probabilityMap[row, col + i]++;
                            }
                        }

                        // Vertical placement
                        if (row + useSize < 10 && CanPlaceShip(row, col, false, useSize))
                        {
                            for (int i = 0; i <= useSize; i++)
                            {
                                _probabilityMap[row + i, col]++;
                            }
                        }
                    }
                }
            }

            // Enhance probabilities near hits
            foreach (var hit in hitCells)
            {
                // Check for aligned hits and prioritize cells along the same direction
                var alignedHits = GetAlignedHits(hit.X, hit.Y);

                if (alignedHits.Count > 1)
                {
                    // We have a line of hits, increase probabilities at the ends of the line
                    var lineEnds = GetLineEnds(alignedHits);
                    foreach (var end in lineEnds)
                    {
                        if (_battleGrid.Cells[end.X, end.Y].Status == CellStatus.Empty)
                        {
                            _probabilityMap[end.X, end.Y] += 40; // Higher weight for completing the line
                        }                        
                    }
                }
                else
                {
                    // Single hit: increase probability around it
                    var neighbors = GetNeighbors(hit.X, hit.Y);
                    foreach (var neighbor in neighbors)
                    {
                        if (_battleGrid.Cells[neighbor.X, neighbor.Y].Status == CellStatus.Empty)
                        {
                            _probabilityMap[neighbor.X, neighbor.Y] += 30; // Moderate weight for neighbors
                        }
                    }
                }
            }

            ProbabilityMapUpdated?.Invoke(_probabilityMap);
        }
        private List<Cell> GetAlignedHits(int x, int y)
        {
            var alignedHits = new List<Cell> { _battleGrid.Cells[x, y] };

            // Check in all directions (horizontal and vertical)
            var directions = new List<(int dx, int dy)> { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dx, dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;

                while (!IsOutOfBounds(newX, newY) && _battleGrid.Cells[newX, newY].Status == CellStatus.Hit)
                {
                    alignedHits.Add(_battleGrid.Cells[newX, newY]);
                    newX += dx;
                    newY += dy;
                }
            }

            return alignedHits;
        }
        private List<Cell> GetLineEnds(List<Cell> alignedHits)
        {
            var sortedHits = alignedHits.OrderBy(c => c.X).ThenBy(c => c.Y).ToList();
            var start = sortedHits.First();
            var end = sortedHits.Last();

            // Determine direction (horizontal or vertical)
            bool isHorizontal = start.Y != end.Y;

            // Calculate line ends
            var lineEnds = new List<(int X, int Y)>
            {
                isHorizontal ? (start.X, start.Y - 1) : (start.X - 1, start.Y),
                isHorizontal ? (end.X, end.Y + 1) : (end.X + 1, end.Y)
            };

            // Filter out of bounds cells
            var validEnds = lineEnds.Where(p => !IsOutOfBounds(p.X, p.Y));
            return validEnds.Select(p => _battleGrid.Cells[p.X, p.Y]).ToList();
        }
        private List<Cell> GetNeighbors(int x, int y)
        {
            var directions = new List<(int dx, int dy)> { (-1, 0), (1, 0), (0, -1), (0, 1) };
            var neighbors = new List<Cell>();

            foreach (var (dx, dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (!IsOutOfBounds(newX, newY))
                {
                    neighbors.Add(_battleGrid.Cells[newX, newY]);
                }
            }

            return neighbors;
        }


        private bool CanPlaceShip(int row, int col, bool isHorizontal, int size)
        {
            for (int i = 0; i <= size; i++)
            {
                int checkRow = isHorizontal ? row : row + i;
                int checkCol = isHorizontal ? col + i : col;

                if (checkRow >= 10 || checkCol >= 10 || _battleGrid.Cells[checkRow, checkCol].Status != CellStatus.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        private Cell FindBestCell()
        {
            double maxProbability = 0;
            Cell bestCell = null;

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    if (_battleGrid.Cells[row, col].Status == CellStatus.Empty && _probabilityMap[row, col] > maxProbability)
                    {
                        maxProbability = _probabilityMap[row, col];
                        bestCell = _battleGrid.Cells[row, col];
                    }
                }
            }

            return bestCell;
        }

        private bool IsOutOfBounds(int x, int y)
        {
            return x < 0 || x >= 10 || y < 0 || y >= 10;
        }

        private void FireShot(Cell cell)
        {
            _client.FireShot(cell.X, cell.Y);
        }
    }
}
