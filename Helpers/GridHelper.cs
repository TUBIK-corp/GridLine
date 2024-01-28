using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace IspolnitelCherepashka.Helpers
{
    public static class GridHelper
    {
        public static int Step = 36;
        public static Grid CreateGrid(int w_pixels, int h_pixels)
        {
            Grid grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromRgb(33, 33, 33));
            
            grid.Margin = new System.Windows.Thickness(4);
            grid.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            grid.Width = w_pixels*Step;
            grid.Height = h_pixels*Step;

            for (int wp = 0; wp < w_pixels; wp++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(Step, System.Windows.GridUnitType.Pixel) });
                for (int hp = 0; hp < h_pixels; hp++)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(Step, System.Windows.GridUnitType.Pixel) });
                    Border border = new Border();
                    border.Background = Brushes.AliceBlue;
                    border.Margin = new System.Windows.Thickness(1);
                    grid.Children.Add(border);
                    Grid.SetColumn(border, wp);
                    Grid.SetRow(border, hp);
                }
            }
            return grid;
        }

    }
}
