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
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    //Region DataTemplate Selector
    #region SETTING TEMPLATE SELECTOR
    public class SettingTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate SingleInputTemplate { get; set; }
        public required DataTemplate MultipleInputTemplate { get; set; }
        public required DataTemplate MultipleKeyValueTemplate { get; set; }
        public required DataTemplate MultipleKeyValueWithReferenceTemplate { get; set; }
        

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
                    case 3:
                        return MultipleKeyValueWithReferenceTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
    #endregion

    public class SettingViewModel : ViewModelBase
    {
        private string _settingTitle = "";
        public string SettingTitle
        {
            get
            {
                return _settingTitle;
            }
            set
            {
                _settingTitle = value;
                OnPropertyChanged(nameof(SettingTitle));
            }
        }
        private string _backBtnLabel = "Back";
        public string BackBtnLabel
        {
            get
            {
                return _backBtnLabel;
            }
            set
            {
                _backBtnLabel = value;
                OnPropertyChanged(nameof(BackBtnLabel));
            }
        }
        private string _nextBtnLabel = "Next";
        public string NextBtnLabel
        {
            get
            {
                return _nextBtnLabel;
            }
            set
            {
                _nextBtnLabel = value;
                OnPropertyChanged(nameof(NextBtnLabel));
            }
        }
        
        private Brush _nextBtnLabelForeground = Brushes.White;
        public Brush NextBtnLabelForeground
        {
            get
            {
                return _nextBtnLabelForeground;
            }
            set
            {
                _nextBtnLabelForeground = value;
                OnPropertyChanged(nameof(NextBtnLabelForeground));
            }
        }
        
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

        private Dictionary<string, ObservableCollection<Setting>> InstallationItems = new();

        private ObservableCollection<DictionaryInput> _keyValueItems = new();

        private ObservableCollection<DictionaryInput> _inputList;

        private int currentSettingListIndex = 0;

        private bool isCloseSettingModalEnable = false;
        public Setting SettingSelected { get; set; }    
        public DictionaryInput InputSelected { get; set; }
        public DictionaryInput KeyValuePairSelected { get; set; }

        public SettingViewModel()
        {
            isCloseSettingModalEnable = false; 

            ProgressItems = new ObservableCollection<ProgressBarItem>();

            //_keyValueItems = new ObservableCollection<DictionaryInput>()
            //{
            //    new DictionaryInput()
            //    {
            //        DictionaryKey = "DictionaryKey1",
            //        DictionaryValue = "DictionaryValue1",
            //    },               
            //    new DictionaryInput()
            //    {
            //        DictionaryKey = "DictionaryKey2",
            //        DictionaryValue = "DictionaryValue2",
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
        private DelegateInstallationData? delegateInstallationData = null;
        public void SetVMDelegateInstallationData(DelegateInstallationData del)
        {
            this.delegateInstallationData = del;
        }
        
        #endregion

        //Deinitiate Setting Modal
        public void DeInit()
        {
            InstallationItems.Clear();
            delegateOverlayShow?.Invoke(false);
            Debug.WriteLine("InstallationItems.count: " + InstallationItems.Count);
        }
        //Close Setting Modal
        public bool CloseSettingModal()
        {
            return isCloseSettingModalEnable;
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
                //sort selection installations list order by Install ID
                List<Install> sortedSelectedInstallation = new List<Install>();
                sortedSelectedInstallation = selectedInstallation.OrderBy(s => s.InstallID).ToList();
                foreach (var item in sortedSelectedInstallation)
                {
                    InstallationItems[item.InstallName] = item.SettingList;
                }
                
                SettingTitle = InstallationItems.First().Key;
                SettingList = InstallationItems.First().Value;

                if (selectedInstallation.Count == 1)
                {
                    NextBtnLabel = "Run";
                    NextBtnLabelForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212121")); ;
                }
                //OrderedKeysInInstalltionSettings = InstallationItems.Keys.ToList();// already sorted because it's a SortedDictionary
            }
        }

        //Button of adding single input box to inputlist
        //public void AddInputItems(Setting setting)
        //{
        //    Debug.WriteLine("count of InputList " + InputSelected.DictionaryValue);
        //    if (setting.InputList == null)
        //    {
        //        setting.InputList = new ObservableCollection<DictionaryInput>();
        //    }

        //    setting.InputList.Add(new DictionaryInput() { DictionaryValue = "Value"});
        //    Debug.WriteLine("count of InputList " + setting.InputList.Count);
        //}
        public void AddInputItems()
        {
            if (!InputSelected.IsRemovable)
            {
                InputSelected.AddBtnImageSource = "/Views/Assets/Icon_Remove_FFD3D3D3.png";
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == SettingSelected.SettingName);
                _inputList = targetSetting.InputList;
                _inputList.Add(new DictionaryInput());
                InputSelected.IsRemovable = true;
            }
            else
            {
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == SettingSelected.SettingName);
                _inputList = targetSetting.InputList;
                _inputList.Remove(InputSelected);
            }

            Debug.WriteLine($"keyValueItems.Count : {_inputList.Count}");
        }

        //Button of adding key value input box to keyValueItemsList
        public void AddKeyValuePair()
        {
            Debug.WriteLine($"Key : {KeyValuePairSelected.DictionaryKey} | Value : {KeyValuePairSelected.DictionaryValue}");
            Debug.WriteLine($"IsRemovable : {KeyValuePairSelected.IsRemovable}");

            if(!KeyValuePairSelected.IsRemovable)
            {
                KeyValuePairSelected.AddBtnImageSource = "/Views/Assets/Icon_Remove_FFD3D3D3.png";
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == SettingSelected.SettingName);
                _keyValueItems = targetSetting.KeyValueItems;
                _keyValueItems.Add(new DictionaryInput());
                KeyValuePairSelected.IsRemovable = true;
            }
            else
            {
                var targetSetting = SettingList.FirstOrDefault(s => s.SettingName == SettingSelected.SettingName);
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
                //int nextKey = OrderedKeysInInstalltionSettings[currentSettingListIndex];
                int nextKey = currentSettingListIndex;
                Debug.WriteLine($"NextBtn, Currrent Key: {nextKey}");
                ProgressItems[nextKey] = new ProgressBarItem()
                {
                    ProgressBarColor = greenProgressBar,
                };
                SettingTitle = InstallationItems.ElementAt(nextKey).Key;
                SettingList = InstallationItems.ElementAt(nextKey).Value;
                if (nextKey == InstallationItems.Count - 1)
                {
                    NextBtnLabel = "Run";
                    NextBtnLabelForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF212121"));
                }
                else
                {
                    BackBtnLabel = "Previous";
                }
            }
            else
            {
                Debug.WriteLine("Already at last item.");

                //send installation items back to mainwindowviewmodel
                delegateInstallationData?.Invoke(InstallationItems);
                isCloseSettingModalEnable = true;
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
                int previousKey = currentSettingListIndex;
                Debug.WriteLine($"PreviousBtn, Currrent Key: {previousKey}");
                ProgressItems[previousKey+1] = new ProgressBarItem()
                {
                    ProgressBarColor = grayProgressBar,
                };
                SettingTitle = InstallationItems.ElementAt(previousKey).Key;
                SettingList = InstallationItems.ElementAt(previousKey).Value;

                if (previousKey == 0)
                {
                    BackBtnLabel = "Back";
                }
                else
                {
                    NextBtnLabel = "Next";
                    NextBtnLabelForeground = Brushes.White;

                }
            }
            else
            {
                Debug.WriteLine("Already at first item.");
                isCloseSettingModalEnable = true;
            }
        }
        public void CheckSelectedObject()
        {
            //Debug.WriteLine($"SettingName : {SettingSelected.SettingName}");
            Debug.WriteLine($"Key : {KeyValuePairSelected.DictionaryKey} | Value : {KeyValuePairSelected.DictionaryValue}");
        }
    }
}
