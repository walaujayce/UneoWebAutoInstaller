using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Progress : ModelBase
    {
        private int _progressID;
        private string _progressName = "";
        private int _statusState = (int)EInstallStatus.Pending;
        private string _statusImage = "";
        private double _progressNameTextOpacity = 0.5;
        private Visibility _isDetailedListVisible = Visibility.Collapsed; 
        private string _expandImage = "/Views/Assets/Icon_Contract.png";
        //private ObservableCollection<ProgressDetail> _progressDescriptionList = new();
        private ObservableCollection<ProgressDetail> _progressDescriptionList = new();

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
        public double ProgressNameTextOpacity
        {
            get => _progressNameTextOpacity;
            set
            {
                _progressNameTextOpacity = value;
                OnPropertyChanged(nameof(ProgressNameTextOpacity));
            }
        }        
        public Visibility IsDetailedListVisible
        {
            get => _isDetailedListVisible;
            set
            {
                _isDetailedListVisible = value;
                OnPropertyChanged(nameof(IsDetailedListVisible));
            }
        }        
        public string ExpandImage
        {
            get => _expandImage;
            set
            {
                _expandImage = value;
                OnPropertyChanged(nameof(ExpandImage));
            }
        }
        public ObservableCollection<ProgressDetail> ProgressDescriptionList
        {
            get => _progressDescriptionList;
            set
            {
                _progressDescriptionList = value;
                OnPropertyChanged(nameof(ProgressDescriptionList));
            }
        }
    }
}

