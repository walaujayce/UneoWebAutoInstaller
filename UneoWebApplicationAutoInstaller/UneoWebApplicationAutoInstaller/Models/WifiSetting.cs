using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class WifiSetting : ModelBase
    {
        private int settingID;
        public int SettingID
        {
            get => settingID;
            set
            {
                settingID = value;
                OnPropertyChanged("SettingID");
            }
        }
        private string settingTitle = "";
        public string SettingTitle
        {
            get => settingTitle;
            set
            {
                settingTitle = value;
                OnPropertyChanged("SettingTitle");
            }
        }
        private ObservableCollection<DictionaryInput> settingList = new();
        public ObservableCollection<DictionaryInput> SettingList
        {
            get => settingList;
            set
            {
                settingList = value;
                OnPropertyChanged("SettingList");
            }
        }
        public static bool DHCP { get; set; } = true;
        public static int Security { get; set; } = 2;
        public static string SSID { get; set; } = "";
        public static string PSK { get; set; } = "";               
        public static string MAC { get; set; } = "";
        public static string UCB_IP { get; set; } = "";
        public static string Netmask { get; set; } = "";
        public static string Gateway { get; set; } = "";
        public static string Server_IP { get; set; } = "";
        public static int PORT { get; set; } = 7282;
        public static int RSSI { get; set; } = 0;
    }

}
