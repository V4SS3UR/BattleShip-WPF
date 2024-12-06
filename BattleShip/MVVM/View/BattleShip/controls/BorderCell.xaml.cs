using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_App.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour BorderCell.xaml
    /// </summary>
    public partial class BorderCell : Border, INotifyPropertyChanged
    {
        public event Action<int, int> CellClicked;

        private bool _isHit; public bool IsHit
        {
            get => _isHit;
            set 
            { 
                _isHit = value;
                AnimateScale(0.8);
                OnPropertyChanged(); 
            }
        }
        private bool _isSunk; public bool IsSunk
        {
            get => _isSunk;
            set 
            { 
                _isSunk = value;
                AnimateScale(0.8);
                OnPropertyChanged(); 
            }
        }
        private bool _isMissed; public bool IsMissed
        {
            get => _isMissed;
            set 
            { 
                _isMissed = value;
                AnimateScale(0.8);
                OnPropertyChanged(); 
            }
        }
        private bool _hasShip; public bool HasShip
        {
            get => _hasShip;
            set { _hasShip = value; OnPropertyChanged(); }
        }

        public BorderCell()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetPosition(int x, int y)
        {
            Grid.SetRow(this, x);
            Grid.SetColumn(this, y);
        }

        private void OnCellClicked(object sender, MouseButtonEventArgs e)
        {
            CellClicked?.Invoke(Grid.GetColumn(this), Grid.GetRow(this));
        }

        private void AnimateScale(double scale)
        {
            var scaleTransform = this.ScaleTransform;

            // Animate with a bouncing ease
            var animation = new DoubleAnimation(scale, TimeSpan.FromMilliseconds(200))
            {
                AutoReverse = true,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
