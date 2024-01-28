using GridLine_IDE.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GridLine_IDE.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddToDatabaseWindow.xaml
    /// </summary>
    public partial class AddToDatabaseWindow : Window
    {

        private List<string> _commands = new List<string>();
        public AddToDatabaseWindow(List<string> commandLines)
        {
            InitializeComponent();
            _commands = commandLines;
        }

        private void SaveToDBClicked(object sender, RoutedEventArgs e)
        {
            SaveToDb();
        }
        private void SaveToDb()
        {
            var program = DatabaseHelper.SelectData(x => x.Name.ToLower().Equals(ProgramNameBox.Text.ToLower()));
            if (program != null && program.Count > 0)
            {
                ProgramNameStatus.Text = "Программа с таким именем уже существует";
                return;
            }
            ProgramNameStatus.Text = "";

            DatabaseHelper.AddProgram(ProgramNameBox.Text, _commands);
            this.Close();
            MessageBox.Show($"Программа {ProgramNameBox.Text} успешно добавлена.");
        }

        private void ToggleButton(bool flag)
        {
            var template = SendButton.Template;

            var StopText = (TextBlock)template.FindName("SendButtonText", SendButton);

            if (flag)
            {
                StopText.Foreground = Brushes.White;
                SendButton.IsEnabled = true;
            }
            else
            {
                StopText.Foreground = Brushes.Gray;
                SendButton.IsEnabled = false;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SaveToDb();
            }
        }

        private void ProgramNameChanged(object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(ProgramNameBox.Text))
                ToggleButton(true);
            else ToggleButton(false);
        }
    }
}
