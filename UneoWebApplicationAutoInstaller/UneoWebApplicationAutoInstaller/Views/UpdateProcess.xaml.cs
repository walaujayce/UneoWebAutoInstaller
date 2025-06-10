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

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for UpdateProcess.xaml
    /// </summary>
    public partial class UpdateProcess : Page
    {
        private UpdateProcessViewModel _updateProcessVM;
        public UpdateProcess()
        {
            InitializeComponent();
            _updateProcessVM = new UpdateProcessViewModel();
            DataContext = _updateProcessVM;
        }

        private void BackToPrevious_Update_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ProceedToNextStage_Update_Clicked(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
