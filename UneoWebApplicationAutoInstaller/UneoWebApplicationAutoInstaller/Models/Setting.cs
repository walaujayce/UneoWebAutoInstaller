using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.Models
{

    public class Setting : ModelBase
    {
        private string _settingTitle = "";
        private int _settingType = (int)ESettingType.SingleInput;
        private string _settingName = "";
        private string _settingDescription = "";
        private string _settingValue = "";
        private ObservableCollection<DictionaryInput> _inputList = new();
        private ObservableCollection<DictionaryInput> _keyValueItems = new();

        public string SettingTitle
        {
            get => _settingTitle;
            set
            {
                _settingTitle = value;
                OnPropertyChanged(nameof(SettingTitle));
            }
        }
        public int SettingType
        {
            get => _settingType;
            set
            {
                _settingType = value;
                OnPropertyChanged(nameof(SettingType));
            }
        }
        public string SettingName
        {
            get => _settingName;
            set
            {
                _settingName = value;
                OnPropertyChanged(nameof(SettingName));
            }
        }
        public string SettingDescription
        {
            get => _settingDescription;
            set
            {
                _settingDescription = value;
                OnPropertyChanged(nameof(SettingDescription));
            }
        }
        public string SettingValue
        {
            get => _settingValue;
            set
            {
                _settingValue = value;
                OnPropertyChanged(nameof(SettingValue));
            }
        }
        public ObservableCollection<DictionaryInput> InputList
        {
            get => _inputList;
            set
            {
                _inputList = value;
                if(_inputList.Count > 1)
                {
                    foreach (var item in _inputList)
                    {
                        if (item == _inputList.Last()) continue;
                        item.IsRemovable = true;
                    }
                }
                OnPropertyChanged(nameof(InputList)); 
            }
        }
        public ObservableCollection<DictionaryInput> KeyValueItems
        {
            get => _keyValueItems;
            set
            {
                _keyValueItems = value;
                if (_keyValueItems.Count > 1)
                {
                    foreach (var item in _keyValueItems)
                    {
                        if (item == _keyValueItems.Last()) continue;
                        item.IsRemovable = true;
                    }
                }
                OnPropertyChanged(nameof(KeyValueItems));
            }
        }
    }
}
