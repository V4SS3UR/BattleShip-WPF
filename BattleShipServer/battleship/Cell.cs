namespace BattleShipServer
{
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public CellStatus Status { get; set; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Status = CellStatus.Empty;
        }
    }
}
