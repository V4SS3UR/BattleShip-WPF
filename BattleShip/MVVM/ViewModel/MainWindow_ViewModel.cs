using WPF_App.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace WPF_App.MVVM.ViewModel
{
    internal class MainWindow_ViewModel : ObservableObject
    {
        public static MainWindow_ViewModel Instance { get; set; }

        private string _windowTitle; public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        private string _windowTitleVersion; public string WindowTitleVersion
        {
            get { return _windowTitleVersion; }
            set { _windowTitleVersion = value; OnPropertyChanged(); }
        }

        private object _currentView; public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }


        public MainWindow_ViewModel()
        {
            Instance = this;

            WindowTitle = Assembly.GetExecutingAssembly().GetName().Name;
            WindowTitleVersion = GetAssemblyVersion();

            GetResourceDictionary();

            this.CurrentView = new MainInterface_ViewModel();
        }

        private string GetAssemblyVersion()
        {
            string assemblyNameVersionMajor = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            string assemblyNameVersionMinor = Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            string assemblyNameVersionBuild = Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            string assemblyNameVersionRevision = Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();

            string version = $"v{assemblyNameVersionMajor}.{assemblyNameVersionMinor}";

            if (assemblyNameVersionBuild != "0")
            {
                version += $".{assemblyNameVersionBuild}";
            }

            if (assemblyNameVersionRevision != "0")
            {
                version += $".{assemblyNameVersionRevision}";
            }

            return version;
        }




        public ObservableCollection<Skin> Themes { get; set; }
        private Skin _selectedTheme; public Skin SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;

                AppTheme.ChangeTheme(new Uri(value.ResourceDictionaryPath, UriKind.Relative));

                OnPropertyChanged();
            }
        }


        public void GetResourceDictionary()
        {
            this.Themes = new ObservableCollection<Skin>();
            this.Themes.Add(new Skin("Fog Gray", @"Ressources/Themes/Fog Gray.xaml"));
            this.Themes.Add(new Skin("Concrete Gray", @"Ressources/Themes/Concrete Gray.xaml"));
            this.Themes.Add(new Skin("Sandstone Beige", @"Ressources/Themes/Sandstone Beige.xaml"));
            this.Themes.Add(new Skin("Forest Green", @"Ressources/Themes/Forest Green.xaml"));
            this.Themes.Add(new Skin("Lavender Purple", @"Ressources/Themes/Lavender Purple.xaml"));
            this.Themes.Add(new Skin("Ocean Blue", @"Ressources/Themes/Ocean Blue.xaml"));
            this.Themes.Add(new Skin("Ocean Depths", @"Ressources/Themes/Ocean Depths.xaml"));
            this.Themes.Add(new Skin("Deep Teal Calm", @"Ressources/Themes/Deep Teal Calm.xaml"));
            this.Themes.Add(new Skin("Sunset Orange", @"Ressources/Themes/Sunset Orange.xaml"));

            var oceanDepths = Themes.First(t => t.Name == "Ocean Depths");
            SelectedTheme = oceanDepths;
        }
    }
}