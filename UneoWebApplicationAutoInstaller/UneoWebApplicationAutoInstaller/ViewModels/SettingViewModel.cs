using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using UneoWebApplicationAutoInstaller.Models;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using System.Diagnostics;
using System.Windows.Media;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    //Region DataTemplate Selector
    #region SETTING TEMPLATE SELECTOR
    public class SettingTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate SingleInputTemplate { get; set; }
        public required DataTemplate MultipleInputTemplate { get; set; }
        public required DataTemplate MultipleKeyValueTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Setting setting)
            {
                switch (setting.SettingType)
                {
                    case 0 :
                        return SingleInputTemplate;
                    case 1:
                        return MultipleInputTemplate;
                    case 2:
                        return MultipleKeyValueTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
    #endregion

    public class SettingViewModel : ViewModelBase
    {        
        private string _installationName = "";
        public string InstallationName
        {
            get
            {
                return _installationName;
            }
            set
            {
                _installationName = value;
                OnPropertyChanged(nameof(InstallationName));
            }
        }
        private ObservableCollection<Setting> _settingList;
        public ObservableCollection<Setting> SettingList
        {
            get
            {
                return _settingList;
            }
            set
            {
                _settingList = value;
                OnPropertyChanged(nameof(SettingList)); 
            }
        }

        public ObservableCollection<ProgressBarItem> ProgressItems { get; set; }

        public readonly SolidColorBrush greenProgressBar = new SolidColorBrush(Color.FromRgb(0x00, 0xE5, 0x00));

        public readonly SolidColorBrush grayProgressBar = new SolidColorBrush(Colors.LightGray);

        private SortedDictionary<int, ObservableCollection<Setting>> InstallationItems = new();

        private ObservableCollection<DictionaryInput> _keyValueItems;

        private int currentSettingListIndex = 0;

        private List<int> OrderedKeysInInstalltionSettings; 

        public SettingViewModel()
        {
            ProgressItems = new ObservableCollection<ProgressBarItem>();

            _keyValueItems = new ObservableCollection<DictionaryInput>()
            {
                new DictionaryInput()
                {
                    DictionaryKey = "DictionaryKey1",
                    DictionaryValue = "DictionaryValue1",
                },               
                new DictionaryInput()
                {
                    DictionaryKey = "DictionaryKey2",
                    DictionaryValue = "DictionaryValue2",
                },               

            };

            //SettingList = new ObservableCollection<Setting>()
            //{
            //    new Setting() { 
            //        SettingType = 0,
            //        SettingName = "Container Name",
            //        SettingValue = "Value",                    
            //    },                    
            //    new Setting() { 
            //        SettingType = 1,
            //        SettingName = "Ports",
            //        InputList = new ObservableCollection<DictionaryInput>(){
            //            new DictionaryInput()
            //            {
            //                DictionaryValue = "Value1",
            //            },
            //            new DictionaryInput()
            //            {
            //                DictionaryValue = "Value2",
            //            }
            //        }
            //    },                    
            //    new Setting() {
            //        SettingType = 2,
            //        SettingName = "Environment Variables",
            //        SettingKey = "Key0",
            //        SettingValue = "Value0",
            //        KeyValueItems = keyValueItems
            //    },
            //};

            SettingList = new ObservableCollection<Setting>();
        }

        #region DELEGATE METHODS
        private DelegateOverlayShow? delegateOverlayShow = null;
        public void SetVMDelegateOverlayShow(DelegateOverlayShow del)
        {
            this.delegateOverlayShow = del;
        }
        #endregion

        //Remove Main Window Overlay
        public void RemoveMainWindowOverlay()
        {
            delegateOverlayShow?.Invoke(false);
        }

        //Deinitiate Setting Modal
        public void DeInit()
        {
            InstallationItems.Clear();
            Debug.WriteLine("InstallationItems.count: " + InstallationItems.Count);
        }

        //Set the number of progress bar
        public void SelectedInstallationListener(List<Install> selectedInstallation)
        {
            if(selectedInstallation.Count > 0)
            {
                for (int i = 0; i < selectedInstallation.Count; i++)
                {
                    ProgressItems.Add( i < 1 ? new ProgressBarItem() { ProgressBarColor = greenProgressBar } : new ProgressBarItem() { ProgressBarColor = grayProgressBar });
                }

                foreach (var item in selectedInstallation)
                {
                    //InstallationItems[item.InstallID] = new ObservableCollection<Setting>()
                    //{
                    //    new Setting() {
                    //        SettingType = 0,
                    //        SettingName = item.InstallName,
                    //        SettingValue = "Value",
                    //    },
                    //    new Setting() {
                    //        SettingType = 1,
                    //        SettingName = "Ports",
                    //        InputList = new ObservableCollection<DictionaryInput>(){
                    //            new DictionaryInput()
                    //            {
                    //                DictionaryValue = "Value1",
                    //            },
                    //            new DictionaryInput()
                    //            {
                    //                DictionaryValue = "Value2",
                    //            }
                    //        }
                    //    },
                    //    new Setting() {
                    //        SettingType = 2,
                    //        SettingName = "Environment Variables",
                    //        KeyValueItems = _keyValueItems
                    //    },
                    //};
                    InstallationItems[item.InstallID] = item.SettingList;
                }
                SettingList = InstallationItems.First().Value;
                OrderedKeysInInstalltionSettings = InstallationItems.Keys.ToList();// already sorted because it's a SortedDictionary
            }

        }

        //Button of adding single input box to inputlist
        public DictionaryInput InputSelected { get; set; }
        public void AddInputItems(Setting setting)
        {
            Debug.WriteLine("count of InputList " + InputSelected.DictionaryValue);
            if (setting.InputList == null)
            {
                setting.InputList = new ObservableCollection<DictionaryInput>();
            }

            setting.InputList.Add(new DictionaryInput() { DictionaryValue = "Value"});
            Debug.WriteLine("count of InputList " + setting.InputList.Count);
        }

        //Button of adding key value input box to keyValueItemsList
        public DictionaryInput KeyValuePairSelected { get; set; }
        public void AddKeyValuePair()
        {
            Debug.WriteLine($"Key : {KeyValuePairSelected.DictionaryKey} | Value : {KeyValuePairSelected.DictionaryValue}");
            Debug.WriteLine($"IsRemovable : {KeyValuePairSelected.IsRemovable}");

            if(!KeyValuePairSelected.IsRemovable)
            {
                KeyValuePairSelected.AddBtnImageSource = "/Views/Assets/Icon_Remove_FFD3D3D3.png";
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == "Environment Variables");
                _keyValueItems = targetSetting.KeyValueItems;
                _keyValueItems.Add(new DictionaryInput());
                KeyValuePairSelected.IsRemovable = true;
            }
            else
            {
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == "Environment Variables");
                _keyValueItems = targetSetting.KeyValueItems;
                _keyValueItems.Remove(KeyValuePairSelected);
            }

            Debug.WriteLine($"keyValueItems.Count : {_keyValueItems.Count}");
        }

        public void GoToNextInstallationSettings()
        {
            if (currentSettingListIndex < InstallationItems.Count - 1)
            {
                currentSettingListIndex++;
                int nextKey = OrderedKeysInInstalltionSettings[currentSettingListIndex];
                Debug.WriteLine($"NextBtn, Currrent Key: {nextKey}");
                ProgressItems[nextKey] = new ProgressBarItem()
                {
                    ProgressBarColor = greenProgressBar,
                };
                SettingList = InstallationItems[nextKey];
            }
            else
            {
                Debug.WriteLine("Already at last item.");

                //foreach (var item in InstallationItems)
                //{
                //    Debug.WriteLine($"Key : {item.Key} | Value : {item.Value}");
                //    foreach(var sub_item in item.Value)
                //    {
                //        Debug.WriteLine($"sub_item value : {sub_item}");
                //    }
                //}
            }
        }
        public void BackToPreviousInstallationSettings()
        {
            if (currentSettingListIndex > 0)
            {
                currentSettingListIndex--;
                int previousKey = OrderedKeysInInstalltionSettings[currentSettingListIndex];
                Debug.WriteLine($"PreviousBtn, Currrent Key: {previousKey}");
                ProgressItems[previousKey+1] = new ProgressBarItem()
                {
                    ProgressBarColor = grayProgressBar,
                };
                SettingList = InstallationItems[previousKey];
            }
            else
            {
                Debug.WriteLine("Already at first item.");
            }
        }
    }
}
