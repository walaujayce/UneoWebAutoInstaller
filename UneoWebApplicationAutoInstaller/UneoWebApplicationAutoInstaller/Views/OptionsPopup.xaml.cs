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
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.ViewModels;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for OptionsPopup.xaml
    /// </summary>
    public partial class OptionsPopup : UserControl
    {
        private OptionsPopupViewModel optionsPopupVM;
        public OptionsPopup()
        {
            InitializeComponent();
            optionsPopupVM = new OptionsPopupViewModel();
            DataContext = optionsPopupVM;
        }
        public void ShowPopup()
        {
            //INIT ALL AVAIABLE IP ADDRESS
            optionsPopupVM.Init();
            OptionPopupList.IsOpen = true;
        }
        private void OptionsList_Selected(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            optionsPopupVM.OptionSelected = (Option)button.DataContext;
            Thread.Sleep(100); //if pop up hide so fast will miss click the add key value pair items button
            OptionPopupList.IsOpen = false;
        }
    }
}
