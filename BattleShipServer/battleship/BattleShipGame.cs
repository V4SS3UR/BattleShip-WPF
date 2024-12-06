using System;

namespace BattleShipServer
{
    public class BattleShipGame
    {
        private Player Player1 { get; }
        private Player Player2 { get; }
        public Player CurrentPlayer { get; set; }
        public Player Opponent { get; set; }

        public GameState gameState { get; set; }

        public BattleShipGame(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
            CurrentPlayer = Player1;
            Opponent = Player2;
        }

        public bool PlaceShip(Player player, int x, int y, bool isHorizontal, int size)
        {
            Ship ship = new Ship(size);
            if (player.PlaceShip(ship, x, y, isHorizontal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ArePlayerReady()
        {
            return Player1.State == PlayerState.ShipsPlaced && Player2.State == PlayerState.ShipsPlaced;
        }

        public bool FireShot(int x, int y)
        {
            return Opponent.GetFireAt(x, y);
        }

        public void SwitchTurn()
        {
            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
                Opponent = Player1;
            }
            else
            {
                CurrentPlayer = Player1;
                Opponent = Player2;
            }

            // Update state
            Player1.State = CurrentPlayer == Player1 ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;
            Player2.State = CurrentPlayer == Player2 ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;

            Console.WriteLine($"Current player: {CurrentPlayer.Name}");
        }


    }
}