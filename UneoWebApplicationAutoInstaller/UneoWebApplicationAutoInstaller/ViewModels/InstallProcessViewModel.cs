using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public InstallProcessViewModel()
        {
            ObservableCollection<DictionaryInput> UMonitorSocketServerAppSettings = Utilities.PublicFunction.LoadJsonFile();

            InstallSelection = new ObservableCollection<Install>()
            {
                new Install() 
                {
                    InstallID = (int)EInstallID.PostgreSQLDatabase,
                    InstallName = "PostgreSQL Database", 
                    IsChecked = false, 
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
                        new Setting()
                        {
                            SettingType = (int)ESettingType.MultipleKeyValue,
                            SettingName = "Volumes",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "C:\\Users\\uneo\\postgres_data",
                                    DictionaryValue = "/bitnami/postgresql",
                                }
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
                                    DictionaryKey = "POSTGRESQL_PASSWORD",
                                    DictionaryValue = "uccc07568009",
                                }
                            }
                        },
                    }

                },
                new Install() 
                { 
                    InstallID = (int)EInstallID.WebAPI,
                    InstallName="WebAPI", 
                    IsChecked = false,
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
                        }
                        
                    }
                },
                new Install() 
                {
                    InstallID = (int)EInstallID.UMonitorSocketServer,
                    InstallName="UMonitorSocketServer",
                    IsChecked = false, 
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
                    InstallName="Website", 
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
                                    DictionaryValue = $"{PublicFunction.GetAllInterfaceName()?.FirstOrDefault().Value}",
                                },                                
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_WEBAPI_URL",
                                    DictionaryValue = $"{PublicFunction.GetAllInterfaceName()?.FirstOrDefault().Value}",
                                },
                            }
                        },
                    }
                },
                new Install()
                {
                    InstallID = (int)EInstallID.UMonitorService,
                    InstallName="UMonitorService",
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
                                    DictionaryValue = "NTU",
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "LOCAL_IP",
                                    DictionaryValue = $"{PublicFunction.GetAllInterfaceName()?.FirstOrDefault().Value}",
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_WEBAPI_URL",
                                    DictionaryValue = $"{PublicFunction.GetAllInterfaceName()?.FirstOrDefault().Value}",
                                },
                            }
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
        private DelegateOverlayShow? delegateOverlayShow = null;
        public void SetVMDelegateOverlayShow(DelegateOverlayShow del)
        {
            this.delegateOverlayShow = del;
        }
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
            delegateOverlayShow?.Invoke(true);
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
                            }))
                    }))
            };
        }

    }
}
