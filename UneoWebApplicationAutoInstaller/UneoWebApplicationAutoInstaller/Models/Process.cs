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
        private SolidColorBrush _borderBrush = new SolidColorBrush(Colors.LightGray);
        private SolidColorBrush _processNameForeground = new SolidColorBrush(Colors.Gray);
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
        public SolidColorBrush ProcessNameForeground
        {
            get => _processNameForeground;
            set
            {
                _processNameForeground = value;
                OnPropertyChanged("ProcessNameForeground");
            }
        }
    }
}
