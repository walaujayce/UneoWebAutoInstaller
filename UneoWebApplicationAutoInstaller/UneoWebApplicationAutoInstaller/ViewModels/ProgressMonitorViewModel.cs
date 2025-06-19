using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using static UneoWebApplicationAutoInstaller.ViewModels.ProgressMonitorViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class ProgressMonitorViewModel : ViewModelBase
    {
        #region GET SET
        private ObservableCollection<Progress> _progressList;
        public ObservableCollection<Progress> ProgressList
        {
            get => _progressList;
            set
            {
                _progressList = value;
                OnPropertyChanged(nameof(ProgressList));
            }
        }        
        private Progress _pogressSelected;
        public Progress ProgressSelected
        {
            get => _pogressSelected;
            set
            {
                _pogressSelected = value;
                OnPropertyChanged(nameof(ProgressSelected));
            }
        }
        //private ObservableCollection<ProgressDetail> _progressDetailList;

        #endregion

        private const string TAG = "PM";
        private int errorCount = 0;
        private ProgressDetail temp_progressDetailed;
        public delegate void DelegatEProgressStatus(string message);
        public DelegatEProgressStatus? delegatEProgressStatus = null;

        public ProgressMonitorViewModel()
        {
            _progressList = new ObservableCollection<Progress>();

            // init temp progress detail
            temp_progressDetailed = new ProgressDetail()
            {
                ProgressDescription = "Waiting...",
                ProgressDescriptionTextOpacity = 0.5
            };
        }
        private DelegateNavigate? delegateNavigate = null;
        public void SetDelegateNavigate(DelegateNavigate del)
        {
            this.delegateNavigate = del;
        }
        public void GetSelectedInstallationTodoListVM(List<Progress> progressList)
        {
            _progressList.Clear();
            foreach (var progress in progressList)
            {
                progress.ProgressDescriptionList.Add(temp_progressDetailed);                    
                _progressList.Add(progress);   

            }    
        }
        public void ProgressResponseListener(ProgressDetail progressResult)
        {
            Log.D(TAG, $"ProgressDescription: {progressResult.ProgressDescription} | StatusStatePD: {progressResult.StatusStatePD}");
            if (progressResult == null) return;

            var parentProgress = _progressList.FirstOrDefault(p => p.ProgressID == progressResult.ProgressParentID);
            if (parentProgress == null) return;

            // start processing
            parentProgress.StatusState = (int)EProgressStatus.Ongoing;
            parentProgress.ProgressNameTextOpacity = 1.0;
            
            var existingDetailList = parentProgress.ProgressDescriptionList
                .FirstOrDefault(d => d.ProgressDescription == progressResult.ProgressDescription);

            // remove waiting temp in progress init
            var tempPD = parentProgress.ProgressDescriptionList
                .FirstOrDefault(d => d.ProgressDescription == "Waiting...");
            if(tempPD != null) parentProgress.ProgressDescriptionList.Remove(tempPD);

            if (existingDetailList == null)
            {
                parentProgress.ProgressDescriptionList.Add(new ProgressDetail
                {
                    ProgressDescription = progressResult.ProgressDescription,
                    StatusStatePD = progressResult.StatusStatePD
                });
            }
            else if (existingDetailList.StatusStatePD != progressResult.StatusStatePD)
            {
                existingDetailList.StatusStatePD = progressResult.StatusStatePD;
            }

            // check if any error exist
            if(progressResult.StatusStatePD == (int)EProgressStatus.Fail)
            {
                errorCount++;
            }

            // check is finish processsing, Yes then show result
            if (progressResult.IsFinish)
            {
                if (errorCount > 0)
                {
                    parentProgress.StatusState = (int)EProgressStatus.Warning;
                }
                else
                {
                    parentProgress.StatusState = (int)EProgressStatus.Pass;
                }
                errorCount = 0;
            }
        }
        public void ToggleDetailedListVisibility()
        {
            if (ProgressSelected.IsDetailedListVisible == Visibility.Visible)
            {
                ProgressSelected.IsDetailedListVisible = Visibility.Collapsed;
                ProgressSelected.ExpandImage = "/Views/Assets/Icon_Contract.png";
            }
            else
            {
                ProgressSelected.IsDetailedListVisible = Visibility.Visible;
                ProgressSelected.ExpandImage = "/Views/Assets/Icon_Expand.png";
            }
        }

        #region TEST AREA
        public void Test_Click()
        {
            //Test_1();
        }
        private async void Test_1()
        {
            await Task.Delay(100);
            foreach (var item in _progressList.ToList())
            {
                item.StatusState = (int)EProgressStatus.Ongoing;
                item.ProgressNameTextOpacity = 1.0;
                await Task.Delay(1500);
                if (item.ProgressID % 2 != 0)
                {
                    item.StatusState = (int)EProgressStatus.Pass;
                }
                else
                {
                    item.StatusState = (int)EProgressStatus.Fail;
                }
                await Task.Delay(500);
            }
        }
        #endregion     

    }
}
