using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using WPF_App.MVVM.ViewModel;

namespace WPF_App.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour Chat_View.xaml
    /// </summary>
    public partial class BattleShip_Chat_View : UserControl
    {
        public BattleShip_Chat_View()
        {
            InitializeComponent();

            this.Loaded += View_Loaded;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as BattleShip_ViewModel;
            vm.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
        }

        private void ChatMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MessageScrollViewer.ScrollToBottom();
        }

        private void ChatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var chatTextBox = sender as TextBox;

            //If enter key is pressed and the message is not empty, send the message
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(chatTextBox.Text))
            {
                (DataContext as BattleShip_ViewModel).SendMessageCommand.Execute(null);
            }
        }
    }
}
