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
using UneoWebApplicationAutoInstaller.ViewModels;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for ProgressMonitor.xaml
    /// </summary>
    public partial class ProgressMonitor : Page
    {
        private ProgressMonitorViewModel _progressMonitorVM; 
        
        public ProgressMonitor()
        {
            InitializeComponent();
            _progressMonitorVM = new ProgressMonitorViewModel();
            DataContext = _progressMonitorVM;
        }

        private void BackToPrevious_ProgressMonitor_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ProceedToNextStage_ProgressMonitor_Clicked(object sender, MouseButtonEventArgs e)
        {
        }
        public void SetDelegate(DelegateNavigate del)
        {
            _progressMonitorVM.SetDelegateNavigate(del);
        }
    }
}
