using GridLine_IDE.Database;
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
    /// Логика взаимодействия для OpenFromDatabaseWindow.xaml
    /// </summary>
    public partial class OpenFromDatabaseWindow : Window
    {
        public OpenFromDatabaseWindow()
        {
            InitializeComponent();
            SelectBox.ItemsSource = DatabaseHelper.GetAllPrograms();
        }

        private ProgramCode _program = null;

        public ProgramCode GetProgram() => _program;

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectBox.SelectedItems.Count > 0)
                ToggleButton(true);
            else ToggleButton(false);
            _program = (ProgramCode)SelectBox.SelectedItem;
        }

        private void ToggleButton(bool flag)
        {
            var template = OpenButton.Template;

            var StopText = (TextBlock)template.FindName("BlockTextOfSend", OpenButton);

            if (flag)
            {
                StopText.Foreground = Brushes.White;
                OpenButton.IsEnabled = true;
            }
            else
            {
                StopText.Foreground = Brushes.Gray;
                OpenButton.IsEnabled = false;
            }
        }

        private bool _checked = false;
        private void SelectButton(object sender, RoutedEventArgs e)
        {
            _checked = true;
            Close();
        }

        private void CloseClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!_checked)
                _program = null;
        }
    }
}
