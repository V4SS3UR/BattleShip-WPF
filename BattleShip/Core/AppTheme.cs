using System;
using System.Windows;

namespace WPF_App.Core
{
    public class AppTheme
    {
        public static ResourceDictionary CurrentTheme { get; set; }

        public static ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return App.Current.Resources.MergedDictionaries[0]; }
        }

        public static void ChangeTheme(Uri themeUri)
        {
            if (CurrentTheme != null)
            {
                ThemeDictionary.MergedDictionaries.Remove(CurrentTheme);
            }

            var theme = new ResourceDictionary() { Source = themeUri };
            ThemeDictionary.MergedDictionaries.Add(theme);

            CurrentTheme = theme;
        }
    }




    public class Skin : ObservableObject
    {
        private string _name; public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public string ResourceDictionaryPath { get; set; }


        public Skin(string name, string resourceDictionary)
        {
            this.Name = name;
            this.ResourceDictionaryPath = resourceDictionary;
        }
    }
}
