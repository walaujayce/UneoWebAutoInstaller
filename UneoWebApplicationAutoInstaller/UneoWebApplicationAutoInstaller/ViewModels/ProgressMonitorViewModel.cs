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
        public void GetSelectedInstallationVM(List<string> selectedInstallation)
        {
            int index = 0;

            _progressList.Clear();
            if (selectedInstallation != null)
            {
                foreach (var install in selectedInstallation)
                {
                    List<string> list = PublicFunction.EnumerateInstallToDoList(install);
                    foreach (var item in list)
                    {
                        _progressList.Add(new Progress()
                        {
                            ProgressID = index,
                            ProgressName = $"{item}",
                        });
                        index++;
                    }
                }
            }    
  
        }
        public void InstallationResponseListener(Dictionary<string, bool> installationResponse)
        {
            
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
                    await Task.Delay(1500);
                    if (item.ProgressID % 2 != 0)
                    {
                        item.StatusState = (int)EInstallStatus.Success;
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
