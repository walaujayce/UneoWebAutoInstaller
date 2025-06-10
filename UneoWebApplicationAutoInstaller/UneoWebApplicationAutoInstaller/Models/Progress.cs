using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Progress : ModelBase
    {
        private string _progressName = "";
        private string _progressDescription = "";
        public string ProgressName
        {
            get => _progressName;
            set
            {
                _progressName = value;
                OnPropertyChanged(nameof(ProgressName));
            }
        }
        public string ProgressDescription
        {
            get => _progressDescription;
            set
            {
                _progressDescription = value;
                OnPropertyChanged(nameof(ProgressDescription));
            }
        }
    }
}
