using BattleShipServer;
using System;
using System.Windows.Input;
using VsrFade.Extensions;
using WPF_App.Core;

namespace WPF_App.MVVM.ViewModel
{
    internal class BattleShip_Game_ViewModel : ObservableObject
    {
        public event Action<int, int> OpponentHitted;
        public event Action<int, int, bool, int> OpponentSunk;
        public event Action<int, int> OpponentMissed;
        public event Action<int, int> PlayerHitted;
        public event Action<int, int, bool, int> PlayerSunk;
        public event Action<int, int> PlayerMissed;

        public event Action Winned;
        public event Action Lost;

        private bool _isMyTurn; public bool IsMyTurn
        {
            get => _isMyTurn;
            set { _isMyTurn = value; OnPropertyChanged(); }
        }

        private bool _isWinned; public bool IsWinned
        {
            get => _isWinned;
            set { _isWinned = value; OnPropertyChanged(); }
        }
        private bool _isLost; public bool IsLost
        {
            get => _isLost;
            set { _isLost = value; OnPropertyChanged(); }
        }



        private Player _player;
        private Client _client;

        public ICommand NewGameCommand { get; set; }


        public BattleShip_Game_ViewModel()
        {
            NewGameCommand = new RelayCommand(
                _ => NewGame(),
                _ => IsLost || IsWinned);
        }

        internal void SetClient(Client client)
        {
            _client = client;

            _client.NewTurn += Client_NewTurn;
            _client.Hit += Client_Hit;
            _client.Sunk += Client_Sunk;
            _client.Miss += Client_Miss;

            _client.Winned += Client_Winned;
            _client.Lost += Client_Lost;
        }    

        internal Player GetPlayer()
        {
            return _player;
        }
        internal void SetPlayer(Player player)
        {
            this._player = player;
        }

        public void OpponentFieldCellClicked(int x, int y)
        {
            if (_player.State == PlayerState.TakingTurn)
            {
                _client.FireShot(x, y);
            }
            else
            {
                // Not your turn
            }            
        }
        public void NewGame()
        {
            _client.RequestNewGame();
        }


        private void Client_Winned()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _player.State = PlayerState.Winned;
                IsWinned = true;
                Winned?.Invoke();
                ToastManager.Toast.GetToast("BattleShipToast").ShowSuccessToast("You won the game!", "Congratulations!", true);
            });
        }
        private void Client_Lost()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _player.State = PlayerState.Lost;
                IsLost = true;
                Lost?.Invoke();
                ToastManager.Toast.GetToast("BattleShipToast").ShowErrorToast("You lost the game!", "Better luck next time!", true);
            });
        }
        private void Client_NewTurn(bool myTurn)
        {
            IsMyTurn = myTurn;
            _player.State = myTurn ? PlayerState.TakingTurn : PlayerState.WaitingForTurn;
        }
        private void Client_Hit(int arg1, int arg2)
        {
            if (IsMyTurn)
            {
                // I hit the opponent
                OpponentHitted?.Invoke(arg1, arg2);
            }
            else
            {
                // Opponent hit me
                PlayerHitted?.Invoke(arg1, arg2);
            }
        }
        private void Client_Sunk(int arg1, int arg2, bool isHorizontal, int size)
        {
            if (IsMyTurn)
            {
                // I sunk the opponent
                OpponentSunk?.Invoke(arg1, arg2, isHorizontal, size);
            }
            else
            {
                // Opponent sunk me
                PlayerSunk?.Invoke(arg1, arg2, isHorizontal, size);
            }
        }
        private void Client_Miss(int arg1, int arg2)
        {
            if (IsMyTurn)
            {
                // Opponent missed me
                OpponentMissed?.Invoke(arg1, arg2);
            }
            else
            {
                // I missed the opponent
                PlayerMissed?.Invoke(arg1, arg2);
            }
        }
    }
}
