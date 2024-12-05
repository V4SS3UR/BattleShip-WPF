using MessageApp.Core;
using MessageApp.MVVM.View;

namespace MessageApp.MVVM.ViewModel
{
    public class MainInterface_ViewModel : ObservableObject
    {
        public ViewNavigator ViewNavigator { get; }


        public MainInterface_ViewModel()
        {
            this.ViewNavigator = new ViewNavigator();

            var chatView = this.ViewNavigator.AddView(typeof(Chat_View), "Chat", selected: true);

            chatView.Navigate();
        }
    }
}
