using BattleShipServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using WPF_App.MVVM.ViewModel;

namespace WPF_App.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour BattleShip_ShipPlacement_View.xaml
    /// </summary>
    public partial class BattleShip_ShipPlacement_View : UserControl
    {
        private BattleShip_ShipPlacement_ViewModel viewModel => DataContext as BattleShip_ShipPlacement_ViewModel;

        public BattleShip_ShipPlacement_View()
        {
            InitializeComponent();

            viewModel.ShipPlaced += ViewModel_ShipPlaced;
            viewModel.ShipRemoved += ViewModel_ShipRemoved;
        }        

        private void ViewModel_ShipPlaced(int x, int y, bool isHorizontal, int size)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Console.WriteLine($"Drawn ship at {x}, {y} with size {size} and orientation {isHorizontal}");
                    DrawShip(x, y, isHorizontal, size);

                    //Get the cellborder
                    if (isHorizontal)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            var cell = MyFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == x + i && Grid.GetRow(o) == y);
                            cell.HasShip = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                        {
                            var cell = MyFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == x && Grid.GetRow(o) == y + i);
                            cell.HasShip = true;
                        }
                    }
                }
                catch (Exception)
                {
                }
            });
        }
        private void ViewModel_ShipRemoved(Ship ship)
        {
            UnDrawShip(ship);

            // Get cell of ship and tag is as not having a ship
            foreach (var cell in ship.Cells)
            {
                var cellBorder = MyFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == cell.X && Grid.GetRow(o) == cell.Y);
                cellBorder.HasShip = false;
            }
        }

        private void DrawShip(int x, int y, bool isHorizontal, int size)
        {
            Border shipBorder = new Border
            {
                Name = $"shipBorder{x}{y}",
                //Colors from hex
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3D5E81")),
                Opacity = 1,
                CornerRadius = isHorizontal ? new CornerRadius(5,25,25,5) : new CornerRadius(5,5,25,25),
                Margin = new Thickness(7),
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0, 0),
            };

            Grid.SetRow(shipBorder, y);
            Grid.SetColumn(shipBorder, x);

            if (isHorizontal)
            {
                Grid.SetColumnSpan(shipBorder, size);
            }
            else
            {
                Grid.SetRowSpan(shipBorder, size);
            }

            MyFieldGrid.Children.Add(shipBorder);

            // Animate the ship scale from 0 to 1
            var scaleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut }
            };

            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
        private void UnDrawShip(Ship ship)
        {
            var borderShips = MyFieldGrid.Children.OfType<Border>();

            var shipCells = ship.Cells;
            if (shipCells.Count == 0) return;

            int x = shipCells[0].X;
            int y = shipCells[0].Y;

            // Find the border where row or column is one of the cell
            var border = borderShips.FirstOrDefault(o => o.Name == $"shipBorder{x}{y}");

            // Animate the ship scale from 1 to 0
            var scaleAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new BackEase() { EasingMode = EasingMode.EaseIn }
            };

            scaleAnimation.Completed += (s, e) =>
            {
                if(MyFieldGrid.Children.Contains(border))
                {
                    MyFieldGrid.Children.Remove(border);
                }
            };

            border.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            border.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private void MyFieldGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            grid.MouseLeave += (_, e2) =>
            {
                // Remove all temp borders
                var borders = grid.Children.OfType<BorderCell>().Where(o => o.Name.StartsWith("cursor")).ToArray();
                foreach (var border in borders)
                {
                    grid.Children.Remove(border);
                }
            };

            // Create 10x10 grid
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var cell = new BorderCell();
                    cell.MouseEnter += Cell_MouseEnter;
                    cell.MouseLeave += Cell_MouseLeave;
                    cell.CellClicked += MyFieldGrid_CellClicked;
                    cell.SetPosition(i, j);
                    grid.Children.Add(cell);
                }
            }

            // Add the row and column numbers to the grid
            // first row and first column with a negative margin to display outside the grid
            for (int i = 0; i < 10; i++)
            {
                var rowNumber = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(-75, 0, 0, 0),
                };
                Grid.SetRow(rowNumber, i);
                Grid.SetColumn(rowNumber, 0);
                grid.Children.Add(rowNumber);

                //Columns are letters
                var columnLetter = new TextBlock
                {
                    Text = ((char)(65 + i)).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -75, 0, 0)
                };
                Grid.SetRow(columnLetter, 0);
                Grid.SetColumn(columnLetter, i);
                grid.Children.Add(columnLetter);
            }
        }
        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            // Add a temp border to show the cursor position at the same grid position
            var cell = sender as BorderCell;

            int x = Grid.GetColumn(cell);
            int y = Grid.GetRow(cell);

            var border = new BorderCell
            {
                Name = $"cursor{x}{y}",
                Background = Brushes.Blue,
                Margin = new Thickness(7),
                CornerRadius = viewModel.IsHorizontal ? new CornerRadius(5, 25, 25, 5) : new CornerRadius(5, 5, 25, 25),
                IsHitTestVisible = false
            };

            Grid.SetRow(border, y);
            Grid.SetColumn(border, x);

            // Expand the cursor to the size of the ship
            if (viewModel.IsHorizontal)
            {
                Grid.SetColumnSpan(border, viewModel.ShipSize);
            }
            else
            {
                Grid.SetRowSpan(border, viewModel.ShipSize);
            }

            // if the ship is too big, set background color to red
            if ( (viewModel.IsHorizontal ? x : y) + viewModel.ShipSize > 10)
            {
                border.Background = Brushes.Red;

                // And remove the head of the ship
                border.CornerRadius = viewModel.IsHorizontal ? new CornerRadius(5,0,0,5) : new CornerRadius(5,5,0,0);

                // And remove the last margin
                border.Margin = viewModel.IsHorizontal ? new Thickness(7, 7, 1, 7) : new Thickness(7, 7, 7, 1);
            }
            else

            // if there is a ship on the cells, set background color to red  
            for (int i = 0; i < viewModel.ShipSize; i++)
            {
                if (viewModel.IsHorizontal)
                {
                    if (MyFieldGrid.Children.OfType<BorderCell>().Any(o => Grid.GetColumn(o) == x + i && Grid.GetRow(o) == y && o.HasShip))
                    {
                        border.Background = Brushes.Red;
                        break;
                    }
                }
                else
                {
                    if (MyFieldGrid.Children.OfType<BorderCell>().Any(o => Grid.GetColumn(o) == x && Grid.GetRow(o) == y + i && o.HasShip))
                    {
                        border.Background = Brushes.Red;
                        break;
                    }
                }
            }

            // Opacity animation
            var opacityAnimation = new DoubleAnimation(0, 0.5, TimeSpan.FromSeconds(0.2));
            border.BeginAnimation(Border.OpacityProperty, opacityAnimation);

            MyFieldGrid.Children.Add(border);
        }
        private void Cell_MouseLeave(object sender, MouseEventArgs e)
        {
            //Get the temp border of the grid position
            var cell = sender as BorderCell;

            int x = Grid.GetColumn(cell);
            int y = Grid.GetRow(cell);

            var cursor = MyFieldGrid.Children.OfType<BorderCell>().FirstOrDefault(o => o.Name == $"cursor{x}{y}");

            // Opacity animation
            var opacityAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            opacityAnimation.Completed += (s, _) =>
            {
                MyFieldGrid.Children.Remove(cursor);
            };

            cursor.BeginAnimation(Border.OpacityProperty, opacityAnimation);
        }
        private void MyFieldGrid_CellClicked(int arg1, int arg2)
        {
            viewModel.MyFieldCellClicked(arg1, arg2);
        }
    }
}
