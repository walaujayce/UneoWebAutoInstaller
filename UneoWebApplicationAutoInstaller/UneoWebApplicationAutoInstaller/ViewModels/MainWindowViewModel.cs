using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;
using UneoWebApplicationAutoInstaller.Command;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.Views;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using ProcessOrigin = System.Diagnostics.Process;

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
        
        public delegate void DelegateInstallationData(Dictionary<int, List<Setting>> installationData);
        public DelegateInstallationData delegateInstallationData;

        public delegate void DelegateSelectedInstallation(List<Install> selectedInstallation);
        public DelegateSelectedInstallation delegateSelectedInstallation;

        public delegate void DelegateProgressResult(Progress progressResult);

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
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isShown)
                {
                    OverlayVisibility = Visibility.Visible;

                }
                else
                {

                    OverlayVisibility = Visibility.Collapsed;
                }
            });
        }
        private async void InstallationDataListener(Dictionary<int, List<Setting>> installationData)
        {
            //Get items of installation and enumerate all To-do-list in progress monitor page
            int index = 0;
            List<Progress> progressList = new List<Progress>();
            foreach (var item in installationData)
            {
                Debug.WriteLine($"Key : {item.Key} | Value : {item.Value}");
                List<string> toDoLsit = PublicFunction.EnumerateInstallToDoList(item.Key);
                foreach (var toDo in toDoLsit)
                {
                    progressList.Add(new Progress()
                    {
                        ProgressID = index,
                        ProgressName = $"{toDo}",
                    });
                    index++;
                }
            }

            _progressMonitorPage.GetSelectedInstallationTodoList(progressList);
            
            NavigateToSelectedPage((int)ENavigatePage.ProgressMonitorPage);

            //Check Docker Is Running And Auto Restart Enabled
            if (Config.EnableDockerCheck)
            {
                if (!await CheckDockerIsRunningAndAutoRestartEnabled()) return;
            }

            //Dataparser to new model for installation process
            if (installationData.Count > 0)
            {
                CmdDataParser cmdDataParser = new CmdDataParser();
                cmdDataParser.DataParser(installationData);
                cmdDataParser.SetDelegateProgressResult(new DelegateProgressResult(ProgressResultListener));
            }
        }

        private void ProgressResultListener(Progress progressResult)
        {
            _progressMonitorPage.SendInstallationResponseToProgressMonitorPage(progressResult);
        }
        public async Task<bool> CheckDockerIsRunningAndAutoRestartEnabled()
        {
            ///Check Docker Is Running
            Debug.WriteLine("STEP 1");
            bool isDockerRunning = false;
            do
            {
                isDockerRunning = ProcessOrigin.GetProcessesByName("com.docker.backend").Any();
                if (!isDockerRunning)
                {
                    MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "Please run Docker before proceed to next step.\n\nPress \"No\" to cancel installation.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if(result == MessageBoxResult.No)
                    {
                        NavigateToSelectedPage((int)ENavigatePage.ProcessSelectionPage);
                        return isDockerRunning;
                    }
                    await Task.Delay(10);
                }
                else
                {
                    Debug.WriteLine("Docker Desktop is running.");
                }

            }while (!isDockerRunning);

            //Check Docker Auto Restart Enabled
            Debug.WriteLine("STEP 2");

            if (!isDockerRunning) return isDockerRunning; // Docker is not running, go back

            bool autoRestart = false;
            do
            {
                string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Docker", "settings-store.json");

                if (File.Exists(settingsFilePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(settingsFilePath);
                    JObject settings = JObject.Parse(jsonContent);

                    autoRestart = settings["AutoStart"]?.ToObject<bool>() ?? false;

                    if (!autoRestart)
                    {
                        MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "1) Go to Docker Desktop -> Settings -> General\n2) Enable \"Start Docker Desktop when you sign in to your computer\".\n\nPress \"Ok\" after Docker Desktop's setting has changed or press \"Cancel\" to ignore.", "Run Docker Desktop automatically when you sign in to your computer", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if(result == MessageBoxResult.Cancel)
                        {
                            Debug.WriteLine("Docker Desktop is NOT set to start on login.");
                            break;
                        }
                        await Task.Delay(10);
                    }
                    else
                    {
                        Debug.WriteLine("Docker Desktop is set to start on login.");
                    }
                }
            } while (!autoRestart);

            return isDockerRunning; // Docker is not running, go back

        }

    }
}
