using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Process : ModelBase
    {
        private int _processID;
        private string _processName = "";
        private string _processDescription = "";
        private bool _isChecked = false;
        private bool _isEnabled = true;
        private SolidColorBrush _backgroundColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush _borderBrush = new SolidColorBrush(Colors.Orange);
        private SolidColorBrush _innerBorderBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush _processNameForeground = new SolidColorBrush(Colors.Orange); 
        private SolidColorBrush _processDescriptionForeground = new SolidColorBrush(Colors.Gray); 
        private string _nextBtnImage = "/Views/Assets/Icon_GoToNext_Orange.png";
        private double _borderThickness = 1.5; 
        private double _borderOpacity = 1.0; 
        public int ProcessID
        {
            get => _processID;
            set
            {
                _processID = value;
                OnPropertyChanged("ProcessID");
            }
        }
        public string ProcessName
        {
            get => _processName;
            set
            {
                _processName = value;
                OnPropertyChanged("ProcessName");
            }
        }
        public string ProcessDescription
        {
            get => _processDescription;
            set
            {
                _processDescription = value;
                OnPropertyChanged("ProcessDescription");
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
        public SolidColorBrush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged("BorderBrush");
            }
        }
        public SolidColorBrush InnerBorderBrush
        {
            get => _innerBorderBrush;
            set
            {
                _innerBorderBrush = value;
                OnPropertyChanged("InnerBorderBrush");
            }
        }
        public double BorderThickness
        {
            get => _borderThickness;
            set
            {
                _borderThickness = value;
                OnPropertyChanged("BorderThickness");
            }
        }
        public SolidColorBrush ProcessNameForeground
        {
            get => _processNameForeground;
            set
            {
                _processNameForeground = value;
                OnPropertyChanged("ProcessNameForeground");
            }
        }
        
        public SolidColorBrush ProcessDescriptionForeground
        {
            get => _processDescriptionForeground;
            set
            {
                _processDescriptionForeground = value;
                OnPropertyChanged("ProcessDescriptionForeground");
            }
        }
        public SolidColorBrush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }      
        public string NextBtnImage
        {
            get => _nextBtnImage;
            set
            {
                _nextBtnImage = value;
                OnPropertyChanged("NextBtnImage");
            }
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public double BorderOpacity
        {
            get => _borderOpacity;
            set
            {
                _borderOpacity = value;
                OnPropertyChanged("BorderOpacity");
            }
        }        
    }
}
