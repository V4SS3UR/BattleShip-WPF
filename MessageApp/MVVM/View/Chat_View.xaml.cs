using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessageApp.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour Chat_View.xaml
    /// </summary>
    public partial class Chat_View : UserControl
    {
        public Chat_View()
        {
            InitializeComponent();
        }

        private void ChatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var chatTextBox = sender as TextBox;

            //If enter key is pressed and the message is not empty, send the message
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(chatTextBox.Text))
            {
                (DataContext as ViewModel.Chat_ViewModel).SendMessageCommand.Execute(null);
            }
        }
    }
}
