using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Imaging;
using System;

using GridLine_IDE.Helpers;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Data;

namespace GridLine_IDE.Models
{
    public class MovementField
    {

        public delegate void MovementEvent();
        public event MovementEvent MoveEnded;
        public event MovementEvent Moved;

        public Grid Field { get; private set; }
        public System.Windows.Controls.Image UserVisual { get; private set; }
        public List<Point> Positions { get; private set; }


        public Timer timer = null;
        public int Tick { get; private set; }
        public int Iterator { get; private set; }

        public MovementField(Grid grid)
        {
            Field = grid;
            Positions = new List<Point>() { new Point(0, 0) };
            UserVisual = new System.Windows.Controls.Image();
            UserVisual.Margin = new System.Windows.Thickness(2);
            UserVisual.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            UserVisual.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            UserVisual.Source = App.CurrentSkin.ImageSource;

            object c = new object();

            Field.Children.Add(UserVisual);

            timer = new Timer(10);
            Tick = 0;
            Iterator = 0;
            timer.Elapsed += TimerElapsed;
        }

        public void ReloadIcon()
        {
            UserVisual.Source = App.CurrentSkin.ImageSource;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Tick += 10;
            if (Tick % App.ExecutionInterval == 0)
            {
                if (Iterator >= Positions.Count)
                {
                    Iterator--;
                    timer.Stop();
                    Tick = 0;
                    EndMove();
                    Moved?.Invoke();
                    return;
                }
                Iterator++;
                FillCurrentCell();
                UpdateUI(Iterator);
                Moved?.Invoke();
            }
        }

        public void FillCurrentCell()
        {
            try
            {
                if (Iterator < Positions.Count)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var list = Field.Children.OfType<Border>();
                        var border = list.First(pos => (pos.Tag as GridHelper.Cell).X == Positions[Iterator].X
                            && (pos.Tag as GridHelper.Cell).Y == Positions[Iterator].Y);

                        border.Background = new SolidColorBrush(Color.FromRgb(166, 166, 188));

                    });
                }
            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            Positions.Insert(0, App.LangLineProgram.SpawnPoint);
            Iterator = 0;
            Tick = 0;
            ReloadUI();
            FillCurrentCell();
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
                FillCurrentCell();
                Moved?.Invoke();
                UpdateUI(Iterator);
            }
        }


        public void UpdateUI(int index)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (index < Positions.Count)
                    {
                        Grid.SetColumn(UserVisual, (int)Positions[index].X);
                        Grid.SetRow(UserVisual, (int)Positions[index].Y);
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        public void ReloadUI()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var list = Field.Children.OfType<Border>();
                foreach (var border in list)
                {
                    border.Background = new SolidColorBrush(Colors.AliceBlue);
                }

                Grid.SetColumn(UserVisual, (int)App.LangLineProgram.SpawnPoint.X);
                Grid.SetRow(UserVisual, (int)App.LangLineProgram.SpawnPoint.Y);
            });
        }
    }
}
