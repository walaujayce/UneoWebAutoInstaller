using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.InstallProcessViewModel;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using static UneoWebApplicationAutoInstaller.ViewModels.ProgressMonitorViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class InstallProcessViewModel : ViewModelBase
    {
        private ObservableCollection<Install> _installSelection = new();
        public ObservableCollection<Install> InstallSelection
        {
            get => _installSelection;
            set
            {
                _installSelection = value;
                OnPropertyChanged("InstallSelection");
            }
        }

        private List<Install> _installSelected;
        private ObservableCollection<DictionaryInput> UMonitorSocketServerAppSettings = new();
        private string ipAddress_WIFI = "";
        private string ipAddress_Ethernet = "";

        public InstallProcessViewModel()
        {
            InitializeIPAddress();
            InitializeUMonitorSocketServerAppsettings();

            InstallSelection = new ObservableCollection<Install>()
            {
                new Install() 
                {
                    InstallID = (int)EInstallID.PostgreSQLDatabase,
                    InstallName = "PostgreSQL Database", 
                    IsChecked = true, 
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Image Name",
                            SettingValue = "bitnami/postgresql:latest",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Container Name",
                            SettingValue = "UNEO_DATABASE",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleInput,
                            SettingName = "Ports",
                            InputList = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryValue = "5432:5432"
                                },
                            }                        
                        },
                        //new Setting()
                        //{
                        //    SettingType = (int)ESettingType.MultipleKeyValue,
                        //    SettingName = "Volumes",
                        //    KeyValueItems = new ObservableCollection<DictionaryInput>()
                        //    {
                        //        new DictionaryInput()
                        //        {
                        //            DictionaryKey = "C:\\Users\\uneo\\postgres_data",
                        //            DictionaryValue = "/bitnami/postgresql",
                        //        }
                        //    }
                        //},
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValue,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "POSTGRESQL_PASSWORD",
                                    DictionaryValue = "uccc07568009",
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "PGPASSWORD",
                                    DictionaryValue = "uccc07568009",
                                }
                            }
                        },
                    }
                },
                new Install() 
                { 
                    InstallID = (int)EInstallID.WebAPI,
                    InstallName = "WebAPI", 
                    IsChecked = true,
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Image Name",
                            SettingValue = "uneotw/umonitorwebapi:latest",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Container Name",
                            SettingValue = "UNEO_WEBAPI",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleInput,
                            SettingName = "Ports",
                            InputList = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryValue = "7286:8032",
                                },                                
                                new DictionaryInput()
                                {
                                    DictionaryValue = "7284:8080"
                                },
                            }
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValue,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "DATABASE_IP",
                                    DictionaryValue = ipAddress_Ethernet,
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "FLOOR",
                                    DictionaryValue = "1F",
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "SECTION",
                                    DictionaryValue = "A",
                                }
                            }
                        },
                    }
                },
                new Install() 
                {
                    InstallID = (int)EInstallID.UMonitorSocketServer,
                    InstallName = "UMonitorSocketServer",
                    IsChecked = true, 
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValue,
                            SettingName = "App Settings",
                            KeyValueItems = UMonitorSocketServerAppSettings
                        } 
                    }
                },
                new Install() 
                { 
                    InstallID = (int)EInstallID.Website,    
                    InstallName = "Website", 
                    IsChecked = true,
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Image Name",
                            SettingValue = "p2211/uext-v1:latest",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Container Name",
                            SettingValue = "UNEO_WEBSITE",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleInput,
                            SettingName = "Ports",
                            SettingValue = "Value",
                            InputList = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryValue = "8005:5173"
                                }
                            }
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValueWithReference,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_SOCKETSERVER_URL",
                                    DictionaryValue = ipAddress_WIFI,
                                },                                
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_WEBAPI_URL",
                                    DictionaryValue = ipAddress_Ethernet,
                                },
                            }
                        },
                    }
                },
                new Install()
                {
                    InstallID = (int)EInstallID.UMonitorService,
                    InstallName = "UMonitorService",
                    IsChecked = true,
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Image Name",
                            SettingValue = "p2211/umonitorservices:latest",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Container Name",
                            SettingValue = "UNEO_SERVICES",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValueWithReference,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "USERNAME",
                                    DictionaryValue = "TEST",
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "SOCKETSERVER_IP",
                                    DictionaryValue = ipAddress_WIFI,
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "WEBAPI_IP",
                                    DictionaryValue = ipAddress_Ethernet,
                                },                                                                
                                new DictionaryInput()
                                {
                                    DictionaryKey = "EMAIL",
                                    DictionaryValue = "carlos.chu@uneotech.com,ted.fang@uneotech.com",
                                },                                                                                                
                                new DictionaryInput()
                                {
                                    DictionaryKey = "EMAIL_CC",
                                    DictionaryValue = "york_huang@uneotech.com,jayce@uneotech.com",
                                },
                            }
                        },
                    }
                },
                new Install()
                {
                    InstallID = (int)EInstallID.Notification,
                    InstallName = "Notification",
                    IsChecked = true,
                    SettingList = new()
                    {                        
                        new Setting()
                        {
                            SettingType = (int)ESettingType.Checkbox,
                            SettingName = "Email Notification",
                            CheckboxList = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "Windows Update",
                                    IsChecked = true,
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "Windows Restart",
                                    IsChecked = true,
                                },
                            }
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Server Name",
                            SettingValue = "TEST",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "WEBAPI IP",
                            SettingValue = ipAddress_WIFI,
                        },  
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Email",
                            SettingValue = "carlos.chu@uneotech.com,ted.fang@uneotech.com",
                        },
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Email Cc",
                            SettingValue = "york_huang@uneotech.com,jayce@uneotech.com",
                        },                        
                        new Setting()
                        {
                            SettingType = (int)ESettingType.SingleInput,
                            SettingName = "Event ID",
                            SettingValue = "13,19,1074",
                        },                        
                    }
                },
            };
        }

        private DelegateNavigate? delegateNavigate = null;
        public void SetDelegateNavigate(DelegateNavigate del)
        {
            this.delegateNavigate = del;
        }
        //private DelegateOverlayShow? delegateOverlayShow = null;
        //public void SetVMDelegateOverlayShow(DelegateOverlayShow del)
        //{
        //    this.delegateOverlayShow = del;
        //}
        private DelegateSelectedInstallation? delegateSelectedInstallation = null;
        public void SetVMDelegateSelectedInstallation(DelegateSelectedInstallation del)
        {
            this.delegateSelectedInstallation = del;
        }

        public void ProceedToInstallation()
        {
            _installSelected = new();

            foreach (var item in InstallSelection)
            {
                if (item.IsChecked == true)
                {
                    
                    _installSelected.Add(CloneInstall(item));
                    Debug.WriteLine(item.InstallName);
                }
            }
            if(_installSelected.Count < 1)
            {
                MessageBox.Show("At least one application should be selected.", "Alert", MessageBoxButton.OK);
                return;
            }
            //delegateOverlayShow?.Invoke(true);
            delegateSelectedInstallation?.Invoke(_installSelected);
        }
        private Install CloneInstall(Install original)
        {
            return new Install
            {
                InstallID = original.InstallID,
                InstallName = original.InstallName,
                InstallDescription = original.InstallDescription,
                IsChecked = original.IsChecked,
                SettingList = new ObservableCollection<Setting>(
                    original.SettingList.Select(s => new Setting
                    {
                        SettingType = s.SettingType,
                        SettingName = s.SettingName,
                        SettingValue = s.SettingValue,
                        InputList = new ObservableCollection<DictionaryInput>(
                            s.InputList.Select(i => new DictionaryInput
                            {
                                DictionaryKey = i.DictionaryKey,
                                DictionaryValue = i.DictionaryValue,
                                IsRemovable = i.IsRemovable,
                                AddBtnImageSource = i.AddBtnImageSource,
                            })),
                        KeyValueItems = new ObservableCollection<DictionaryInput>(
                            s.KeyValueItems.Select(kv => new DictionaryInput
                            {
                                DictionaryKey = kv.DictionaryKey,
                                DictionaryValue = kv.DictionaryValue,
                                IsRemovable = kv.IsRemovable,
                                AddBtnImageSource = kv.AddBtnImageSource,
                            })),
                        CheckboxList = new ObservableCollection<DictionaryInput>(
                            s.CheckboxList.Select(kv => new DictionaryInput
                            {
                                DictionaryKey = kv.DictionaryKey,
                                DictionaryValue = kv.DictionaryValue,
                                IsChecked = kv.IsChecked,
                                CheckboxImage = kv.CheckboxImage,
                            })),
                    }))
            };
        }
        /// <summary>
        /// Initialize UMonitorSocketServer Appsettings JSON file
        /// </summary>
        public void InitializeUMonitorSocketServerAppsettings()
        {
            Debug.WriteLine("Load JSON file in debug folder");
            PublicFunction.USocketServer_AppSettings_JSON_FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UMonitorSocketServer", "publish", "appsettings.json");
            UMonitorSocketServerAppSettings = PublicFunction.LoadJsonFile(PublicFunction.USocketServer_AppSettings_JSON_FilePath);
        }
        /// <summary>
        /// Init IP Address of both wifi and thernet
        /// </summary>
        public void InitializeIPAddress()
        {
            ipAddress_WIFI = PublicFunction.GetWireless80211IPAddress();
            ipAddress_Ethernet = PublicFunction.GetEthernetIPAddress();
            if (ipAddress_WIFI == null || ipAddress_WIFI == "")
            {
                ipAddress_WIFI = ipAddress_Ethernet;
            }
            if (ipAddress_Ethernet == null || ipAddress_Ethernet == "")
            {
                ipAddress_Ethernet = ipAddress_WIFI;
            }
        }
    }
}
