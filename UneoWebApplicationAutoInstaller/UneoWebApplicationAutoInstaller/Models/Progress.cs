using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Progress : ModelBase
    {
        private int _progressID;
        private string _progressName = "";
        private string _progressDescription = "";
        private int _statusState = (int)EInstallStatus.Pending;
        private string _statusImage = "";
        private double _textOpacity = 0.5;
        
        public bool IsRotating => StatusState == (int)EInstallStatus.Ongoing;
        public int ProgressID
        {
            get => _progressID;
            set
            {
                _progressID = value;
                OnPropertyChanged(nameof(ProgressID));
            }
        }
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
        public int StatusState
        {
            get => _statusState;
            set
            {
                _statusState = value;
                OnPropertyChanged(nameof(IsRotating));

                OnPropertyChanged(nameof(StatusState));
            }
        }
        public string StatusImage
        {
            get => _statusImage;
            set
            {
                _statusImage = value;
                OnPropertyChanged(nameof(StatusImage));
            }
        }
        public double TextOpacity
        {
            get => _textOpacity;
            set
            {
                _textOpacity = value;
                OnPropertyChanged(nameof(TextOpacity));
            }
        }
    }
}
