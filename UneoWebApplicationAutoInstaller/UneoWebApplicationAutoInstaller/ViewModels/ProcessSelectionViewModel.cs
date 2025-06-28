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
        /// <summary>
        /// 要執行的process
        /// </summary>
        private Process processSelectedToProceed;
        public Process ProcessSelectedToProceed
        {
            get
            {
                return processSelectedToProceed;
            }
            set
            {
                processSelectedToProceed = value;
                OnPropertyChanged(nameof(ProcessSelectedToProceed));
            }
        }
        public ObservableCollection<Process> ProcessSelection { get; set; }

        public ProcessSelectionViewModel()
        {
            ProcessSelection = new ObservableCollection<Process>()
            {
                new Process() { ProcessID = (int)ENavigatePage.InstallProcessPage,  ProcessName="Install", ProcessDescription="I don't have the application installed on my computer and I want to install it.",},
                new Process() { ProcessID = (int)ENavigatePage.UpdateProcessPage, ProcessName="Update", ProcessDescription="My application is not working correctly and I want to reinstall it.",},
                new Process() { ProcessID = (int)ENavigatePage.DiagnosticPage, ProcessName="Diagnostic", ProcessDescription="Check server's integrity automatically.", IsEnabled = false, BorderOpacity = 0.5},
                new Process() { ProcessID = (int)ENavigatePage.ConfigurationPage, ProcessName="Configuration", ProcessDescription="Modify setting values of UCB.", IsEnabled = true, BorderOpacity = 1.0},
            };
        }
        public void ProcessBorderHover(bool isHover)
        {
            if (isHover)
            {
                ProcessSelected.InnerBorderBrush = new SolidColorBrush(Colors.Orange);

            }
            else
            {
                ProcessSelected.BorderThickness = 1.5;
                ProcessSelected.BackgroundColor = new SolidColorBrush(Colors.White);
                ProcessSelected.ProcessNameForeground = new SolidColorBrush(Colors.Orange);
                ProcessSelected.NextBtnImage = "/Views/Assets/Icon_GoToNext_Orange.png";
                ProcessSelected.ProcessDescriptionForeground = new SolidColorBrush(Colors.Gray);
                ProcessSelected.BorderBrush = new SolidColorBrush(Colors.Orange);
                ProcessSelected.InnerBorderBrush = new SolidColorBrush(Colors.White);
            }
        }
        public void ProcessBorderMouseButtonDown()
        {   
            ProcessSelected.ProcessNameForeground = new SolidColorBrush(Colors.White);
            ProcessSelected.NextBtnImage = "/Views/Assets/Icon_GoToNext_White.png";
            ProcessSelected.ProcessDescriptionForeground = new SolidColorBrush(Color.FromRgb(249, 249, 250));
            ProcessSelected.BorderBrush = new SolidColorBrush(Colors.Transparent);
            ProcessSelected.InnerBorderBrush = new SolidColorBrush(Colors.Transparent);
            ProcessSelected.BackgroundColor = new SolidColorBrush(Colors.Orange);
            ProcessSelected.BorderThickness = 1.5;
        }

        private DelegateNavigate? delegateNavigate = null;
        public void SetDelegateNavigate(DelegateNavigate del)
        {
            this.delegateNavigate = del;
        }
        public async void ProcessToNextStage()
        {
            //ProcessSelected.ProcessNameForeground = new SolidColorBrush(Colors.Orange);
            //ProcessSelected.NextBtnImage = "/Views/Assets/Icon_GoToNext_Orange.png";
            //ProcessSelected.ProcessDescriptionForeground = new SolidColorBrush(Colors.Gray);
            //ProcessSelected.BorderBrush = new SolidColorBrush(Colors.Orange);
            //ProcessSelected.InnerBorderBrush = new SolidColorBrush(Colors.Orange);

            await Task.Delay(500);
            delegateNavigate?.Invoke(ProcessSelectedToProceed.ProcessID);
        }
    }
}
