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
using WPF_App.Core;

namespace WPF_App.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour BattleShip_Debug_View.xaml
    /// </summary>
    public partial class BattleShip_Debug_View : UserControl
    {
        public BattleShip_Debug_View()
        {
            InitializeComponent();

            BattleShipServer.Server server = new BattleShipServer.Server();
            _ = server.StartServer();
        }
    }
}
