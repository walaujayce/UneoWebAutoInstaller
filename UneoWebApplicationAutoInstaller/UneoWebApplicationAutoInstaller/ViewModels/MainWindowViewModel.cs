using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.Views;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _version; 
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        private Visibility _overlayVisibility = Visibility.Collapsed;
        public Visibility OverlayVisibility
        {
            get { return _overlayVisibility; }
            set
            {
                _overlayVisibility = value;
                OnPropertyChanged(nameof(OverlayVisibility));
            }
        }

        public string MainWindowTitle
        {
            get { return $"Uneo Web Application Auto Installer ({Version})"; }
        }

        private readonly INavigationService _navigationService;

        public delegate void DelegateNavigate(int pageNumber);
        public DelegateNavigate delegateNavigate;

        public delegate void DelegateSettingModalShow(bool isShown);
        public DelegateSettingModalShow delegateSettingModalShow;

        public delegate void DelegateOverlayShow(bool isShown);
        public DelegateOverlayShow delegateOverlayShow; 
        
        public delegate void DelegateInstallationData(Dictionary<string, ObservableCollection<Setting>> installationData);
        public DelegateInstallationData delegateInstallationData;

        public delegate void DelegateSelectedInstallation(List<Install> selectedInstallation);
        public DelegateSelectedInstallation delegateSelectedInstallation;

        private ProcessSelection _processSelectionPage = new();
        private InstallProcess _installProcessPage = new();
        private UpdateProcess _updateProcessPage = new();
        private UninstallProcess _uninstallProcessPage = new();
        private ProgressMonitor _progressMonitorPage = new();

        public MainWindowViewModel(INavigationService navigationService)
        {
            Version = "V1.1.0";

            _navigationService = navigationService;
            NavigateToSelectedPage((int)ENavigatePage.ProcessSelectionPage);
            _processSelectionPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));
            _installProcessPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));
            _installProcessPage.SetDelegateOverlayShow(new DelegateOverlayShow(OverlayShowListener));
            _installProcessPage.SetDelegateSelectedInstallation(new DelegateSelectedInstallation(SettingModalListener));

            _progressMonitorPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));

            PublicFunction.GetAllInterfaceName();
        }
        private void NavigateToSelectedPage(int pageNumber)
        {
            switch (pageNumber)
            {   
                case (int)ENavigatePage.ProcessSelectionPage:
                    _navigationService.NavigateTo(_processSelectionPage);
                    break;
                case (int)ENavigatePage.InstallProcessPage:
                    _navigationService.NavigateTo(_installProcessPage);
                    break;
                case (int)ENavigatePage.UpdateProcessPage:
                    _navigationService.NavigateTo(_updateProcessPage);
                    break;
                case (int)ENavigatePage.UninstallPage:
                    _navigationService.NavigateTo(_uninstallProcessPage);
                    break;
                case (int)ENavigatePage.ProgressMonitorPage:
                    _navigationService.NavigateTo(_progressMonitorPage);
                    break;
                default:
                    MessageBox.Show("No such page!", "Alert", MessageBoxButton.OK);
                    return;
            }
        }
        private void SettingModalListener(List<Install> selectedInstallation)
        {
            if (selectedInstallation.Count > 0)
            {
                SettingModal settingModal = new SettingModal()
                {
                    Owner = Application.Current.MainWindow,
                };

                settingModal.SetDelegateOverlayShow(new DelegateOverlayShow(OverlayShowListener));
                settingModal.SetDelegateInstallationData(new DelegateInstallationData(InstallationDataListener));
                settingModal.SetSelectedInstallation(selectedInstallation);

                settingModal.ShowDialog();
            }
        }
        private void OverlayShowListener(bool isShown)
        {
            if (isShown)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OverlayVisibility = Visibility.Visible;
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OverlayVisibility = Visibility.Collapsed;
                });
            }
        }
        private void InstallationDataListener(Dictionary<string, ObservableCollection<Setting>> installationData)
        {
            List<string> selectedInstallationList = new List<string>();
            foreach(var item in installationData)
            {
                selectedInstallationList.Add(item.Key);
                Debug.WriteLine($"Key : {item.Key} | Value : {item.Value}");
            }
            _progressMonitorPage.GetSelectedInstallation(selectedInstallationList);
            NavigateToSelectedPage((int)ENavigatePage.ProgressMonitorPage);
            //Dataparser to new model for installation process

        }

        private void InstallationStatusListener(Dictionary<string, bool> installationResponse)
        {
            _progressMonitorPage.SendInstallationResponseToProgressMonitorPage(installationResponse);
        }

    }
}
