using GridLine_IDE.Enums;
using GridLine_IDE.Models;
using GridLine_IDE.NewCommands;
using IspolnitelCherepashka.Helpers;
using LangLine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using ExceptionEventArgs = LangLine.Models.ExceptionEventArgs;
namespace GridLine_IDE
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public List<LangLine.> CommandList;

        public MainWindow()
        {
            App.LangLineProgram = new LangLine.LangLineCore(21, 21);
            InitializeComponent();
            FieldBorder.AddHandler(UIElement.PreviewMouseWheelEvent,
                 new MouseWheelEventHandler(MouseWheel_Zooming),
                 true // Handler will be called even though e.Handled = true
                );
            App.LangLineProgram.InterpreterModule.OnCompleted += ProgramCompleted;
            App.LangLineProgram.InterpreterModule.OnException += ProgramException; ;
            App.MainGrid = new MovementField(GridHelper.CreateGrid(App.LangLineProgram.MainField.Width, App.LangLineProgram.MainField.Height));
            Field.Children.Add(App.MainGrid.Field);
            App.MainGrid.MoveEnded += GridMoveEnded;

            //new Thread(() => { while (true) {Thread.Sleep(100);  Console.WriteLine(App.LangLineProgram.StackTrace.Count); } }).Start();
            //try
            //{
            //    App.LangLineProgram.LogException(new ExceptionLog(1, new Exception()));
            //} catch
            //{
            //
            //}
            App.LangLineProgram.RegisterCommand("LOG", typeof(LogCommand));
            SetupProgram();
        }

        private void GridMoveEnded()
        {
            Dispatcher.Invoke(() =>
            {
                ToggleStopButton(false);
                TogglePRButton(PRStatuses.DisabledResume); 
            });
        }

        public void SetupProgram()
        {
            App.LangLineProgram.LimitNesting(3);
            App.LangLineProgram.InterpreterModule.OnCustomEvent += CustomEventInvoked;
        }

        private void CustomEventInvoked(CustomEventArgs args)
        {
            switch(args.Name)
            {
                case "Log":
                    var paragraph = new Paragraph();
                    paragraph.Foreground = Brushes.White;
                    paragraph.Inlines.Add($"В {GetCurrentTime()} лог: {(string)args.Value}");
                    ConsoleWrite(paragraph);
                    break;
            }
        }

        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public void ConsoleWrite(Paragraph paragraph)
        {
            ConsoleBox.Document.Blocks.InsertBefore(ConsoleBox.Document.Blocks.LastBlock, paragraph);
            ConsoleBox.ScrollToEnd();
        }

        private void ProgramCompleted(CompletedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                ProgramResultBox.Foreground = Brushes.Lime;
                ProgramResultBox.TextDecorations = null;

                var goodEndParagraph = new Paragraph();
                goodEndParagraph.Foreground = Brushes.Gray;
                goodEndParagraph.Inlines.Add($"\nВ {GetCurrentTime()}: Программа завершилась успешно.");
                ConsoleWrite(goodEndParagraph);
                ProgramResultBox.Text = $"Программа успешно выполненена за {App.LangLineProgram.MainField.GetPositions().Count} шагов.";
            });
        }

        private void ProgramException(ExceptionEventArgs args)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var stacktrace = App.LangLineProgram.StackTrace;
                    stacktrace.Reverse();

                    var exceptionParagraph = new Paragraph();
                    exceptionParagraph.Foreground = Brushes.Red;
                    exceptionParagraph.Inlines.Add($"\nВ {GetCurrentTime()} произошла ошибка:");
                    foreach (var exception in stacktrace)
                    {

                        Hyperlink lineLink = new Hyperlink();
                        lineLink.Inlines.Add($"\n{exception.Exception.Message}");
                        lineLink.Foreground = Brushes.Red;
                        lineLink.Tag = exception.Line;
                        lineLink.Click += ConsoleLinkClicked;
                        exceptionParagraph.Inlines.Add(lineLink);
                        foreach (var paragraph in CodeInput.Document.Blocks.ToList().Where(line => (int)line.Tag == exception.Line))
                        {
                            paragraph.Foreground = Brushes.Red;
                        }
                    }
                    exceptionParagraph.Inlines.Add("\n");

                    var arg = args;
                    ConsoleWrite(exceptionParagraph);
                    ProgramResultBox.Foreground = Brushes.Red;
                    ProgramResultBox.TextDecorations = TextDecorations.Underline;
                    ProgramResultBox.Text = $"{stacktrace.Last().Exception.Message}";
                });
            } catch(Exception ex)
            {
                var exceptionParagraph = new Paragraph();
                exceptionParagraph.Foreground = Brushes.Red;
                exceptionParagraph.Inlines.Add("Внутренняя ошибка: " + ex.Message);
                ConsoleWrite(exceptionParagraph);
            };
            var badEndParagraph = new Paragraph();
            badEndParagraph.Foreground = Brushes.Gray;
            badEndParagraph.Inlines.Add($"В {GetCurrentTime()}: Программа завершилась с ошибкой.");
            ConsoleWrite(badEndParagraph);
        }


        public void ResetCommandList()
        {
            int i = 0;
            foreach (Paragraph paragraph in CodeInput.Document.Blocks.ToList())
            {
                i++;
                paragraph.Foreground = Brushes.White;
                paragraph.Tag = i;
            }
            ProgramResultBox.Foreground = Brushes.White;
            ProgramResultBox.Text = "";
            ProgramResultBox.TextDecorations = TextDecorations.Baseline;
            App.LangLineProgram.SetCommandsFromFlowDocument(CodeInput.Document);
        }

        private void MouseWheel_Zooming(object sender, MouseWheelEventArgs e)
        {
            var scale = (ScaleTransform)Field.RenderTransform;
            if ((scale.ScaleX <=0.4 && e.Delta < 0) || (scale.ScaleX>=10.5 && e.Delta>0))
                return;
            scale.CenterX = Mouse.GetPosition(Field).X;
            scale.CenterY = Mouse.GetPosition(Field).Y;
            if (e.Delta > 0)
            {
                scale.ScaleX *= 1.1;
                scale.ScaleY *= 1.1;
            } else
            {
                scale.ScaleX /= 1.1;
                scale.ScaleY /= 1.1;
            }

        }

        private void FieldScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            StartProgram();
        }


        public void StartProgram()
        {
            IsPaused = false;
            TogglePRButton(PRStatuses.Pause);
            ToggleStopButton(true);
            ResetCommandList();
            App.LangLineProgram.StartProgram();
            var positions = App.LangLineProgram.MainField.GetPositions();
            App.MainGrid.UpdatePositions(positions);
            App.MainGrid.StartMovement();
        }

        private void CurrentIntervalValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized)
                return;
            var value = (int)CurrentIntervalSlider.Value * 10;
            App.ExecutionInterval = value;
            CurrentIntervalText.Text = value + " мс";
        }


        private void CodeInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control
                && e.Key == Key.V)
            {
                if (Clipboard.GetData("Text") != null)
                    Clipboard.SetText((string)Clipboard.GetData("Text"), TextDataFormat.Text);
                else
                    e.Handled = true;
            }

            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift
                && e.Key == Key.Enter)
            {
                e.Handled = true;
                ((RichTextBox)sender).Document.Blocks.Add(new Paragraph());
            }
        }

        private void CodeInputPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                e.Handled = true;
                if (e.Delta > 0)
                {
                    if (CodeInput.FontSize < 64)
                        CodeInput.FontSize += 1;
                }
                else if (e.Delta < 0)
                {
                    if(CodeInput.FontSize > 5)
                        CodeInput.FontSize -= 1;
                }
            }
                
        }

        private void ConsoleLinkClicked(object sender, RoutedEventArgs e)
        {
            var reference = (Hyperlink)e.Source;
            var line = (int)reference.Tag;

            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                    new Action(delegate ()
                    {
                        CodeInput.Focus();
                    }
            ));
            FocusOnParagraph(line);
        }

        public void FocusOnParagraph(int line)
        {
            if (line <= CodeInput.Document.Blocks.Count)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                        new Action(delegate ()
                        {
                            Block block = CodeInput.Document.Blocks.ElementAt(line-1);
                            CodeInput.CaretPosition = block.ContentEnd;
                        }));
            }
        }

        private void CodeInputCaretChanged(object sender, RoutedEventArgs e)
        {
            TextPointer tp1 = CodeInput.Selection.Start.GetLineStartPosition(0);
            TextPointer tp2 = CodeInput.Selection.Start;

            int column = tp1.GetOffsetToPosition(tp2);

            int someBigNumber = int.MaxValue;
            int lineMoved, currentLineNumber;
            CodeInput.Selection.Start.GetLineStartPosition(-someBigNumber, out lineMoved);
            currentLineNumber = -lineMoved;

            CodeInputStatus.Text = $"Строка {currentLineNumber+1}, символ {(column < 1 ? 1 : column)}";
        }

        private void ToggleStopButton(bool flag)
        {
            var template = StopButton.Template;

            var StopImage = (Image)template.FindName("StopImage", StopButton);
            var StopText = (TextBlock)template.FindName("StopText", StopButton);

            if (flag)
            {
                StopImage.Source = (BitmapImage)App.Current.FindResource("StopImage");
                StopText.Foreground = Brushes.White;
                StopButton.IsEnabled = true;
            } else
            {
                StopImage.Source = (BitmapImage)App.Current.FindResource("StopDisabledImage");
                StopText.Foreground = Brushes.Gray;
                StopButton.IsEnabled = false;
            }
        }
        
        private bool IsPaused = false;
        private void TogglePRButton(PRStatuses flag)
        {
            var template = PauseResumeButton.Template;

            var PRImage = (Image)template.FindName("PauseResumeImage", PauseResumeButton);
            var PRText = (TextBlock)template.FindName("PauseResumeText", PauseResumeButton);


            if (flag == PRStatuses.Resume)
            {
                PRImage.Source = (BitmapImage)App.Current.FindResource("ResumeImage");
                PauseResumeButton.IsEnabled = true;
                PRText.Foreground = Brushes.White;
                PRText.Text = "Возобновить";
                IsPaused = true;
            } else if(flag == PRStatuses.Pause)
            {
                PRImage.Source = (BitmapImage)App.Current.FindResource("PauseImage");
                PauseResumeButton.IsEnabled = true;
                PRText.Foreground = Brushes.White;
                PRText.Text = "Остановить";
                IsPaused = false;
            } else
            {
                PRImage.Source = (BitmapImage)App.Current.FindResource("ResumeDisabledImage");
                PauseResumeButton.IsEnabled = false;
                PRText.Foreground = Brushes.Gray;
                PRText.Text = "Возобновить";
            }
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            App.MainGrid.StopMovement();
        }

        private void PauseResumeButtonClick(object sender, RoutedEventArgs e)
        {
            TogglePR();
        }

        private void TogglePR()
        {
            if (PauseResumeButton.IsEnabled) {
                IsPaused = !IsPaused;
                if (IsPaused)
                {
                    TogglePRButton(PRStatuses.Resume);
                    App.MainGrid.Pause();
                }
                else
                {
                    TogglePRButton(PRStatuses.Pause);
                    App.MainGrid.Resume();
                }
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Space)
                {
                    TogglePR();
                }

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    StartProgram();
                }

                if(e.Key == Key.Right)
                {
                    StepForward();
                }

                if(e.Key == Key.Left)
                {
                    StepBackward();
                }
            }
        }

        private void StepForward() => App.MainGrid.StepForward();
        private void StepBackward() => App.MainGrid.StepBackward();

        private void ClickBackward(object sender, RoutedEventArgs e)
        {
            StepBackward();
        }

        private void ClickForward(object sender, RoutedEventArgs e)
        {
            StepForward();
        }
    }
}
