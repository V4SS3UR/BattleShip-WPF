using WPF_App.Core;

namespace WPF_App.MVVM.Model
{
    public class Message : ObservableObject
    {
        private string _text; public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }
        private string _userId; public string UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(); }
        }

        private bool _isMine; public bool IsMine
        {
            get => _isMine;
            set { _isMine = value; OnPropertyChanged(); }
        }
        private bool _isServer; public bool IsServer
        {
            get => _isServer;
            set { _isServer = value; OnPropertyChanged(); }
        }

        public Message(string message)
        {
            Text = message;
        }

        public Message(string text, string userId)
        {
            Text = text;
            UserId = userId;
        }
        
        public Message(string text, string userId, bool isMine)
        {
            Text = text;
            UserId = userId;
            IsMine = isMine;
        }
    }
}
