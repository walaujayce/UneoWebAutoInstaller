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
using System.Windows.Shapes;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.ViewModels;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.Views
{
    /// <summary>
    /// Interaction logic for SettingModal.xaml
    /// </summary>
    public partial class SettingModal : Window
    {
        private SettingViewModel settingVM;
        public SettingModal()
        {
            InitializeComponent();
            settingVM = new SettingViewModel();
            DataContext = settingVM;
            OptionPopupList.ViewModel.OptionSelectedChanged += OnOptionSelectedChanged;
            OptionPopupList.ViewModel.Init(); // Load the options list

        }
        private void CloseBtn_Clicked(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            settingVM.DeInit();
        }
        private void BackToPrevious_SettingModal_Clicked(object sender, MouseButtonEventArgs e)
        {
            settingVM.BackToPreviousInstallationSettings();
            if (settingVM.CloseSettingModal())
            {
                this.Close();
                settingVM.DeInit();
            }
        }
        private void ProceedToNextStage_SettingModal_Clicked(object sender, MouseButtonEventArgs e)
        {
            settingVM.GoToNextInstallationSettings();
            if (settingVM.CloseSettingModal())
            {
                this.Close();
                settingVM.DeInit();
            }
        }
        public void SetDelegateOverlayShow(DelegateOverlayShow del)
        {
            settingVM.SetVMDelegateOverlayShow(del);
        }        
        public void SetDelegateInstallationData(DelegateInstallationData del)
        {
            settingVM.SetVMDelegateInstallationData(del);
        }
        
        public void SetSelectedInstallation(List<Install> selectedInstallation)
        {       
            settingVM.SelectedInstallationListener(selectedInstallation);
        }
        
        public void SetSelectedUpdate(Update selectedUpdate)
        {
            settingVM.SelectedUpdateListener(selectedUpdate);
        }
        public void SetProcessMode(int processMode)
        {
            settingVM.ProcessSettingMode = processMode;
        }
        
        private void AddInputBtn_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            settingVM.InputSelected = (DictionaryInput)button.DataContext;
            settingVM.AddInputItems();
        }
        private void AddKeyValuePairBtn_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            settingVM.KeyValuePairSelected = (DictionaryInput)button.DataContext;
            settingVM.AddKeyValuePair();
        }
        private void SetCurrentSettingSelected_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Grid)sender;
            settingVM.SettingSelected = (Setting)button.DataContext;
        }
        private void SettingValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var button = (TextBox)sender;
            settingVM.SettingSelected = (Setting)button.DataContext;
            settingVM.SettingSelected.SettingValue = (string)button.Text;
        }
        private void DictionaryKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            var button = (TextBox)sender;
            settingVM.KeyValuePairSelected = (DictionaryInput)button.DataContext;
            settingVM.KeyValuePairSelected.DictionaryKey = (string)button.Text;
            //settingVM.CheckSelectedObject();
        }
        private void DictionaryValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var button = (TextBox)sender;
            settingVM.KeyValuePairSelected = (DictionaryInput)button.DataContext;
            settingVM.KeyValuePairSelected.DictionaryValue = (string)button.Text;
            //settingVM.CheckSelectedObject();
        }
        private void OnOptionSelectedChanged(object? sender, OptionSelectedEventArgs e)
        {
            //settingVM.CheckSelectedObject();
            settingVM.KeyValuePairSelected.DictionaryValue = e.SelectedValue;
        }
        private void OptionBtn_Clicked(object sender, MouseButtonEventArgs e)
        {
            var button = (Border)sender;
            settingVM.KeyValuePairSelected = (DictionaryInput)button.DataContext;
            OptionPopupList.ShowPopup();
        }

        private void SettingModal_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                settingVM.GoToNextInstallationSettings();
                if (settingVM.CloseSettingModal())
                {
                    this.Close();
                    settingVM.DeInit();
                }
            }else if (e.Key == Key.Escape)
            {
                this.Close();
                settingVM.DeInit();
            }
        }
    }
}
