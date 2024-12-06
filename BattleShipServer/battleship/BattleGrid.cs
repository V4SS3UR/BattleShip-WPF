using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleShipServer
{
    public class BattleGrid
    {
        public Cell[,] Cells { get; set; }
        private int _size;

        public BattleGrid(int size = 10)
        {
            _size = size;
            Cells = new Cell[size, size];

            // Initialize all cells in the grid
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    Cells[i, j] = new Cell(i, j);
                }
            }
        }

        //Get cell
        public Cell GetCell(int x, int y)
        {
            return Cells[x, y];
        }

        // Add a ship to the grid at a specific position with orientation (horizontal or vertical)
        public bool PlaceShip(Ship ship, int x, int y, bool isHorizontal)
        {
            if (isHorizontal)
            {
                // Check if ship fits in the grid
                if (x + ship.Size > _size) return false;

                // Check if cells are empty
                for (int i = 0; i < ship.Size; i++)
                {
                    if (Cells[x + i, y].Status != CellStatus.Empty)
                        return false;
                }

                // Place ship in cells
                for (int i = 0; i < ship.Size; i++)
                {
                    Cells[x + i, y].Status = CellStatus.Ship;
                    ship.Cells.Add(Cells[x + i, y]);
                }
            }
            else
            {
                // Check if ship fits in the grid
                if (y + ship.Size > _size) return false;

                // Check if cells are empty
                for (int i = 0; i < ship.Size; i++)
                {
                    if (Cells[x, y + i].Status != CellStatus.Empty)
                        return false;
                }

                // Place ship in cells
                for (int i = 0; i < ship.Size; i++)
                {
                    Cells[x, y + i].Status = CellStatus.Ship;
                    ship.Cells.Add(Cells[x, y + i]);
                }
            }

            return true;
        }

        //Remove ship
        public void RemoveShip(Ship ship)
        {
            foreach (var cell in ship.Cells)
            {
                cell.Status = CellStatus.Empty;
            }
            ship.Cells.Clear();
        }

        // Fire a shot at a specific location
        public bool GetFireShot(int x, int y)
        {
            if (Cells[x, y].Status == CellStatus.Ship)
            {
                Cells[x, y].Status = CellStatus.Hit;
                return true; // Hit
            }
            else
            {
                Cells[x, y].Status = CellStatus.Missed;
                return false; // Missed
            }
        }

        // Reset the grid
        internal void Reset()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    Cells[i, j].Status = CellStatus.Empty;
                }
            }
        }
    }
}
