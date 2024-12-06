using WPF_App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_App.MVVM.Model
{
    internal class User : ObservableObject
    {
        private string _username; public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        private string _id; public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
