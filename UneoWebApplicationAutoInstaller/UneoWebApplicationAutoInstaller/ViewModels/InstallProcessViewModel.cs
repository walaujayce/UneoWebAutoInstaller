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
            InstallSelection = new ObservableCollection<Install>()
            {
                new Install() 
                {
                    InstallID = (int)EInstallID.PostgreSQL,
                    InstallName = "PostgreSQL Database", 
                    IsChecked = true, 
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Image Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Container Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 1,
                            SettingName = "Ports",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 2,
                            SettingName = "Volumes",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "Key0",
                                    DictionaryValue = "Value0",
                                }
                            }
                        },
                        new Setting()
                        {
                            SettingType = 2,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "Key00",
                                    DictionaryValue = "Value00",
                                }
                            }
                        },
                    }

                },
                new Install() 
                { 
                    InstallID = (int)EInstallID.WebAPI,
                    InstallName="WebAPI", 
                    IsChecked = true,
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Image Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Container Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 1,
                            SettingName = "Ports",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 2,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "Key1",
                                    DictionaryValue = "Value1",
                                },
                            }
                        },
                    }
                },
                new Install() 
                {
                    InstallID = (int)EInstallID.UMonitorSocketServer,
                    InstallName="UMonitorSocketServer",
                    IsChecked = false, 
                },
                new Install() 
                { 
                    InstallID = (int)EInstallID.Website,    
                    InstallName="Website", 
                    IsChecked = false,
                    SettingList = new()
                    {
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Image Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 0,
                            SettingName = "Container Name",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 1,
                            SettingName = "Ports",
                            SettingValue = "Value",
                        },
                        new Setting()
                        {
                            SettingType = 2,
                            SettingName = "Environment Variables",
                            KeyValueItems = new ObservableCollection<DictionaryInput>()
                            {
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_SOCKETSERVER_URL",
                                    DictionaryValue = "Value2",
                                },                                
                                new DictionaryInput()
                                {
                                    DictionaryKey = "VITE_WEBAPI_URL",
                                    DictionaryValue = "Value2",
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
            delegateOverlayShow?.Invoke(true);
            _installSelected = new();

            foreach (var item in InstallSelection)
            {
                if (item.IsChecked == true)
                {
                    
                    _installSelected.Add(CloneInstall(item));
                    Debug.WriteLine(item.InstallName);
                }
            }
            Debug.WriteLine("");
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
                        KeyValueItems = new ObservableCollection<DictionaryInput>(
                            s.KeyValueItems.Select(kv => new DictionaryInput
                            {
                                DictionaryKey = kv.DictionaryKey,
                                DictionaryValue = kv.DictionaryValue
                            }))
                    }))
            };
        }

    }
}
