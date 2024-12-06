using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleShipServer
{
    public class Player
    {
        public event Action<PlayerState> PlayerStateChanged;
        public event Action Hitted;
        public event Action Sunk;

        public string Name { get; set; }
        public BattleGrid Grid { get; set; }
        public List<Ship> Ships { get; set; }
        public PlayerState State { get; set; }

        private int[] _shipRules = new int[] { 2, 3, 3, 4, 5 };
        public int[] ShipRules => _shipRules;

        public Player(string name)
        {
            Name = name;
            Grid = new BattleGrid();
            Ships = new List<Ship>();
            State = PlayerState.Idle;
        }

        // Place ships on the grid
        public bool PlaceShip(Ship ship, int startX, int startY, bool isHorizontal)
        {
            bool isPlaced = Grid.PlaceShip(ship, startX, startY, isHorizontal);

            if (isPlaced)
            {
                Ships.Add(ship);

                if (AreAllShipsPlaced())
                {
                    State = PlayerState.ShipsPlaced;
                }
            }

            return isPlaced;
        }

        //Remove ship
        public void RemoveShip(Ship ship)
        {
            Grid.RemoveShip(ship);
            Ships.Remove(ship);
        }

        // Get Fire
        public bool GetFireAt(int x, int y)
        {
            return Grid.GetFireShot(x, y);
        }

        // Check if all ships are sunk
        public bool IsAllShipsSunk()
        {
            return Ships.All(ship => ship.IsSunk());
        }

        // Check if all ships are placed
        public bool AreAllShipsPlaced()
        {
            var sizes = _shipRules.Distinct();
            return sizes.All(size => Ships.Count(ship => ship.Size == size) == _shipRules.Count(s => s == size));
        }

        // Reset player
        public void Reset()
        {
            Ships.Clear();
            Grid.Reset();
            State = PlayerState.Idle;
        }
    }
}
