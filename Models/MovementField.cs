using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GridLine_IDE.Models
{
    public class MovementField
    {
        public Grid Field { get; private set; }
        public Border UserVisual { get; private set; }
        public List<Point> Positions { get; private set; }


        public Timer timer = null;
        public int Tick { get; private set; }
        public int Iterator { get; private set; }

        public MovementField(Grid grid)
        {
            Field = grid;
            UserVisual = new Border();
            UserVisual.Margin = new System.Windows.Thickness(4);
            UserVisual.Background = Brushes.Red;
            UserVisual.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            UserVisual.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            object c = new object();
            Field.Children.Add(UserVisual);

            timer = new Timer(10);
            Tick = 0;
            Iterator = 0;
            timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Tick += 10;
            if (Tick % App.ExecutionInterval == 0)
            {
                if (Iterator >= Positions.Count)
                {
                    timer.Stop();
                    Tick = 0;
                    Iterator = 0;
                    return;
                }
                UpdateUI(Iterator);
                Iterator++;
            }
        }

        public void UpdatePositions(List<Point> positions)
        {
            Positions = positions;
        }

        public void StartMovement()
        {
            Iterator = 0;
            Tick = 0;
            timer.Start();
        }

        public void StopMovement()
        {
            Iterator = 0;
            Tick = 0;
            timer.Stop();
        }

        public void StepBackward()
        {
            if (Iterator > 0)
            {
                Iterator--;
                UpdateUI(Iterator);
            }
        }
        
        public void StepForward()
        {

            if (Iterator < Positions.Count)
            {
                Iterator++;
                UpdateUI(Iterator);
            }
        }


        public void UpdateUI(int index)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Grid.SetColumn(UserVisual, (int)Positions[index].X);
                Grid.SetRow(UserVisual, (int)Positions[index].Y);
            });
        }
    }
}
