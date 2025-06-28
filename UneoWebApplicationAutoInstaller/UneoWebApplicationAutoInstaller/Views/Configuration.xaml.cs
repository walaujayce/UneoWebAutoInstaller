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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UneoWebApplicationAutoInstaller.ViewModels;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : Page
    {
        private ConfigurationViewModel configurationVM;
        public Configuration()
        {
            InitializeComponent();
            configurationVM = new ConfigurationViewModel();
            DataContext = configurationVM;
        }

        private void SelectLibUSB_Clicked(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            HighlightTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            
            configurationVM.ConfigurationMode = (int)EUcbConfigurationMode.LibUSB;
        }

        private void SelectFt4222h_Clicked(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 70, // slide distance in px
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            HighlightTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            configurationVM.ConfigurationMode = (int)EUcbConfigurationMode.FT4222h;
        }

        private void BackToPrevious_Configuration_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
