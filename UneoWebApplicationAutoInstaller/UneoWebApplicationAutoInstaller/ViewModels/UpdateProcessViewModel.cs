using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class UpdateProcessViewModel : ViewModelBase
    {
        private ObservableCollection<Update> _updateSelection = new();
        public ObservableCollection<Update> UpdateSelection
        {
            get => _updateSelection;
            set
            {
                _updateSelection = value;
                OnPropertyChanged("UpdateSelection");
            }
        }
        private Update _updateSelected = new();
        public Update UpdateSelected
        {
            get => _updateSelected;
            set
            {
                _updateSelected = value;
                OnPropertyChanged("UpdateSelected");
            }
        }

        private ObservableCollection<DictionaryInput> UMonitorSocketServerAppSettings = new();
        private string ipAddress_WIFI = "";
        private string ipAddress_Ethernet = "";

        public UpdateProcessViewModel()
        {
            InitializeIPAddress();
            InitializeUMonitorSocketServerAppsettings();

            UpdateSelection = new ObservableCollection<Update>()
            {
                new Update()
                {
                    UpdateID = (int)EUpdateID.Website_CONTAINER,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.Website_CONTAINER),
                    UpdateDescription = "IP address changes, then use this to restore website settings.",
                    IsProceedToSettingModal = true,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.Website_IMAGE,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.Website_IMAGE),
                    UpdateDescription = "Update image version.",
                    IsProceedToSettingModal= true,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.UMonitorSocketServer_APPSETTINGS,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.UMonitorSocketServer_APPSETTINGS),
                    UpdateDescription = "Modify appsettings.json file.",
                    IsProceedToSettingModal = true,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.UMonitorSocketServer_ALL,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.UMonitorSocketServer_ALL),
                    UpdateDescription = "Update UMonitorSocketServer version.",
                    IsProceedToSettingModal = false,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.WebAPI_CONTAINER,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.WebAPI_CONTAINER),
                    UpdateDescription = "Recontainerize WebAPI container.",
                    IsProceedToSettingModal= true,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.WebAPI_IMAGE,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.WebAPI_IMAGE),
                    UpdateDescription = "Update image version.",
                    IsProceedToSettingModal= true,
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
                new Update()
                {
                    UpdateID = (int)EUpdateID.UMonitorService_CONTAINER,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.UMonitorService_CONTAINER),
                    UpdateDescription = "Edit environment variables.",
                    IsProceedToSettingModal = true,
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
                                    DictionaryKey = "SOCKETSERVER_IP",
                                    DictionaryValue = ipAddress_WIFI,
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "WEBAPI_URL",
                                    DictionaryValue = ipAddress_Ethernet,
                                },
                            }
                        },
                    }
                },                
                new Update()
                {
                    UpdateID = (int)EUpdateID.UMonitorService_IMAGE,
                    UpdateName = PublicFunction.GetUpdateName((int)EUpdateID.UMonitorService_IMAGE),
                    UpdateDescription = "Update image version.",
                    IsProceedToSettingModal = true,
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
                                    DictionaryKey = "SOCKETSERVER_IP",
                                    DictionaryValue = ipAddress_WIFI,
                                },
                                new DictionaryInput()
                                {
                                    DictionaryKey = "WEBAPI_URL",
                                    DictionaryValue = ipAddress_Ethernet,
                                },
                            }
                        },
                    }
                },         
            };
        }
        private DelegateSelectedUpdate? delegateSelectedUpdate = null;
        public void SetVMDelegateSelectedUpdate(DelegateSelectedUpdate del)
        {
            this.delegateSelectedUpdate = del;
        }
        public void ProccedToUpdate()
        {
            delegateSelectedUpdate?.Invoke(CloneUpdate(UpdateSelected));
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
        private Update CloneUpdate(Update original)
        {
            return new Update
            {
                UpdateID = original.UpdateID,
                UpdateName = original.UpdateName,
                UpdateDescription = original.UpdateDescription,
                IsProceedToSettingModal = original.IsProceedToSettingModal,
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
