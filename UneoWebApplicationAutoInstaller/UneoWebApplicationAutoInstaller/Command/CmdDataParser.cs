using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using UneoWebApplicationAutoInstaller.Models;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using Newtonsoft.Json.Linq;
using ProcessOrigin = System.Diagnostics.Process;
using System.IO;

namespace UneoWebApplicationAutoInstaller.Command
{
    public class CmdDataParser
    {
        private DelegateProgressResult? delegateProgressResult = null;
        public void SetDelegateProgressResult(DelegateProgressResult del)
        {
            delegateProgressResult = del;
        }

        private List<int> OngoingPass;
        private List<int> OngoingFail;
        private Dictionary<int, List<int>> Test_Progress;
        public CmdDataParser() 
        {
            OngoingPass = new List<int>()
            {
                (int)EInstallStatus.Ongoing,
                (int)EInstallStatus.Pass
            };

            OngoingFail = new List<int>()
            {
                (int)EInstallStatus.Ongoing,
                (int)EInstallStatus.Fail
            };

            //<ProgressID, List of status state>
            Test_Progress = new Dictionary<int, List<int>>()
            {
                { 0, OngoingPass },
                { 1, OngoingFail },
                { 2, OngoingPass },
                { 3, OngoingFail },
                { 4, OngoingFail },
                { 5, OngoingPass },
                { 6, OngoingPass },
            };
        }

        /// <summary>
        /// installID, setting list
        /// </summary>
        /// <param name="installationData"></param>
        public async void DataParser(Dictionary<int, List<Setting>> installationData)
        {

            foreach (var installation in installationData.OrderBy(i => i.Key))
            {
                List<Setting> settingList = installation.Value.ToList();
                switch (installation.Key)
                {
                    case (int)EInstallID.PostgreSQLDatabase:
                        PostgreSQLDatabaseInstallProcess(settingList);
                        break;
                    case (int)EInstallID.WebAPI:
                        WebAPIInstallProcess(settingList);
                        break;
                    case (int)EInstallID.Website:
                        WebsiteInstallProcess(settingList);
                        break;
                    case (int)EInstallID.UMonitorSocketServer:
                        UMonitorSocketServerInstallProcess(settingList);
                        break;
                    case (int)EInstallID.UMonitorService:
                        UMonitorServiceInstallProcess(settingList);
                        break;
                }

                // Wait a moment before go to next step
                await Task.Delay(100);
            }
        }
        
        private async void PostgreSQLDatabaseInstallProcess(List<Setting> settingList)
        {
            //var task = Task.Run(async () =>
            //{
            //    foreach(var item in Test_Progress)
            //    {
            //        foreach(var progress in item.Value)
            //        {
            //            delegateProgressResult?.Invoke(new Progress()
            //            {
            //                ProgressID = item.Key,
            //                StatusState = progress
            //            });
            //            await Task.Delay(1000);
            //        }
            //    }
            //});
            //await task;
        }
        private void WebAPIInstallProcess(List<Setting> settingList)
        {

        }
        private void WebsiteInstallProcess(List<Setting> settingList)
        {

        }
        private void UMonitorSocketServerInstallProcess(List<Setting> settingList)
        {

        }
        private void UMonitorServiceInstallProcess(List<Setting> settingList)
        {

        }
    }
}
