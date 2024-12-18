using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPF_App.MVVM.ViewModel;

namespace WPF_App.MVVM.View
{
    /// <summary>
    /// Logique d'interaction pour BattleShip_Game_ViewModel.xaml
    /// </summary>
    public partial class BattleShip_Game_View : UserControl
    {
        private BattleShip_Game_ViewModel viewModel => DataContext as BattleShip_Game_ViewModel;


        public BattleShip_Game_View()
        {
            InitializeComponent();

            viewModel.OpponentHitted += ViewModel_OpponentHitted;
            viewModel.OpponentSunk += ViewModel_OpponentSunk;
            viewModel.OpponentMissed += ViewModel_OpponentMissed;
            viewModel.PlayerHitted += ViewModel_PlayerHitted;
            viewModel.PlayerSunk += ViewModel_PlayerSunk;
            viewModel.PlayerMissed += ViewModel_PlayerMissed;

            this.Loaded += BattleShip_Game_View_Loaded;
        }


        private void BattleShip_Game_View_Loaded(object sender, RoutedEventArgs e)
        {
            DrawPlayerGrid();
            DrawOpponentGrid();

            var player = viewModel.GetPlayer();
            var ships = player.Ships;

            foreach (var ship in ships)
            {
                var cells = ship.Cells;
                int x = cells[0].X;
                int y = cells[0].Y;
                bool isHorizontal = cells.All(c => c.Y == y);

                PlaceShip(x, y, isHorizontal, ship.Size);
            }
        }
        private void PlaceShip(int x, int y, bool isHorizontal, int size)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Console.WriteLine($"Drawn ship at {x}, {y} with size {size} and orientation {isHorizontal}");
                    DrawPlayerShip(x, y, isHorizontal, size);

                    //Get the cellborder
                    if (isHorizontal)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            var cell = PlayerFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == x + i && Grid.GetRow(o) == y);
                            cell.HasShip = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < size; i++)
                        {
                            var cell = PlayerFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == x && Grid.GetRow(o) == y + i);
                            cell.HasShip = true;
                        }
                    }
                }
                catch (Exception)
                {
                }
            });
        }
        private void DrawPlayerShip(int x, int y, bool isHorizontal, int size)
        {
            Border shipBorder = new Border
            {
                Name = $"shipBorder{x}{y}",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3D5E81")),
                Opacity = 1,
                CornerRadius = isHorizontal ? new CornerRadius(5, 25, 25, 5) : new CornerRadius(5, 5, 25, 25),
                Margin = new Thickness(7),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0, 0)
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

            PlayerFieldGrid.Children.Add(shipBorder);

            // Animate the ship scale from 0 to 1
            var scaleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut }
            };

            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
        private void DrawOpponentShip(int x, int y, bool isHorizontal, int size)
        {
            Border shipBorder = new Border
            {
                Name = $"shipBorder{x}{y}",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD30002")),
                Opacity = 1,
                CornerRadius = isHorizontal ? new CornerRadius(5, 25, 25, 5) : new CornerRadius(5, 5, 25, 25),
                Margin = new Thickness(7),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0, 0)
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

            OpponentFieldGrid.Children.Add(shipBorder);

            // Animate the ship scale from 0 to 1
            var scaleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut }
            };

            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            shipBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private void ViewModel_OpponentHitted(int arg1, int arg2)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var opponentCell = OpponentFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == arg1 && Grid.GetRow(o) == arg2);
                opponentCell.IsHit = true;
            });
        }
        private void ViewModel_OpponentSunk(int arg1, int arg2, bool isHorizontal, int size)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // Draw the ship of the opponent
                DrawOpponentShip(arg1, arg2, isHorizontal, size);
            });
        }
        private void ViewModel_OpponentMissed(int arg1, int arg2)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var myCell = OpponentFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == arg1 && Grid.GetRow(o) == arg2);
                myCell.IsMissed = true;
            });
        }

        private void ViewModel_PlayerHitted(int arg1, int arg2)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var myCell = PlayerFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == arg1 && Grid.GetRow(o) == arg2);
                myCell.IsHit = true;
            });
        }
        private void ViewModel_PlayerSunk(int arg1, int arg2, bool isHorizontal, int size)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // Get the ship border
                var shipBorder = PlayerFieldGrid.Children.OfType<Border>().FirstOrDefault(o => o.Name == $"shipBorder{arg1}{arg2}");
                if (shipBorder != null)
                {
                    shipBorder.Background = Brushes.Red;
                }
            });
        }
        private void ViewModel_PlayerMissed(int arg1, int arg2)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var opponentCell = PlayerFieldGrid.Children.OfType<BorderCell>().First(o => Grid.GetColumn(o) == arg1 && Grid.GetRow(o) == arg2);
                opponentCell.IsMissed = true;
            });
        }


        private void DrawPlayerGrid()
        {
            var grid = PlayerFieldGrid;

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
                    cell.Name = $"cell{i}{j}";
                    cell.SetPosition(i, j);
                    grid.Children.Add(cell);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var cell = new BorderCell();
                    cell.Name = $"probabilityMap{i}{j}";
                    cell.SetPosition(i, j);
                    cell.Opacity = 0.5;
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
        private void DrawOpponentGrid()
        {
            var grid = OpponentFieldGrid;
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

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var cell = new BorderCell();
                    cell.Name = $"cell{i}{j}";
                    cell.MouseEnter += Cell_MouseEnter;
                    cell.MouseLeave += Cell_MouseLeave;
                    cell.CellClicked += OtherFieldGrid_CellClicked;
                    cell.SetPosition(i, j);
                    grid.Children.Add(cell);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var cell = new BorderCell();
                    cell.Name = $"probabilityMap{i}{j}";
                    cell.SetPosition(i, j);
                    cell.Opacity = 0.5;
                    grid.Children.Add(cell);
                }
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
                IsHitTestVisible = false
            };

            Grid.SetRow(border, y);
            Grid.SetColumn(border, x);

            // Opacity animation
            var opacityAnimation = new DoubleAnimation(0, 0.3, TimeSpan.FromSeconds(0.2));
            border.BeginAnimation(Border.OpacityProperty, opacityAnimation);

            OpponentFieldGrid.Children.Add(border);
        }
        private void Cell_MouseLeave(object sender, MouseEventArgs e)
        {
            //Get the temp border of the grid position
            var cell = sender as BorderCell;

            int x = Grid.GetColumn(cell);
            int y = Grid.GetRow(cell);

            var cursor = OpponentFieldGrid.Children.OfType<BorderCell>().FirstOrDefault(o => o.Name == $"cursor{x}{y}");

            // Opacity animation
            var opacityAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            opacityAnimation.Completed += (s, _) =>
            {
                OpponentFieldGrid.Children.Remove(cursor);
            };

            cursor.BeginAnimation(Border.OpacityProperty, opacityAnimation);
        }
        private void OtherFieldGrid_CellClicked(int arg1, int arg2)
        {
            viewModel.OpponentFieldCellClicked(arg1, arg2);
        }



        public void UpdatePlayerProbabilityMap(double[,] map)
        {
            var playerGrid = this.PlayerFieldGrid;

            // Get all the cells
            var cells = playerGrid.Children.OfType<BorderCell>().Where(o => o.Name.StartsWith("probabilityMap")).ToArray();

            // Update the color of the cells from red = 0 to red = 255 based on the max probability

            double max = map.Cast<double>().Max();

            foreach (var cell in cells)
            {
                int x = Grid.GetColumn(cell);
                int y = Grid.GetRow(cell);
                double value = map[x, y];

                // from black to green
                byte color = (byte)(255 * value / max);
                cell.Background = new SolidColorBrush(Color.FromArgb(color, (byte)(255 - color), color, 0));
            }
        }
        public void UpdateOpponentProbabilityMap(double[,] map)
        {
            var opponentGrid = this.OpponentFieldGrid;

            // Get all the cells
            var cells = opponentGrid.Children.OfType<BorderCell>().Where(o => o.Name.StartsWith("probabilityMap")).ToArray();

            // Update the color of the cells from red = 0 to red = 255 based on the max probability
            double max = map.Cast<double>().Max();

            foreach (var cell in cells)
            {
                int x = Grid.GetColumn(cell);
                int y = Grid.GetRow(cell);
                double value = map[x, y];
                // from black to green
                byte color = (byte)(255 * value / max);
                cell.Background = new SolidColorBrush(Color.FromArgb(color, (byte)(255 - color), color, 0));
            }
        }
    }
}
