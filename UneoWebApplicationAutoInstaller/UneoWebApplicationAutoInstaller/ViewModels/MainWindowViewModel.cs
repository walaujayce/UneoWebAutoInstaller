using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json.Linq;
using UneoWebApplicationAutoInstaller.Command;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.Views;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using Color = System.Windows.Media.Color;
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
            get { return $"Uneo Web Application Auto Installer"; }
        }
        public string Copyright
        {
            get { return $"Copyright ©{DateTime.Now.Year.ToString()} Uneo Inc. All rights reserved."; }
        }
        private string _versionImage = "/Views/Assets/Icon_LatestVersion_FF00FF7F.png";
        public string VersionImage
        {
            get { return _versionImage; }
            set
            {
                _versionImage = value;
                OnPropertyChanged(nameof(VersionImage));
            }
        }
        private string _versionTextBlock = "Latest version";
        public string VersionTextBlock
        {
            get { return _versionTextBlock; }
            set
            {
                _versionTextBlock = value;
                OnPropertyChanged(nameof(VersionTextBlock));
            }
        }
        private SolidColorBrush _versionTextBlockForeground = new SolidColorBrush(Color.FromRgb(154, 205, 50));
        public SolidColorBrush VersionTextBlockForeground
        {
            get { return _versionTextBlockForeground; }
            set
            {
                _versionTextBlockForeground = value;
                OnPropertyChanged(nameof(VersionTextBlockForeground));
            }
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

        public delegate void DelegateSelectedUpdate(Update selectedUpdate);
        public DelegateSelectedInstallation delegateSelectedUpdate;

        public delegate void DelegateProgressResult(ProgressDetail progressResult);

        private ProcessSelection _processSelectionPage = new();
        private InstallProcess _installProcessPage = new();
        private UpdateProcess _updateProcessPage = new();
        private DiagnosticProcess _diagnosticProcessPage = new();
        private ProgressMonitor _progressMonitorPage = new();

        private Version currentVersion, latestVersion;
        private string newInstallerUrl;
        public MainWindowViewModel(INavigationService navigationService)
        {
            Version = Config.Version;

            _navigationService = navigationService;
            NavigateToSelectedPage((int)ENavigatePage.ProcessSelectionPage);

            _processSelectionPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));

            _installProcessPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));
            _installProcessPage.SetDelegateSelectedInstallation(new DelegateSelectedInstallation(InstallSettingModalListener));

            _progressMonitorPage.SetDelegate(new DelegateNavigate(NavigateToSelectedPage));

            _updateProcessPage.SetDelegateSelectedUpdate(new DelegateSelectedUpdate(UpdateSettingModalListener));

            _=CheckApplicationVersion();


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
                case (int)ENavigatePage.DiagnosticPage:
                    _navigationService.NavigateTo(_diagnosticProcessPage);
                    break;
                case (int)ENavigatePage.ProgressMonitorPage:
                    _navigationService.NavigateTo(_progressMonitorPage);
                    break;
                default:
                    MessageBox.Show("No such page!", "Alert", MessageBoxButton.OK);
                    return;
            }
        }
        private void InstallSettingModalListener(List<Install> selectedInstallation)
        {
            Debug.WriteLine("InstallSettingModalListener");
            if (selectedInstallation.Count > 0)
            {
                OverlayShowListener(true);
                SettingModal settingModal = new SettingModal()
                {
                    Owner = Application.Current.MainWindow,
                };
                settingModal.SetProcessMode(0);
                settingModal.SetDelegateOverlayShow(new DelegateOverlayShow(OverlayShowListener));
                settingModal.SetDelegateInstallationData(new DelegateInstallationData(InstallationDataListener));
                settingModal.SetSelectedInstallation(selectedInstallation);
                settingModal.ShowDialog();
            }
        }
        private void UpdateSettingModalListener(Update selectedUpdate)
        {
            Debug.WriteLine("UpdateSettingModalListener");
            if (selectedUpdate.IsProceedToSettingModal)
            {
                OverlayShowListener(true);
                SettingModal settingModal = new SettingModal()
                {
                    Owner = Application.Current.MainWindow,
                };
                settingModal.SetProcessMode(1);
                settingModal.SetDelegateOverlayShow(new DelegateOverlayShow(OverlayShowListener));
                settingModal.SetSelectedUpdate(selectedUpdate);
                settingModal.SetDelegateInstallationData(new DelegateInstallationData(UpdateDataListener));
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
            List<Progress> progressList = new List<Progress>();
            
            foreach (var item in installationData)
            {
                Debug.WriteLine($"Key : {item.Key} | Value : {item.Value}");

                progressList.Add(new Progress()
                {
                    ProgressID = item.Key,
                    ProgressName = PublicFunction.GetInstallationName(item.Key),
                });

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
        private async void UpdateDataListener(Dictionary<int, List<Setting>> installationData)
        {
            //Get items of installation and enumerate all To-do-list in progress monitor page
            List<Progress> progressList = new List<Progress>();

            foreach (var item in installationData)
            {
                Debug.WriteLine($"Key : {item.Key} | Value : {item.Value}");

                progressList.Add(new Progress()
                {
                    ProgressID = item.Key,
                    ProgressName = PublicFunction.GetUpdateName(item.Key),
                });

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
                cmdDataParser.DataParserUpdateProcess(installationData);
                cmdDataParser.SetDelegateProgressResult(new DelegateProgressResult(ProgressResultListener));
            }
        }
        private void ProgressResultListener(ProgressDetail progressResult)
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
        private async Task CheckApplicationVersion()
        {
            string url = "http://files.uneotech.com:3168/share.cgi?ssid=dcc312867be64128953be7637947f7c1&openfolder=forcedownload&ep=&_dc=1750763418805&fid=dcc312867be64128953be7637947f7c1&filename=UneoWebApplicationVersion.json";
            using HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();                

                var result = JsonSerializer.Deserialize<ApplicationVersion>(content);
                if (result == null) return;
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                latestVersion =  System.Version.Parse(result.version);
                newInstallerUrl = result.url;

                if (latestVersion > currentVersion)
                {
                    VersionImage = "/Views/Assets/Icon_Info_B0470F.png";
                    VersionTextBlock = "New version available";
                    VersionTextBlockForeground = new SolidColorBrush(Color.FromRgb(176, 71, 15));                    
                }
                else
                {
                    VersionImage = "/Views/Assets/Icon_LatestVersion_FF00FF7F.png";
                    VersionTextBlock = "Latest version";
                    VersionTextBlockForeground = new SolidColorBrush(Color.FromRgb(154, 205, 50));
                }

            }
            catch (Exception ex)
            {
                VersionImage = "/Views/Assets/Icon_Info_FFFFD700.png";
                VersionTextBlock = "No internet connection";
                VersionTextBlockForeground = new SolidColorBrush(Color.FromRgb(255, 215 , 0));
            }
        }
        public async void CheckVersion()
        {
            await CheckApplicationVersion();

            if (latestVersion > currentVersion)
            {
                var result = MessageBox.Show(
                    $"New version {latestVersion} available.\nDo you want to download and update now?",
                    "Update Available", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    await DownloadAndInstallAsync(newInstallerUrl);
                }
            }
        }
        private async Task DownloadAndInstallAsync(string downloadUrl)
        {
            if(downloadUrl == null || downloadUrl == "") return; 
            string tempFile = Path.Combine(Path.GetTempPath(), $"UneoWebApplicationSetupInstaller_{DateTime.Now:yyyyMMddHHmmss}.msi");

            List<Progress> updateApplicationProgress = [new Progress() {
                ProgressID = 0,
                ProgressName = "Upgrade application to latest version"
            }];

            _progressMonitorPage.GetSelectedInstallationTodoList(updateApplicationProgress);

            NavigateToSelectedPage((int)ENavigatePage.ProgressMonitorPage);

            ProgressDetail progressDetail_UpdateAppVersion = new ProgressDetail();
            progressDetail_UpdateAppVersion.ProgressParentID = 0;
            progressDetail_UpdateAppVersion.ProgressDescription = "Downloading latest msi installer";
            progressDetail_UpdateAppVersion.StatusStatePD = (int)EProgressStatus.Ongoing;
            ProgressResultListener(progressDetail_UpdateAppVersion);

            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(15)
            };
            var data = await client.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(tempFile, data);

            if (File.Exists(tempFile))
            {
                progressDetail_UpdateAppVersion.ProgressDescription = "Downloading latest msi installer";
                progressDetail_UpdateAppVersion.StatusStatePD = (int)EProgressStatus.Pass;
            }
            else
            {
                progressDetail_UpdateAppVersion.ProgressDescription = "Downloading latest msi installer";
                progressDetail_UpdateAppVersion.StatusStatePD = (int)EProgressStatus.Fail;                
                return;
            }

            progressDetail_UpdateAppVersion.IsFinish = true;
            ProgressResultListener(progressDetail_UpdateAppVersion);

            var result = MessageBox.Show(
                    $"This application will be closed.",
                    "Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.OK)
            {
                ProcessOrigin.Start(new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{tempFile}\"",
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }

            Application.Current.Shutdown(); 
        }
    }
}
