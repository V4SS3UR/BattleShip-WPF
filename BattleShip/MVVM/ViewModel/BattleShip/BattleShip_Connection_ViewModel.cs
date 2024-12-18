using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_App.Core;

namespace WPF_App.MVVM.ViewModel
{
    internal class BattleShip_Connection_ViewModel: ObservableObject
    {
        public event Action<BattleShipServer.Client> ConnectedToServer;
        public event Action PlayLocal;
        public event Action PlayAuto;

        private bool _connecting; public bool Connecting
        {
            get => _connecting;
            set { _connecting = value; OnPropertyChanged(); }
        }
        private bool _connected; public bool Connected
        {
            get => _connected;
            set { _connected = value; OnPropertyChanged(); }
        }
        private string _myIp; public string MyIp
        {
            get => _myIp;
            set { _myIp = value; OnPropertyChanged(); }
        }
        private string _opponentIp; public string OpponentIp
        {
            get => _opponentIp;
            set { _opponentIp = value; OnPropertyChanged(); }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand PlayLocalCommand { get; set; }
        public ICommand PlayAutoCommand { get; set; }

        private BattleShipServer.Client client;

        public BattleShip_Connection_ViewModel()
        {
            client = new BattleShipServer.Client();

            MyIp = Network.GetLocalIPAddress();
            OpponentIp = MyIp;

            ConnectCommand = new RelayCommand(
                async _ => await ConnectToServer(),
                _ => !string.IsNullOrEmpty(OpponentIp) && !Connecting &&!Connected);

            PlayLocalCommand = new RelayCommand(
                async _ => await PlayLocalGame(),
                _ => !Connecting && !Connected);

            PlayAutoCommand = new RelayCommand(
                _ => PlayAutoGame(),
                _ => !Connecting && !Connected);
        }

        private async Task ConnectToServer()
        {
            try
            {
                Connecting = true;
                await client.StartClient(OpponentIp);
                this.Connected = true;
                Connecting = false;

                ConnectedToServer?.Invoke(client);
            }
            catch (System.Exception ex)
            {
                Connecting = false;
            }
        }

        private async Task PlayLocalGame()
        {
            PlayLocal?.Invoke();
            await ConnectToServer();
        }

        private void PlayAutoGame()
        {
            PlayAuto?.Invoke();
        }
    }
}
