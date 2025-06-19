using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Update : ModelBase
    {
        private int _updateID;
        private string _updateName = "";
        private string _updateDescription = "";
        private ObservableCollection<Setting> _settingList = new();
        private bool isProceedToSettingModal = false; 
        public int UpdateID
        {
            get => _updateID;
            set
            {
                _updateID = value;
                OnPropertyChanged("UpdateID");
            }
        }
        public string UpdateName
        {
            get => _updateName;
            set
            {
                _updateName = value;
                OnPropertyChanged("UpdateName");
            }
        }
        public string UpdateDescription
        {
            get => _updateDescription;
            set
            {
                _updateDescription = value;
                OnPropertyChanged("UpdateDescription");
            }
        }
        public ObservableCollection<Setting> SettingList
        {
            get => _settingList;
            set
            {
                _settingList = value;
                OnPropertyChanged("SettingList");
            }
        }
        public bool IsProceedToSettingModal
        {
            get => isProceedToSettingModal;
            set
            {
                isProceedToSettingModal = value;
                OnPropertyChanged("IsProceedToSettingModal");
            }
        }
    }
}
