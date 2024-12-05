using MessageApp.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MessageApp.MVVM.ViewModel
{
    internal class Chat_ViewModel : ObservableObject
    {
        private string _username; public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        private string _connectionStatus; public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }
        private string _messageToSend; public string MessageToSend
        {
            get => _messageToSend;
            set { _messageToSend = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ReceivedMessages { get; set; }

        

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendMessageCommand { get; }



        private readonly ChatClient _messageClient;


        public Chat_ViewModel()
        {
            ReceivedMessages = new ObservableCollection<string>();

            _messageClient = new ChatClient();
            _messageClient.ConnectionStatusChanged += UpdateConnectionStatus;
            _messageClient.MessageReceived += AddReceivedMessage;

            ConnectCommand = new RelayCommand(async _ => await ConnectToServer());
            DisconnectCommand = new RelayCommand(_ => _messageClient.Disconnect());
            SendMessageCommand = new RelayCommand(async _ => await SendMessage());
        }

        private async Task ConnectToServer()
        {
            if (string.IsNullOrEmpty(Username))
            {
                UpdateConnectionStatus("Error: Username is required.");
                return;
            }

            _messageClient.Username = Username;
            await _messageClient.ConnectAsync();
        }

        private void UpdateConnectionStatus(string status)
        {
            ConnectionStatus = status;
            ReceivedMessages.Add(status);
        }

        private void AddReceivedMessage(string message)
        {
            ReceivedMessages.Add(message);
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrEmpty(MessageToSend)) return;
            await _messageClient.SendMessageAsync(MessageToSend);
            MessageToSend = string.Empty;
        }
    }
}
