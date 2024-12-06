using BattleShipServer;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Shapes;
using WPF_App.Core;

namespace WPF_App.MVVM.ViewModel
{
    internal class BattleShip_ShipPlacement_ViewModel : ObservableObject
    {
        public event Action<int, int, bool, int> ShipPlaced;
        public event Action<Ship> ShipRemoved;
        public event Action PlayerReady;

        private bool _isHorizontal; public bool IsHorizontal
        {
            get => _isHorizontal;
            set { _isHorizontal = value; OnPropertyChanged(); }
        }
        private int _shipSize; public int ShipSize
        {
            get => _shipSize;
            set { _shipSize = value; OnPropertyChanged(); }
        }

        private int _TwoShipCount; public int TwoShipCount
        {
            get => _TwoShipCount;
            set { _TwoShipCount = value; OnPropertyChanged(); }
        }
        private int _ThreeShipCount; public int ThreeShipCount
        {
            get => _ThreeShipCount;
            set { _ThreeShipCount = value; OnPropertyChanged(); }
        }
        private int _FourShipCount; public int FourShipCount
        {
            get => _FourShipCount;
            set { _FourShipCount = value; OnPropertyChanged(); }
        }
        private int _FiveShipCount; public int FiveShipCount
        {
            get => _FiveShipCount;
            set { _FiveShipCount = value; OnPropertyChanged(); }
        }


        public ICommand ChangeShipSizeCommand { get; set; }
        public ICommand ConfirmShipPlacementCommand { get; set; }
        public ICommand RandomizedShipPlacementCommand { get; set; }

        private Client _client;
        private Player _player;
        private bool _isRandomizing;

        public BattleShip_ShipPlacement_ViewModel()
        {
            ShipSize = 2;
            IsHorizontal = true;

            ChangeShipSizeCommand = new RelayCommand(
                param =>
                {           
                    var str = param.ToString();
                    ShipSize = int.Parse(str[0].ToString());
                    IsHorizontal = str[1] == 'H';
                },
                _ => true);

            ConfirmShipPlacementCommand = new RelayCommand(
                _ => ConfirmShips(),
                _ => _player?.AreAllShipsPlaced() == true);
            RandomizedShipPlacementCommand = new RelayCommand(
                _ => RandomizedShipPlacement(),
                _ => !_isRandomizing);
        }

        public async void RandomizedShipPlacement()
        {
            _isRandomizing = true;

            // Clear all ships
            foreach (var ship in _player.Ships.ToArray())
            {
                ShipRemoved?.Invoke(ship);
                _player.RemoveShip(ship);
            }

            // Ensure a min waiting time of 500 ms before allowing the player to re randomize
            await System.Threading.Tasks.Task.Delay(300);

            // Place ships randomly
            Random random = new Random();
            foreach (var shipSize in _player.ShipRules)
            {
                Ship ship = new Ship(shipSize);
                bool isHorizontal = random.Next(2) == 0;
                int x = random.Next(0,9);
                int y = random.Next(0,9);
                while (!_player.PlaceShip(ship, x, y, isHorizontal))
                {
                    isHorizontal = random.Next(2) == 0;
                    x = random.Next(10);
                    y = random.Next(10);
                }
                ShipPlaced?.Invoke(x, y, isHorizontal, shipSize);
            }

            UpdateShipCounts();

            // Ensure a min waiting time of 500 ms before allowing the player to re randomize
            await System.Threading.Tasks.Task.Delay(300);

            _isRandomizing = false;

            ((RelayCommand)RandomizedShipPlacementCommand).RaiseCanExecuteChanged();
        }


        internal void SetClient(BattleShipServer.Client client)
        {
            _client = client;
        }
        internal void SetPlayer(Player player)
        {
            this._player = player;
        }

        public void MyFieldCellClicked(int x, int y)
        {
            // If there is a ship on the cell, remove it
            if (_player.Grid.GetCell(x, y).Status == CellStatus.Ship)
            {
                Ship ship = _player.Ships.FirstOrDefault(s => s.Cells.Any(cell => cell.X == x && cell.Y == y));
                ShipRemoved?.Invoke(ship);
                _player.RemoveShip(ship);
                return;
            }

            // Test if player can still place ships of this size
            var shipRules = _player.ShipRules;
            int numberOfShipAllowed = shipRules.Count(s => s == ShipSize);
            if(_player.Ships.Count(ship => ship.Size == ShipSize) < numberOfShipAllowed) 
            {
                if (_player.AreAllShipsPlaced()) return;

                int size = ShipSize;
                Ship ship = new Ship(size);
                bool isHorizontal = IsHorizontal;

                if (_player.PlaceShip(ship, x, y, isHorizontal))
                {
                    ShipPlaced?.Invoke(x, y, isHorizontal, size);
                }
            }

            UpdateShipCounts();
        }

        private void UpdateShipCounts()
        {
            TwoShipCount = _player.Ships.Count(ship => ship.Size == 2);
            ThreeShipCount = _player.Ships.Count(ship => ship.Size == 3);
            FourShipCount = _player.Ships.Count(ship => ship.Size == 4);
            FiveShipCount = _player.Ships.Count(ship => ship.Size == 5);

            ((RelayCommand)ConfirmShipPlacementCommand).RaiseCanExecuteChanged();
        }

        public void ConfirmShips()
        {
            if (_player.AreAllShipsPlaced())
            {
                foreach (var ship in _player.Ships)
                {
                    var startX = ship.Cells.Min(cell => cell.X);
                    var endX = ship.Cells.Max(cell => cell.X);
                    var startY = ship.Cells.Min(cell => cell.Y);

                    var isHorizontal = startX != endX;

                    _client.PlaceShip(startX, startY, isHorizontal, ship.Size);                    
                }

                PlayerReady?.Invoke();
            }
        }
    }
}
