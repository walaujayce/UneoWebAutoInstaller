using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Views;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class ProcessSelectionViewModel:ViewModelBase
    {
        public ObservableCollection<Process> ProcessSelection { get; set; }

        public ProcessSelectionViewModel()
        {
            ProcessSelection = new ObservableCollection<Process>()
            {
                new Process() { ProcessID = (int)ENavigatePage.InstallProcessPage,  ProcessName="Install", ProcessDescription="I don't have the application installed on my computer and I want to install it.", IsChecked = true, BorderBrush = new SolidColorBrush(Colors.Orange), ProcessNameForeground =new SolidColorBrush(Colors.Black)},
                new Process() { ProcessID = (int)ENavigatePage.UpdateProcessPage, ProcessName="Update", ProcessDescription="My application is not working correctly and I want to reinstall it.", IsChecked = false, BorderBrush = new SolidColorBrush(Colors.LightGray), ProcessNameForeground =new SolidColorBrush(Colors.Gray)},
                new Process() { ProcessID = (int)ENavigatePage.UninstallPage, ProcessName="Uninstall", ProcessDescription="I want to delete the application completely.", IsChecked = false, BorderBrush = new SolidColorBrush(Colors.LightGray), ProcessNameForeground =new SolidColorBrush(Colors.Gray)}
            };
            ProcessSelected = ProcessSelection[1]; //default selection - Install
            SelectChange(ProcessSelected);

        }
        /// <summary>
        /// 目前選到的process
        /// </summary>
        private Process processSelected;
        public Process ProcessSelected
        {
            get
            {
                return processSelected;
            }
            set
            {
                processSelected = value;
                OnPropertyChanged(nameof(ProcessSelected));
            }
        }
        // Radio button change status of each selection
        public void SelectChange(Process processSelected)
        {
            foreach (var process in ProcessSelection)
            {
                if (processSelected.ProcessName == process.ProcessName)
                {
                    process.BorderBrush = new SolidColorBrush(Colors.Orange);
                    process.ProcessNameForeground = new SolidColorBrush (Colors.Black);
                    process.IsChecked = true;
                }
                else
                {
                    process.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    process.ProcessNameForeground = new SolidColorBrush(Colors.Gray);
                    process.IsChecked = false;
                }
            }
        }
        private DelegateNavigate? delegateNavigate = null;
        public void SetDelegateNavigate(DelegateNavigate del)
        {
            this.delegateNavigate = del;
        }
        public void ProcessToNextStage()
        {
            delegateNavigate?.Invoke(ProcessSelected.ProcessID);
        }
    }
}
