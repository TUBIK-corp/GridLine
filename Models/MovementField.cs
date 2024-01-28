using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GridLine_IDE.Models
{
    public class MovementField
    {

        public delegate void MovementEvent();
        public event MovementEvent MoveEnded;
        public event MovementEvent Moved;

        public Grid Field { get; private set; }
        public Border UserVisual { get; private set; }
        public List<Point> Positions { get; private set; }


        public Timer timer = null;
        public int Tick { get; private set; }
        public int Iterator { get; private set; }

        public MovementField(Grid grid)
        {
            Field = grid;
            Positions = new List<Point>() { new Point(0, 0) };
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
                if (Iterator+1 >= Positions.Count)
                {
                    //Iterator--;
                    timer.Stop();
                    Tick = 0;
                    EndMove();
                    return;
                }
                UpdateUI(Iterator);
                Iterator++;
                Moved?.Invoke();
            }
        }

        public int GetCurrentMoveIndex()
        {
            return Iterator;
        }

        public void Pause() => timer.Stop();
        public void Resume() => timer.Start();

        private void EndMove()
        {
            MoveEnded?.Invoke();
        }

        public void UpdatePositions(List<Point> positions)
        {
            Positions = positions;
            Positions.Insert(0, new Point(0,0));
            ReloadUI();
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
            UpdateUI(Positions.Count - 1);
            EndMove();
            timer.Stop();
        }

        public void StepBackward()
        {
            if (Iterator > 0)
            {
                Iterator--;
                Moved?.Invoke();
                UpdateUI(Iterator);
            }
        }
        
        public void StepForward()
        {
            if (Iterator < Positions.Count-1)
            {
                Iterator++;
                Moved?.Invoke();
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
        public void ReloadUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Grid.SetColumn(UserVisual, 0);
                Grid.SetRow(UserVisual, 0);
            });
        }
    }
}
