using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.ViewModels;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for InformationCenter.xaml
    /// </summary>
    public partial class InformationCenter : Window
    {
        private InformationCenterViewModel informationCenterVM;
        public InformationCenter()
        {
            InitializeComponent();
            informationCenterVM = new InformationCenterViewModel();
            DataContext = informationCenterVM;
        }
        public void SetDelegateOverlayShow(DelegateOverlayShow del)
        {
            informationCenterVM.SetVMDelegateOverlayShow(del);
        }
        public void SetInformationList(string title, List<string> informationList)
        {
            informationCenterVM.InformationListener(title, informationList);
        }
        private void CloseBtn_Clicked(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            informationCenterVM.DeInit();
        }

        private void ConfirmButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            informationCenterVM.DeInit();
        }

        private void Download_Clicked(object sender, MouseButtonEventArgs e)
        {
            informationCenterVM.DownloadFile();
        }
    }
}
