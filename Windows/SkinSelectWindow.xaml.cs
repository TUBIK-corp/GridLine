using GridLine_IDE.Helpers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GridLine_IDE.Windows
{
    /// <summary>
    /// Логика взаимодействия для SkinSelectWindow.xaml
    /// </summary>
    public partial class SkinSelectWindow : Window
    {
        public SkinItem SelectedsSkin { get; private set; } = null;
        private bool Saved = false;

        public SkinSelectWindow()
        {
            InitializeComponent();
            SkinSelectedBox.Source = App.CurrentSkin.ImageSource;
            SkinName.Text = App.CurrentSkin.Name;
            InitSkins();
        }


        public List<SkinItem> Skins = null;

        public void InitSkins()
        {
            var availableSkins = new List<SkinItem>
            {
                new SkinItem("UserBlueRobotImage", "Синий пылесос"),
                new SkinItem("UserRedRobotImage", "Красный пылесос"),
                new SkinItem("UserDarkBlueRobotImage", "Тёмно-синий пылесос"),
                new SkinItem("UserGrayRobotImage", "Серый пылесос"),
                new SkinItem("UserPomniImage", "Помни")
            };

            Skins = availableSkins;
            SkinBox.ItemsSource = Skins;
        }

        public class SkinItem
        {
            public BitmapImage ImageSource { get; set; }

            public string BitmapName { get; set; }

            public string Name { get; set; }

            public SkinItem(string bitmapName, string name)
            {
                ImageSource = GetBitmapImage(bitmapName);
                BitmapName = bitmapName;
                Name = name;
            }

            public BitmapImage GetBitmapImage(string name)
            {
                return (BitmapImage)Application.Current.FindResource(name);
            }
        }


        private void SkinChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SkinBox.SelectedItems.Count < 1)
            {
                ToggleButton(false);
                return;
            }

            var skinData = SkinBox.SelectedItem as SkinItem;

            SkinSelectedBox.Source = skinData.ImageSource; 
            SkinName.Text = skinData.Name;

            SelectedsSkin = skinData;

            ToggleButton(true);
        }
        private void ToggleButton(bool flag)
        {
            var template = SaveButton.Template;

            var SaveText = (TextBlock)template.FindName("SaveText", SaveButton);

            if (flag)
            {
                SaveText.Foreground = Brushes.White;
                SaveButton.IsEnabled = true;
            }
            else
            {
                SaveText.Foreground = Brushes.Gray;
                SaveButton.IsEnabled = false;
            }
        }


        private void SelectClicked(object sender, RoutedEventArgs e)
        {
            Saved = true;
            Close();
        }

        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Saved)
            {
                App.CurrentSkin.ImageSource = SelectedsSkin.ImageSource;
                App.CurrentSkin.Name = SelectedsSkin.Name;
                ConfigHelper.Config.SkinName = SelectedsSkin.BitmapName;
                ConfigHelper.Config.IconName = SelectedsSkin.Name;
                ConfigHelper.WriteConfig();
            }
        }
    }
}
