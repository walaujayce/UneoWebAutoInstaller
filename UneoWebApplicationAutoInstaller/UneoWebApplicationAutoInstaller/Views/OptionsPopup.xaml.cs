using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for OptionsPopup.xaml
    /// </summary>
    public partial class OptionsPopup : UserControl
    {
        public OptionsPopup()
        {
            InitializeComponent();
        }
        public void ShowPopup()
        {
            OptionPopupList.IsOpen = true;
        }
        private void OptionListBoxItem_SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OptionsListBox.SelectedItem is ListBoxItem selectedItem)
            {
                Debug.WriteLine($"Selected: {selectedItem.Content}");
            }
            Thread.Sleep(100); //if pop up hide so fast will miss click the add key value pair items button
            OptionPopupList.IsOpen = false;
        }
    }
}
