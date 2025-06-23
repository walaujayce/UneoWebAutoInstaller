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
    /// Interaction logic for ProcessSelection.xaml
    /// </summary>
    public partial class ProcessSelection : Page
    {
        private ProcessSelectionViewModel _processSelectionVM;
        public ProcessSelection()
        {
            InitializeComponent();
            _processSelectionVM = new ProcessSelectionViewModel();
            DataContext = _processSelectionVM;
            
            this.Focus();
        }
        private void SelectProcess_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            _processSelectionVM.ProcessSelected = (Process)button.DataContext;
            _processSelectionVM.ProcessToNextStage();
        }
        public void SetDelegate(DelegateNavigate del)
        {
            _processSelectionVM.SetDelegateNavigate(del);
        }
        private void ProceedToNextStage_Clicked(object sender, MouseButtonEventArgs e)
        {
            _processSelectionVM.ProcessToNextStage();
        }

        private void ProcessSelection_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _processSelectionVM.ProcessToNextStage();
                e.Handled = true;
            }
        }

        private void ProcessBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = (Border)sender;
            _processSelectionVM.ProcessSelected = (Process)button.DataContext;
            _processSelectionVM.ProcessBorderHover(true);
        }

        private void ProcessBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = (Border)sender;
            _processSelectionVM.ProcessSelected = (Process)button.DataContext;
            _processSelectionVM.ProcessBorderHover(false);

        }

        private void ProcessBorder_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            _processSelectionVM.ProcessSelected = (Process)button.DataContext;
            _processSelectionVM.ProcessSelectedToProceed = (Process)button.DataContext;
            _processSelectionVM.ProcessBorderMouseButtonDown();
        }
    }
}
