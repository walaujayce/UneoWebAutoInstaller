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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UneoWebApplicationAutoInstaller.Models;
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
            //var animation = new DoubleAnimation
            //{
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            //};
            //HighlightTransform.BeginAnimation(TranslateTransform.XProperty, animation);

            //configurationVM.ConfigurationMode = (int)EUcbConfigurationMode.LibUSB;
        }

        private void SelectFt4222h_Clicked(object sender, MouseButtonEventArgs e)
        {
            //var animation = new DoubleAnimation
            //{
            //    To = 70, // slide distance in px
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            //};
            //HighlightTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            //configurationVM.ConfigurationMode = (int)EUcbConfigurationMode.FT4222h;
        }

        private void BackToPrevious_Configuration_Clicked(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void ConnectButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "LeftClickUp";
            configurationVM.ConnectAndReadLibUSB();
        }
        private void WriteButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "LeftClickUp";
            configurationVM.WriteWiFiConfig();
        }
        private void SaveButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "LeftClickUp";
            configurationVM.SaveCurrentValueAsTemplate();
        }
        private void WriteWithTemplateButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "LeftClickUp";
            configurationVM.WriteWiFiConfigWithTemplate();
        }
        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var inputBox = (TextBox)sender;
            configurationVM.InputBoxSelected = (DictionaryInput)inputBox.DataContext;
            configurationVM.InputBoxSelected.DictionaryValue = (string)inputBox.Text;
            configurationVM.PrintCurrentSelectedInputBoxValue();
        }
        private void SelectedInputBox_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            configurationVM.InputBoxSelected = (DictionaryInput)button.DataContext;
            //configurationVM.InputBoxSelected.DictionaryValue = (DictionaryInput)button.DataContext;  
        }
        private void CopyWifiName_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "MouseDown";
            if (!string.IsNullOrEmpty(configurationVM.ConnectedWifiName))
            {
                Clipboard.SetText(configurationVM.ConnectedWifiName);
            }
        }
        private void CopyWifiPassword_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "MouseDown";
            if (!string.IsNullOrEmpty(configurationVM.ConnectedWifiPassword))
            {
                Clipboard.SetText(configurationVM.ConnectedWifiPassword);
            }
        }
        private void CopyIPv4Address_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "MouseDown";
            configurationVM.IpAddressSelected = (DictionaryInput)button.DataContext;
            if (!string.IsNullOrEmpty(configurationVM.IpAddressSelected.DictionaryValue.ToString()))
            {
                Clipboard.SetText(configurationVM.IpAddressSelected.DictionaryValue.ToString());
            }
        }
        private void RefreshWifiAndIpv4List_Clicked(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation rotation = new DoubleAnimation
            {
                From = 0,
                To = 180,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                RepeatBehavior = new RepeatBehavior(1)
            };
            //rotation.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut };

            RefreshRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotation);

            configurationVM.GetIpAddressList();
            configurationVM.GetWifiNameAndPassword();
        }
        private void LeftMouseButtonDown_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "LeftClickDown";
        }
        private void MouseLeaveCaptured(object sender, MouseEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "MouseLeave";
        }
        private void CursorStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = (Border)sender;
            button.Tag = "MouseLeave";
        }

        private void RefreshIcon_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            if (image?.RenderTransform is RotateTransform rotate)
            {
                rotate.CenterX = image.ActualWidth / 2;
                rotate.CenterY = image.ActualHeight / 2;
            }
        }
    }
}
