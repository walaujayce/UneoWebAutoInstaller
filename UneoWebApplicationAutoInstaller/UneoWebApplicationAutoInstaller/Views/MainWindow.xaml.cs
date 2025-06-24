using System.Security.Cryptography.X509Certificates;
using System.Text;
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
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.ViewModels;
using UneoWebApplicationAutoInstaller.Views;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel mainVM;

        public MainWindow()
        {
            InitializeComponent();
            var navigationService = new FrameNavigationService(Navigation);
            mainVM = new MainWindowViewModel(navigationService);
            DataContext = mainVM;
        }
        private void CloseWindow_Clicked(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void DragWindow_Click(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AvoidDragWindow_Clicked(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Prevents the drag from being triggered
        }

        private void CheckVersion_Clicked(object sender, MouseButtonEventArgs e)
        {
            mainVM.CheckVersion();
        }
    }
}