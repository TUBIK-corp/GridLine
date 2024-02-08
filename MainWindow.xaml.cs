using GridLine_IDE.Enums;
using GridLine_IDE.Helpers;
using GridLine_IDE.Models;
using GridLine_IDE.NewCommands;
using GridLine_IDE.Windows;
using LangLine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ExceptionEventArgs = LangLine.Models.ExceptionEventArgs;
namespace GridLine_IDE
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupProgram();
            FieldBorder.AddHandler(UIElement.PreviewMouseWheelEvent,
                 new MouseWheelEventHandler(MouseWheel_Zooming),
                 true // Handler will be called even though e.Handled = true
                );
        }

        private void UpdateStatus()
        {
            Dispatcher.Invoke(() =>
            {
                var currentIndex = App.MainGrid.GetCurrentMoveIndex();
                var maxIndex = App.MainGrid.Positions.Count - 1;

                StepStatus.Text = $"{(currentIndex > -1 ? (currentIndex < App.MainGrid.Positions.Count - 1 ? currentIndex : App.MainGrid.Positions.Count - 1) : 0)} из {(maxIndex > -1 ? maxIndex : 0)}";

            });
        }

        public void TestDatabase()
        {
           var name = DatabaseHelper.AddTestProgram();
           MessageBox.Show(name);
        }

        private void GridMoveEnded()
        {
            Dispatcher.Invoke(() =>
            {
                ToggleStopButton(false);
                TogglePRButton(PRStatuses.DisabledResume);
                UpdateStatus();
            });
        }

        public void SetupProgram(Config config = null)
        {
            Field.Children.Clear();
            if (config == null)
                InitLangLine();
            else ReInitLangLine(config);
            App.LangLineProgram.InterpreterModule.OnCompleted += ProgramCompleted;
            App.LangLineProgram.InterpreterModule.OnException += ProgramException;
            App.MainGrid = new MovementField(GridHelper.CreateGrid(App.LangLineProgram.MainField.Width, App.LangLineProgram.MainField.Height));
            Field.Children.Add(App.MainGrid.Field);
            App.MainGrid.MoveEnded += GridMoveEnded;
            App.MainGrid.Moved += UpdateStatus;
            App.LangLineProgram.InterpreterModule.OnCustomEvent += CustomEventInvoked;
            App.LangLineProgram.RegisterCommand("LOG", typeof(LogCommand));
            UpdateStatus();
        }

        public void InitLangLine()
        {
            App.LangLineProgram = new LangLine.LangLineCore(ConfigHelper.Config.Width, ConfigHelper.Config.Height);
            App.LangLineProgram.SetSpawnPoint(new Point(ConfigHelper.Config.StartX, ConfigHelper.Config.StartY));
            App.LangLineProgram.LimitNesting(ConfigHelper.Config.LimitNesting);
            App.LangLineProgram.LimitVariable(ConfigHelper.Config.LimitMaxValue);
        }
        public void ReInitLangLine(Config config)
        {
            App.LangLineProgram = new LangLine.LangLineCore(config.Width, config.Height);
            App.LangLineProgram.SetSpawnPoint(new Point(config.StartX, config.StartY));
            App.LangLineProgram.LimitNesting(config.LimitNesting);
            App.LangLineProgram.LimitVariable(config.LimitMaxValue);
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
                    UpdateStatus();
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
            UpdateStatus();
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
                PRText.Text = "Пауза";
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
        private bool _hasChanges = false;

        public bool HasChanges 
        { 
            get => _hasChanges; 
            set
            {
                _hasChanges = value;
                ChangeSavedStatus();
            }
        }

        public void ChangeSavedStatus()
        {
            if(HasChanges)
                SavedStatus.Visibility = Visibility.Visible;
            else SavedStatus.Visibility = Visibility.Hidden;
        }

        private string _openedPath = string.Empty;

        public string OpenedPath { 
            get => _openedPath; 
            set
            {
                _openedPath = value;
                ChangeTitleWithPath();
            } 
        }

        public void ChangeTitleWithPath()
        {
            if (string.IsNullOrWhiteSpace(OpenedPath))
                Title = "GridLine IDE";
            else if (!Title.Equals("GridLine IDE - " + OpenedPath))
                Title = "GridLine IDE - " + OpenedPath;
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                    TogglePR();
                }

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    StartProgram();
                }

                if (e.Key == Key.Right)
                {
                    StepForward();
                    e.Handled = true;
                }

                if (e.Key == Key.Left)
                {
                    StepBackward();
                    e.Handled = true;
                }

                if (e.Key == Key.S)
                {
                    SaveChanges();
                    e.Handled = true;
                }
                if (e.Key == Key.O)
                {
                    OpenFile();
                    e.Handled = true;
                }
            }
            else if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (e.Key == Key.S)
                {
                    SaveChanges(true);
                    e.Handled = true;
                }
            } else if(e.Key == Key.F1)
            {
                OpenHelpMenu();
            }
        }

        public void OpenHelpMenu()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/TUBIK-corp/GridLine",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии справки: {ex.Message}");
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

        private void SaveChangesClicked(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }


        public bool SaveFile(bool ignoreCreated = false)
        {
            if (string.IsNullOrWhiteSpace(OpenedPath) || ignoreCreated)
            {
                using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        OpenedPath = saveFileDialog.FileName;
                    } else
                    {
                        return false;
                    }
                }
            }
            if (!File.Exists(OpenedPath))
            {
                File.Create(OpenedPath).Close();
            }
            File.WriteAllLines(OpenedPath, GetCommandLines());
            HasChanges = false;
            return true;
        }

        public void OpenFile()
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OpenedPath = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            if (HasChanges)
            {
                var messageBox = MessageBox.Show("У вас остались несохранённые изменения. Сохранить их?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageBox == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (messageBox == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            CodeInput.Document = new FlowDocument();
            foreach (string line in File.ReadAllLines(OpenedPath))
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(line.Trim('\n').Trim('\r'));
                CodeInput.Document.Blocks.Add(paragraph);
            }
            HasChanges = false;
            ToggleStopButton(false);
            TogglePRButton(PRStatuses.DisabledResume);
        }

        public List<string> GetCommandLines()
        {
            var list = new List<string>(); 
            foreach (Paragraph paragraph in CodeInput.Document.Blocks)
            {
                string text = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                text = text.TrimEnd('\n').TrimEnd('\r').Replace('\\', '/');
                list.Add(text);
            }
            return list;
        }

        public void SaveChanges(bool ignoreCreated = false)
        {
            if (HasChanges)
            {
                SaveFile(ignoreCreated);
            }
        }

        private void CodeInputCodeChanged(object sender, TextChangedEventArgs e)
        {
            if(!Keyboard.IsKeyDown(Key.LeftCtrl) 
                && !Keyboard.IsKeyDown(Key.RightCtrl))
                HasChanges = true;
        }

        private void OpenFileClicked(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void SaveChangesAsClicked(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void OpenInBDClicked(object sender, RoutedEventArgs e)
        {
            OpenFromDatabaseWindow window = new OpenFromDatabaseWindow();
            window.ShowDialog();
            if(window.GetProgram() != null)
            {
                if (HasChanges)
                {
                    var messageBox = MessageBox.Show("У вас остались несохранённые изменения. Сохранить их?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (messageBox == MessageBoxResult.Yes)
                    {
                        if (!SaveFile())
                            return;
                    }
                    else if (messageBox == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                var program = window.GetProgram();
                var program_config = ConfigHelper.UnpackConfig(program.Config);
                if(program_config.Width != App.LangLineProgram.MainField.Width
                    || program_config.Height != App.LangLineProgram.MainField.Height
                    || program_config.StartX != App.LangLineProgram.SpawnPoint.X
                    || program_config.StartY != App.LangLineProgram.SpawnPoint.Y)
                {
                    var messageBox = MessageBox.Show("Данный файл содержит иные конфигурационные данные. Использовать новую конфигурацию?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    
                    if (messageBox == MessageBoxResult.Yes)
                    {
                        SetupProgram(program_config);
                    }
                    else if (messageBox == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                var positions = JsonConvert.DeserializeObject<List<Point>>(program.Positions);
                if(positions != null && positions.Count>1)
                {
                    positions.RemoveAt(0);
                    App.MainGrid.UpdatePositions(positions);
                    UpdateStatus();
                }
                
                CodeInput.Document = new FlowDocument();
                foreach (string line in program.Code.Split('\n'))
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(line.Trim('\n').Trim('\r'));
                    CodeInput.Document.Blocks.Add(paragraph);
                }

                OpenedPath = string.Empty;
                HasChanges = false;
                ToggleStopButton(false);
                TogglePRButton(PRStatuses.DisabledResume);
            }

        }

        private void SaveToDBClicked(object sender, RoutedEventArgs e)
        {
            AddToDatabaseWindow window = new AddToDatabaseWindow(GetCommandLines());
            window.ShowDialog();
        }

        private void HelpClicked(object sender, RoutedEventArgs e)
        {
            OpenHelpMenu();
        }

        private void CustomizeClicked(object sender, RoutedEventArgs e)
        {
            SkinSelectWindow window = new SkinSelectWindow();
            window.ShowDialog();
            App.MainGrid.ReloadIcon();
            e.Handled = true;
        }

        private void RestartClicked(object sender, RoutedEventArgs e)
        {
            if (HasChanges)
            {
                var messageBox = MessageBox.Show("У вас остались несохранённые изменения. Сохранить их?", "Внимание!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageBox == MessageBoxResult.Yes)
                {
                    if (!SaveFile())
                        return;
                }
                else if (messageBox == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            HasChanges = false;
            SetupProgram();
            e.Handled = true;
        }
    }
}
