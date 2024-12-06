using System.Collections.Generic;
using System.Linq;

namespace BattleShipServer
{
    public class Ship
    {
        public int Size { get; set; }
        public List<Cell> Cells { get; set; } // List of cells occupied by this ship

        public Ship(int size)
        {
            Size = size;
            Cells = new List<Cell>();
        }

        public bool IsSunk()
        {
            return Cells.All(cell => cell.Status == CellStatus.Hit);
        }
    }
}
