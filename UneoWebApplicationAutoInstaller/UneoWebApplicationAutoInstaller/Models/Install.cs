using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Install : ModelBase
    {
        private int _installID;
        private string _installName = "";
        private string _installDescription = ""; 
        private bool _isChecked = false;
        private ObservableCollection<Setting> _settingList = new();
        public int InstallID
        {
            get => _installID;
            set
            {
                _installID = value;
                OnPropertyChanged("InstallID");
            }
        }
        public string InstallName
        {
            get => _installName;
            set
            {
                _installName = value;
                OnPropertyChanged("InstallName");
            }
        }
        public string InstallDescription
        {
            get => _installDescription;
            set
            {
                _installDescription = value;
                OnPropertyChanged("InstallDescription");
            }
        }
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
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
    }
}
