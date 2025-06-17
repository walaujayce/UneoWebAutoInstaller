using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using System.Windows;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class ProgressDetail : ModelBase
    {
        private int _progressParentID;
        private string _progressDescription = "";
        private int _statusStatePD = (int)EInstallStatus.Pending;
        private string _statusImagePD = "";
        private double __progressDescriptionTextOpacity = 1.0;
        private bool _isFinish = false;
        public bool IsRotating => StatusStatePD == (int)EInstallStatus.Ongoing;
        public int ProgressParentID
        {
            get => _progressParentID;
            set
            {
                _progressParentID = value;
                OnPropertyChanged(nameof(ProgressParentID));
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
        public int StatusStatePD
        {
            get => _statusStatePD;
            set
            {
                _statusStatePD = value;
                OnPropertyChanged(nameof(IsRotating));

                OnPropertyChanged(nameof(StatusStatePD));
            }
        }
        public string StatusImagePD
        {
            get => _statusImagePD;
            set
            {
                _statusImagePD = value;
                OnPropertyChanged(nameof(StatusImagePD));
            }
        }
        public double ProgressDescriptionTextOpacity
        {
            get => __progressDescriptionTextOpacity;
            set
            {
                __progressDescriptionTextOpacity = value;
                OnPropertyChanged(nameof(ProgressDescriptionTextOpacity));
            }
        }
        public bool IsFinish
        {
            get => _isFinish;
            set
            {
                _isFinish = value;
                OnPropertyChanged(nameof(IsFinish));
            }
        }
    }

}
