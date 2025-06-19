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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.ViewModels;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for InstallProcess.xaml
    /// </summary>
    public partial class InstallProcess : Page
    {
        InstallProcessViewModel installProcessVM;
        public InstallProcess()
        {
            InitializeComponent();
            installProcessVM = new InstallProcessViewModel();
            DataContext = installProcessVM;
            this.Focus();
        }
        public void SetDelegate(DelegateNavigate del)
        {
            installProcessVM.SetDelegateNavigate(del);
        }
        public void SetDelegateOverlayShow(DelegateOverlayShow del)
        {
            installProcessVM.SetVMDelegateOverlayShow(del);
        }        
        public void SetDelegateSelectedInstallation(DelegateSelectedInstallation del)
        {
            installProcessVM.SetVMDelegateSelectedInstallation(del);
        }
        private void BackToPrevious_Install_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.GoBack();
        }
        private void ProceedToNextStage_Install_Clicked(object sender, MouseButtonEventArgs e)
        {
            installProcessVM.ProceedToInstallation();
        }
        private void InstallProcess_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                installProcessVM.ProceedToInstallation();
                e.Handled = true;
            }
        }
    }
}
