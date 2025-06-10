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
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using static UneoWebApplicationAutoInstaller.ViewModels.ProgressMonitorViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class ProgressMonitorViewModel : ViewModelBase
    {
        public ObservableCollection<Progress> ProgressList { get; set; }

        public delegate void DelegateInstallStatus(string message);
        public DelegateInstallStatus? delegateInstallStatus = null;

        public ProgressMonitorViewModel()
        {
        //    ProgressList = new ObservableCollection<Progress>()
        //    {
        //        new Progress() { ProgressName="PostgreSQL Database", ProgressDescription="I don't have the application installed on my computer and I want to install it."},
        //        new Progress() { ProgressName="WebAPI", ProgressDescription="My application is not working correctly and I want to reinstall it."}
        //    };
        }
        private DelegateNavigate? delegateNavigate = null;
        public void SetDelegateNavigate(DelegateNavigate del)
        {
            this.delegateNavigate = del;
        }
        private void ProcessStatusListener(string message)
        {
            Debug.WriteLine("message in listener: " + message);
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressList.Clear();
                ProgressList.Add(new Progress() { ProgressName = $"{message}" });               
            });
        }
    }
}
