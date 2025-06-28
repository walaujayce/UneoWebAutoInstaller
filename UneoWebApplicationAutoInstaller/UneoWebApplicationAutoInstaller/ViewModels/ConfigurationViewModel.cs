using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private SolidColorBrush _libUsbForeground = new SolidColorBrush(Colors.White);
        public SolidColorBrush LibUsbForeground
        {
            get { return _libUsbForeground; }
            set
            {
                _libUsbForeground = value;
                OnPropertyChanged(nameof(LibUsbForeground));
            }
        }
        private SolidColorBrush _ft4222hForeground = new SolidColorBrush(Colors.Black);
        public SolidColorBrush Ft4222hForeground
        {
            get { return _ft4222hForeground; }
            set
            {
                _ft4222hForeground = value;
                OnPropertyChanged(nameof(Ft4222hForeground));
            }
        }
        private int _configurationMode = (int)EUcbConfigurationMode.LibUSB;
        public int ConfigurationMode
        {
            get { return _configurationMode; }
            set
            {
                _configurationMode = value;
                switch (_configurationMode)
                {
                    case (int)EUcbConfigurationMode.LibUSB:
                        LibUsbForeground = new SolidColorBrush(Colors.White);
                        Ft4222hForeground = new SolidColorBrush(Colors.Black);
                        break;
                    case (int)EUcbConfigurationMode.FT4222h:
                        LibUsbForeground = new SolidColorBrush(Colors.Black);
                        Ft4222hForeground = new SolidColorBrush(Colors.White);
                        break;
                }
                OnPropertyChanged(nameof(ConfigurationMode));
            }
        }
    }
}
