using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using static UneoWebApplicationAutoInstaller.ViewModels.ProgressMonitorViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class ProgressMonitorViewModel : ViewModelBase
    {
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
        public delegate void DelegateInstallStatus(string message);
        public DelegateInstallStatus? delegateInstallStatus = null;

        public ProgressMonitorViewModel()
        {
            _progressList = new ObservableCollection<Progress>();
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
                _progressList.Add(progress);
            }  
  
        }
        public void InstallationResponseListener(Dictionary<int, int> progressResult)
        {
            foreach (var item in progressResult)
            {
                _progressList.FirstOrDefault(p => p.ProgressID == item.Key).StatusState = item.Value;
            }
        }
        public void Test_Click()
        {
            Test_1();
        }
        private async void Test_1()
        {
            await Task.Delay(100);
            foreach (var item in _progressList.ToList())
            {
                item.StatusState = (int)EInstallStatus.Ongoing;
                item.TextOpacity = 1.0;
                await Task.Delay(1500);
                if (item.ProgressID % 2 != 0)
                {
                    item.StatusState = (int)EInstallStatus.Pass;
                }
                else
                {
                    item.StatusState = (int)EInstallStatus.Fail;
                }
                await Task.Delay(500);
            }
        }

    }
}
