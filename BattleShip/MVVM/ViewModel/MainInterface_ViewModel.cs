using WPF_App.Core;
using WPF_App.MVVM.View;

namespace WPF_App.MVVM.ViewModel
{
    public class MainInterface_ViewModel : ObservableObject
    {
        public ViewNavigator ViewNavigator { get; }


        public MainInterface_ViewModel()
        {
            this.ViewNavigator = new ViewNavigator();

            var battleShipView = this.ViewNavigator.AddView(typeof(BattleShip_View), "BattleShip", singleton: false);
            //var battleShipDebugView = this.ViewNavigator.AddView(typeof(BattleShip_Debug_View), "Debug", singleton:false);

            battleShipView.Navigate();
        }
    }
}
