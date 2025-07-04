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
    /// Interaction logic for DiagnosticProcess.xaml
    /// </summary>
    public partial class DiagnosticProcess : Page
    {
        private DiagnosticViewModel diagnosticProcessVM;
        public DiagnosticProcess()
        {
            InitializeComponent();
            diagnosticProcessVM = new DiagnosticViewModel();
            DataContext = diagnosticProcessVM;
        }

        private void BackToPrevious_Diagnostic_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
