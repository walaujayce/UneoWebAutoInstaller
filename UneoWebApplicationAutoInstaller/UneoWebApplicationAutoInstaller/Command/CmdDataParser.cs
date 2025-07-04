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
using UneoWebApplicationAutoInstaller.Utilities;
using System.Diagnostics;
using System.Text.Json;
using System.ComponentModel;

namespace UneoWebApplicationAutoInstaller.Command
{
    public class CmdDataParser
    {
        private DelegateProgressResult? delegateProgressResult = null;
        public void SetDelegateProgressResult(DelegateProgressResult del)
        {
            delegateProgressResult = del;
        }

        private const string TAG = "CmdDataParser";
        private const int OVERALL_ATTEMPT_TIMES = 30;
        private const int DEBUG_WAITING_TIME = 500;
        private string imageName_PostgreSQL = "bitnami/postgresql:latest";
        private string containterName_PostgreSQL = "UNEO_DATABASE";
        private const string DATABASE_LOCAL_FOLDER_NAME = "postgres_data";
        private const string DATABASE_NAME = "uneo_web";
        private string imageName_WebAPI = "uneotw/umonitorwebapi:latest";
        private string containerName_WebAPI = "UNEO_WEBAPI";
        private string imageName_Website = "p2211/uext-v1:latest";
        private string containerName_Website = "UNEO_WEBSITE";
        private string imageName_UMonitorServices = "p2211/umonitorservices:latest";
        private string containerName_UMonitorServices = "UNEO_SERVICES";
        public CmdDataParser() 
        {
            
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
                        await PostgreSQLDatabaseInstallProcess(settingList);
                        break;
                    case (int)EInstallID.WebAPI:
                        await WebAPIInstallProcess(settingList);
                        break;
                    case (int)EInstallID.Website:
                        // Init progress result and send to progress monitor
                        ProgressDetail progressDetail_Website = new ProgressDetail();
                        progressDetail_Website.ProgressParentID = (int)EInstallID.Website;
                        await WebsiteInstallProcess(settingList, progressDetail_Website);
                        break;
                    case (int)EInstallID.UMonitorSocketServer:
                        await UMonitorSocketServerInstallProcess(settingList);
                        break;
                    case (int)EInstallID.UMonitorService:
                        await UMonitorServiceInstallProcess(settingList);
                        break;
                }

                // Wait a moment before go to next step
                await Task.Delay(100);
            }
            Debug.WriteLine("End of installation process!");
        }
        public async void DataParserUpdateProcess(Dictionary<int, List<Setting>> installationData)
        {
            List<Setting> settingList = installationData.First().Value.ToList();
            switch (installationData.First().Key)
            {
                case (int)EUpdateID.Website_CONTAINER:
                    await RemakeWebsiteContainer(settingList);
                    break;
                case (int)EUpdateID.Website_IMAGE:
                    await UpdateWebsiteImage(settingList);
                    break;
                case (int)EUpdateID.UMonitorSocketServer_APPSETTINGS:
                    await ModifySocketServerAppSettings(settingList);
                    break;
                case (int)EUpdateID.UMonitorSocketServer_ALL:
                    await UpdateSocketServer();
                    break;
                case (int)EUpdateID.WebAPI_CONTAINER:
                    await RemakeWebAPI(settingList);
                    break;
                case (int)EUpdateID.WebAPI_IMAGE:
                    await UpdateWebApiImage(settingList);
                    break;
                case (int)EUpdateID.UMonitorService_CONTAINER:
                    await RemakeMonitorService(settingList);
                    break;
                case (int)EUpdateID.UMonitorService_IMAGE:
                    await UpdateMonitorService(settingList);
                    break;
            }
            // Wait a moment before go to next step
            await Task.Delay(100);

            Debug.WriteLine("End of update process!");
        }
        // IP changes, remake website container
        private async Task RemakeWebsiteContainer(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_RemakeWebsiteContainer = new ProgressDetail();
            progressDetail_RemakeWebsiteContainer.ProgressParentID = (int)EUpdateID.Website_CONTAINER;

            // Get setting param - image name, container name, ports, envirionment variables
            imageName_Website = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_Website = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            progressDetail_RemakeWebsiteContainer.ProgressDescription = "Stopping current existed container";
            progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);

            // check image existence using image name
            bool isImageExists_Website = await CheckImageExistence(imageName_Website);
            if (isImageExists_Website)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_Website);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container WEBSITE exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_Website);
                    // get running container ID using image name
                    bool isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                    if (isContainerRunning_Website)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_Website);
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);

                        //check again 
                        isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                        if (!isContainerRunning_Website)
                        {
                            progressDetail_RemakeWebsiteContainer.ProgressDescription = "Stopping current existed container";
                            progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Pass;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                            await Task.Delay(100);
                            progressDetail_RemakeWebsiteContainer.ProgressDescription = "Deleting current existed container";
                            progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                        }
                        else
                        {
                            progressDetail_RemakeWebsiteContainer.ProgressDescription = "Stopping current existed container";
                            progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_RemakeWebsiteContainer.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                            return;
                        }

                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);

                    bool isCurrentContainerExisted = await CheckContainerExistenceUsingImageName(imageName_Website);
                    if (!isCurrentContainerExisted)
                    {
                        progressDetail_RemakeWebsiteContainer.ProgressDescription = "Deleting current existed container";
                        progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Pass;
                        delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                    }
                    else
                    {
                        progressDetail_RemakeWebsiteContainer.ProgressDescription = "Deleting current existed container";
                        progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_RemakeWebsiteContainer.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                        return;
                    }
                }
                // use image name to containerize 
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Containerize Website image";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);

                await ContainerizeImage(imageName_Website, containerName_Website, portScript, environmentVariableScript);

            }
            else
            {
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Stopping current existed container";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                await Task.Delay(100);
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Deleting current existed container";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
                await Task.Delay(100);
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Containerize Website image";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);

                await WebsiteInstallProcess(settingList, progressDetail_RemakeWebsiteContainer);
            }
            // confirm container is running            
            bool isWebsiteContainerRunning = await CheckContainerRunningUsingImage(imageName_Website);
            if (isWebsiteContainerRunning)
            {
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Containerize Website image";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Pass;
            }
            else
            {
                progressDetail_RemakeWebsiteContainer.ProgressDescription = "Containerize Website image";
                progressDetail_RemakeWebsiteContainer.StatusStatePD = (int)EProgressStatus.Fail;
            }
            progressDetail_RemakeWebsiteContainer.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_RemakeWebsiteContainer);
        }
        private async Task UpdateWebsiteImage(List<Setting> settingList)
        {            
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UpdateWebsite = new ProgressDetail();
            progressDetail_UpdateWebsite.ProgressParentID = (int)EUpdateID.Website_IMAGE;

            // Get setting param - image name, container name, ports, envirionment variables
            imageName_Website = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_Website = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            progressDetail_UpdateWebsite.ProgressDescription = "Stopping current existed container";
            progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);

            // check image existence using image name
            bool isImageExists_Website = await CheckImageExistence(imageName_Website);
            if (isImageExists_Website)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_Website);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container WEBSITE exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_Website);
                    // get running container ID using image name
                    bool isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                    if (isContainerRunning_Website)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_Website);
                        
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);
                        
                        //check again 
                        isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                        if (!isContainerRunning_Website)
                        {
                            progressDetail_UpdateWebsite.ProgressDescription = "Stopping current existed container";
                            progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Pass;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                            await Task.Delay(100);
                            progressDetail_UpdateWebsite.ProgressDescription = "Deleting current existed container";
                            progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Ongoing;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                        }
                        else
                        {
                            progressDetail_UpdateWebsite.ProgressDescription = "Stopping current existed container";
                            progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_UpdateWebsite.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                            return;
                        }
                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);
                    
                    bool isCurrentContainerExisted = await CheckContainerExistenceUsingImageName(imageName_Website);
                    if (!isCurrentContainerExisted)
                    {
                        progressDetail_UpdateWebsite.ProgressDescription = "Deleting current existed container";
                        progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Pass;
                        delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                    }
                    else
                    {
                        progressDetail_UpdateWebsite.ProgressDescription = "Deleting current existed container";
                        progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_UpdateWebsite.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                        return;
                    }
                }
                // delete image 
                progressDetail_UpdateWebsite.ProgressDescription = "Deleting Website image";
                progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                await DeleteImage(imageName_Website);

                // check image is deleted
                bool isImageDeleted = await CheckImageExistence(imageName_Website);
                if (!isImageDeleted)
                {
                    progressDetail_UpdateWebsite.ProgressDescription = "Deleting Website image";
                    progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                }
                else
                {
                    progressDetail_UpdateWebsite.ProgressDescription = "Deleting Website image";
                    progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_UpdateWebsite.IsFinish= true;
                    delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
                }
            }         

            await WebsiteInstallProcess(settingList, progressDetail_UpdateWebsite);
            // confirm container is running
            bool isWebsiteContainerRunning = await CheckContainerRunningUsingImage(imageName_Website);
            if (isWebsiteContainerRunning)
            {
                progressDetail_UpdateWebsite.ProgressDescription = "Containerize Website image";
                progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Pass;
            }
            else
            {
                progressDetail_UpdateWebsite.ProgressDescription = "Containerize Website image";
                progressDetail_UpdateWebsite.StatusStatePD = (int)EProgressStatus.Fail;
            }
            progressDetail_UpdateWebsite.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UpdateWebsite);
        }
        private async Task ModifySocketServerAppSettings(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UMonitorSocketServer = new ProgressDetail();
            progressDetail_UMonitorSocketServer.ProgressParentID = (int)EUpdateID.UMonitorSocketServer_APPSETTINGS;

            // Initialize UMonitorSocketServer appsettings
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
            progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);

            // load and 
            ObservableCollection<DictionaryInput> appSettingList = new ObservableCollection<DictionaryInput>();
            List<DictionaryInput> temp_appSettingList = settingList.First(s => s.SettingName == "App Settings").KeyValueItems.ToList();
            foreach (var appSetting in temp_appSettingList)
            {
                appSettingList.Add(appSetting);
            }
            PublicFunction.WriteJsonFile(appSettingList, PublicFunction.USocketServer_AppSettings_JSON_FilePath);

            // call setting modal to read json then write again
            await StopUMonitorSocketServerAsync();

            await StartUMonitorSocketServer(progressDetail_UMonitorSocketServer);
            progressDetail_UMonitorSocketServer.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
        }
        private async Task UpdateSocketServer()
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UMonitorSocketServerUpdateAll = new ProgressDetail();
            progressDetail_UMonitorSocketServerUpdateAll.ProgressParentID = (int)EUpdateID.UMonitorSocketServer_ALL;

            await StopUMonitorSocketServerAsync();

            PublicFunction.SelectFolderAndDeleteContents();

            // Initialize UMonitorSocketServer appsettings
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_UMonitorSocketServerUpdateAll.ProgressDescription = "Initialize UMonitorSocketServer";
            progressDetail_UMonitorSocketServerUpdateAll.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServerUpdateAll);

            // load and 
            //ObservableCollection<DictionaryInput> appSettingList = new ObservableCollection<DictionaryInput>();
            //List<DictionaryInput> temp_appSettingList = settingList.First(s => s.SettingName == "App Settings").KeyValueItems.ToList();
            //foreach (var appSetting in temp_appSettingList)
            //{
            //    appSettingList.Add(appSetting);
            //}
            //PublicFunction.WriteJsonFile(appSettingList, PublicFunction.USocketServer_AppSettings_JSON_FilePath);

            await StartUMonitorSocketServer(progressDetail_UMonitorSocketServerUpdateAll);
            progressDetail_UMonitorSocketServerUpdateAll.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServerUpdateAll);
        }
        private async Task RemakeWebAPI(List<Setting> settingList)
        {
            // Stop UMonitor Socket Server before update webapi image
            await StopUMonitorSocketServerAsync();

            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_RemakeWebAPIContainer = new ProgressDetail();
            progressDetail_RemakeWebAPIContainer.ProgressParentID = (int)EUpdateID.WebAPI_CONTAINER;

            // Get setting param - image name, container name, ports
            imageName_WebAPI = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }
            
            containerName_WebAPI = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            progressDetail_RemakeWebAPIContainer.ProgressDescription = "Stopping current existed container";
            progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);

            // check image existence using image name
            bool isImageExists_WebAPI = await CheckImageExistence(imageName_WebAPI);
            if (isImageExists_WebAPI)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_WebAPI);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container WebAPI exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_WebAPI);
                    // get running container ID using image name
                    bool isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                    if (isContainerRunning_WebAPI)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_WebAPI);
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);

                        //check again 
                        isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                        if (!isContainerRunning_WebAPI)
                        {
                            progressDetail_RemakeWebAPIContainer.ProgressDescription = "Stopping current existed container";
                            progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Pass;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                            await Task.Delay(100);
                            progressDetail_RemakeWebAPIContainer.ProgressDescription = "Deleting current existed container";
                            progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                        }
                        else
                        {
                            progressDetail_RemakeWebAPIContainer.ProgressDescription = "Stopping current existed container";
                            progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_RemakeWebAPIContainer.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                            return;
                        }
                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);

                    bool isCurrentContainerExisted = await CheckContainerExistenceUsingImageName(imageName_WebAPI);
                    if (!isCurrentContainerExisted)
                    {
                        progressDetail_RemakeWebAPIContainer.ProgressDescription = "Deleting current existed container";
                        progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Pass;
                        delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                    }
                    else
                    {
                        progressDetail_RemakeWebAPIContainer.ProgressDescription = "Deleting current existed container";
                        progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_RemakeWebAPIContainer.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                        return;
                    }
                }
                // use image name to containerize 
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Containerize WebAPI image";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);

                await ContainerizeImage(imageName_WebAPI, containerName_WebAPI, portScript);
            }
            else
            {
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Stopping current existed container";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                await Task.Delay(100);
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Deleting current existed container";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                await Task.Delay(100);
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Containerize WebAPI image";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
                await WebAPIInstallProcess(settingList);
            }
            // confirm container is running
            bool isWebAPIContainerRunning = await CheckContainerRunningUsingImage(imageName_WebAPI);
            if (isWebAPIContainerRunning)
            {
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Containerize WebAPI image";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Pass;
            }
            else
            {
                progressDetail_RemakeWebAPIContainer.ProgressDescription = "Containerize WebAPI image";
                progressDetail_RemakeWebAPIContainer.StatusStatePD = (int)EProgressStatus.Fail;
            }

            await StartUMonitorSocketServer(progressDetail_RemakeWebAPIContainer);
            progressDetail_RemakeWebAPIContainer.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_RemakeWebAPIContainer);
        }
        private async Task UpdateWebApiImage(List<Setting> settingList)
        {            
            // Stop UMonitor Socket Server before update webapi image
            await StopUMonitorSocketServerAsync();

            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UpdateWebAPI = new ProgressDetail();
            progressDetail_UpdateWebAPI.ProgressParentID = (int)EUpdateID.WebAPI_IMAGE;

            // Get setting param - image name, container name, ports
            imageName_WebAPI = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }

            containerName_WebAPI = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            progressDetail_UpdateWebAPI.ProgressDescription = "Stopping current existed container";
            progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);

            // check image existence using image name
            bool isImageExists_WebAPI = await CheckImageExistence(imageName_WebAPI);
            if (isImageExists_WebAPI)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_WebAPI);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container WebAPI exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_WebAPI);
                    // get running container ID using image name
                    bool isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                    if (isContainerRunning_WebAPI)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_WebAPI);
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);
                        
                        //check again 
                        isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                        if (!isContainerRunning_WebAPI)
                        {
                            progressDetail_UpdateWebAPI.ProgressDescription = "Stopping current existed container";
                            progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                            await Task.Delay(100);
                            progressDetail_UpdateWebAPI.ProgressDescription = "Deleting current existed container";
                            progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                        }
                        else
                        {
                            progressDetail_UpdateWebAPI.ProgressDescription = "Stopping current existed container";
                            progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_UpdateWebAPI.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                            return;
                        }
                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);
                    
                    bool isCurrentContainerExisted = await CheckContainerExistenceUsingImageName(imageName_WebAPI);
                    if (!isCurrentContainerExisted)
                    {
                        progressDetail_UpdateWebAPI.ProgressDescription = "Deleting current existed container";
                        progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                        delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                    }
                    else
                    {
                        progressDetail_UpdateWebAPI.ProgressDescription = "Deleting current existed container";
                        progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_UpdateWebAPI.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                        return;
                    }
                }
                // delete image 
                progressDetail_UpdateWebAPI.ProgressDescription = "Deleting WebAPI image";
                progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);

                await DeleteImage(imageName_WebAPI);

                // check image is deleted
                bool isImageDeleted = await CheckImageExistence(imageName_WebAPI);
                if (!isImageDeleted)
                {
                    progressDetail_UpdateWebAPI.ProgressDescription = "Deleting WebAPI image";
                    progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                }
                else
                {
                    progressDetail_UpdateWebAPI.ProgressDescription = "Deleting WebAPI image";
                    progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_UpdateWebAPI.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
                }
            }
            await WebAPIInstallProcess(settingList);
            // confirm container is running
            bool isWebAPIContainerRunning = await CheckContainerRunningUsingImage(imageName_WebAPI);
            if (isWebAPIContainerRunning)
            {
                progressDetail_UpdateWebAPI.ProgressDescription = "Containerize WebAPI image";
                progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Pass;
            }
            else
            {
                progressDetail_UpdateWebAPI.ProgressDescription = "Containerize WebAPI image";
                progressDetail_UpdateWebAPI.StatusStatePD = (int)EProgressStatus.Fail;
            }

            await StartUMonitorSocketServer(progressDetail_UpdateWebAPI);
            progressDetail_UpdateWebAPI.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UpdateWebAPI);
        }
        private async Task RemakeMonitorService(List<Setting> settingList)
        {
            // Get setting param - image name, container name, ports, envirionment variables
            imageName_UMonitorServices = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_UMonitorServices = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // check image existence using image name
            bool isImageExists_UMonitorService = await CheckImageExistence(imageName_UMonitorServices);
            if (isImageExists_UMonitorService)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_UMonitorServices);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container UMonitorServices exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_UMonitorServices);
                    // get running container ID using image name
                    bool isContainerRunning_UMonitorServices = await CheckContainerRunningUsingImage(imageName_UMonitorServices);
                    if (isContainerRunning_UMonitorServices)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_UMonitorServices);
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);
                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);
                }
                // use image name to containerize 
                await ContainerizeImage(imageName_UMonitorServices, containerName_UMonitorServices, environmentVariableScript);
            }
            else
            {
                await UMonitorServiceInstallProcess(settingList);
            }
            // confirm container is running
            //todo - send confirmation message
            await CheckContainerRunningUsingImage(imageName_UMonitorServices);
        }
        private async Task UpdateMonitorService(List<Setting> settingList)
        {
            // Get setting param - image name, container name, ports, envirionment variables
            imageName_UMonitorServices = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_UMonitorServices = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // check image existence using image name
            bool isImageExists_UMonitorServices = await CheckImageExistence(imageName_UMonitorServices);
            if (isImageExists_UMonitorServices)
            {
                // check container existence using image name
                bool isContainerExist = await CheckContainerExistenceUsingImageName(imageName_UMonitorServices);
                if (isContainerExist)
                {
                    Log.I(TAG, "Container UMonitorServices exists locally.");
                    // get container name using image name
                    string currentExistedContainerName = await GetExistedContainerNameUsingImageName(imageName_UMonitorServices);
                    // get running container ID using image name
                    bool isContainerRunning_UMonitorServices = await CheckContainerRunningUsingImage(imageName_UMonitorServices);
                    if (isContainerRunning_UMonitorServices)
                    {
                        // if running, then stop and delete container
                        string containerId = await GetContainerIdUsingImageName(imageName_UMonitorServices);
                        // use container id to remove current container
                        await StopContainerUsingContainerName(currentExistedContainerName);
                    }
                    // if not running, then delete container using container name
                    await DeleteContainerUsingContainerName(currentExistedContainerName);
                }
                // delete image 
                await DeleteImage(imageName_UMonitorServices);
            }
            await UMonitorServiceInstallProcess(settingList);
            // confirm container is running
            //todo - send confirmation message
            await CheckContainerRunningUsingImage(imageName_UMonitorServices);
        }
        private async Task PostgreSQLDatabaseInstallProcess(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_PostgreSQL = new ProgressDetail();
            progressDetail_PostgreSQL.ProgressParentID = (int)EInstallID.PostgreSQLDatabase;

            // Get setting param - image name, container name, ports, environment variables
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); //C:/Users/uneo
            string databaseLocalFolder = Path.Combine(userProfilePath, DATABASE_LOCAL_FOLDER_NAME);
            imageName_PostgreSQL = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            containterName_PostgreSQL = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }

            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var item in environmentVariablesList)
            {
                environmentVariableScript += $"-e {item.DictionaryKey}={item.DictionaryValue} ";
            }

            // Set local database file "postgres_data" in C:\Users\uneo\
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_PostgreSQL.ProgressDescription = "Create local folder for database";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            bool isDatabaseLocalFolderExists = PublicFunction.CheckFileExist(databaseLocalFolder);
            if(!isDatabaseLocalFolderExists) 
            {
                await CreateDatabaseLocalFolder(databaseLocalFolder);
            }

            // Check again after create local folder
            bool isLocalFolderExistAfterCreate_PostgreSQL = PublicFunction.CheckFileExist(databaseLocalFolder);
            if (!isLocalFolderExistAfterCreate_PostgreSQL)
            {
                int attemptTimes = 0;
                do
                {
                    isLocalFolderExistAfterCreate_PostgreSQL = PublicFunction.CheckFileExist(databaseLocalFolder);
                    if(!isLocalFolderExistAfterCreate_PostgreSQL) await CreateDatabaseLocalFolder(databaseLocalFolder);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < OVERALL_ATTEMPT_TIMES && !isLocalFolderExistAfterCreate_PostgreSQL);
                
                // if fail create local folder, then return FAIL
                if (!isLocalFolderExistAfterCreate_PostgreSQL)
                {
                    Log.E(TAG, "Create local folder for database FAIL");
                    progressDetail_PostgreSQL.ProgressDescription = "Create local folder for database";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_PostgreSQL.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                    // failed then return
                    return;
                }
            }

            progressDetail_PostgreSQL.ProgressDescription = "Create local folder for database";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            // Check if PostgreSQL image already exist, if NO, then pull image
            await Task.Delay(100);
            progressDetail_PostgreSQL.ProgressDescription = "Pull PostgreSQL image";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            bool isImageExists_PostgreSQL = await CheckImageExistence(imageName_PostgreSQL);
            if (isImageExists_PostgreSQL)
            {
                Log.I(TAG, $"Image {imageName_PostgreSQL} has already exist.");
                progressDetail_PostgreSQL.ProgressDescription = "Pull PostgreSQL image";
                progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
            }
            else
            {
                Log.I(TAG, $"Image {imageName_PostgreSQL} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                progressDetail_PostgreSQL.ProgressDescription = "Pull PostgreSQL image";
                progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                await PullImage(imageName_PostgreSQL);

                // Check again after pull image
                int attemptTimes = 0;
                bool isImageExistAfterPull_PostgreSQL;
                do
                {
                    isImageExistAfterPull_PostgreSQL = await CheckImageExistence(imageName_PostgreSQL);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < OVERALL_ATTEMPT_TIMES && !isImageExistAfterPull_PostgreSQL);

                if (isImageExistAfterPull_PostgreSQL)
                {

                    Log.I(TAG, "Pull PostgreSQL image PASS");
                    progressDetail_PostgreSQL.ProgressDescription = "Pull PostgreSQL image";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                }
                else
                {
                    Log.E(TAG, "Pull PostgreSQL image FAIL");
                    progressDetail_PostgreSQL.ProgressDescription = "Pull PostgreSQL image";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_PostgreSQL.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                    // failed then return
                    return;
                }
            }

            // Check if PostgreSQL container already exist, if No, then containerize the image
            Log.I(TAG, "Check if PostgreSQL container already exist, if No, then containerize the image");
            progressDetail_PostgreSQL.ProgressDescription = "Containerize PostgreSQL image";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            bool isContainerExists_PostgreSQL = await CheckContainerExistenceUsingImageName(imageName_PostgreSQL);

            if (isContainerExists_PostgreSQL)
            {
                //container exist, check if it is running
                int checkContainerAttemptTimes = 0;
                bool isContainerRunning_PostgreSQL = false;
                do
                {
                    isContainerRunning_PostgreSQL = await CheckContainerRunningUsingImage(imageName_PostgreSQL);
                    if (!isContainerRunning_PostgreSQL)
                    {
                        await RunContainerUsingContainerName(containterName_PostgreSQL);
                    }
                    checkContainerAttemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_PostgreSQL && checkContainerAttemptTimes < OVERALL_ATTEMPT_TIMES);

                Log.I(TAG, "Container PostgreSQL exists locally.");
                progressDetail_PostgreSQL.ProgressDescription = "Containerize PostgreSQL image";
                progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
            }
            else
            {
                Log.I(TAG, "Container PostgreSQL does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");
#if DEBUG
                await ContainerizeDatabaseImage(imageName_PostgreSQL, containterName_PostgreSQL, databaseLocalFolder, "-p 5430:5432 ");
#else
                await ContainerizeDatabaseImage(imageName_PostgreSQL, containterName_PostgreSQL, databaseLocalFolder, portScript, environmentVariableScript);
#endif
                //Check again if the container is running
                int checkContainerRunningAttemptTimes = 0;
                bool isContainerRunning_PostgreSQL = false;
                do
                {
                    isContainerRunning_PostgreSQL = await CheckContainerRunningUsingImage(imageName_PostgreSQL);
                    checkContainerRunningAttemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_PostgreSQL && checkContainerRunningAttemptTimes < OVERALL_ATTEMPT_TIMES);

                if (isContainerRunning_PostgreSQL)
                {
                    Log.I(TAG, "Containerize PostgreSQL image PASS");
                    progressDetail_PostgreSQL.ProgressDescription = "Containerize PostgreSQL image";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                }
                else
                {
                    Log.E(TAG, "Containerize PostgreSQL image FAIL");
                    progressDetail_PostgreSQL.ProgressDescription = "Containerize PostgreSQL image";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_PostgreSQL.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                    // failed then return
                    return;
                }
            }


            // Create database "uneo_web"
            progressDetail_PostgreSQL.ProgressDescription = $"Create database \"{DATABASE_NAME}\"";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            //Get container ID from image name
            string containerIdFile = "container_id.txt";

            // Retrieve the container ID and store it temporarily
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                $"docker ps -q -l -f \"ancestor={imageName_PostgreSQL}\" > {containerIdFile}",
                "Get PostgreSQL container ID from image name");

            // Read the container ID from file
            string containerId = File.ReadAllText(containerIdFile).Trim();

            //Use container ID to create database
#if DEBUG
            string checkDbCommand = $"docker exec {containerId} psql -U postgres -tAc \"SELECT 1 FROM pg_database WHERE datname=\'{DATABASE_NAME}\';\"";
#else
            string checkDbCommand = $"docker exec {environmentVariableScript}{containerId} psql -U postgres -tAc \"SELECT 1 FROM pg_database WHERE datname=\'{DATABASE_NAME}\';\"";
#endif
            int databaseExistAttemtpTimes = 0;
            while (!((await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkDbCommand, $"Check if {DATABASE_NAME} database exists")).Trim() == "1"))
            {
                databaseExistAttemtpTimes++;
                await Task.Delay(5000);
#if DEBUG
                await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                    $"docker exec {containerId} psql -U postgres -c \"CREATE DATABASE {DATABASE_NAME};\"",
                    $"Creating database {DATABASE_NAME}");
#else
                await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                    $"docker exec {environmentVariableScript}{containerId} psql -U postgres -c \"CREATE DATABASE {DATABASE_NAME};\"",
                    $"Creating database {DATABASE_NAME}");
#endif
                if (databaseExistAttemtpTimes >= 20)
                {
                    Log.E(TAG, $"Failed to create {DATABASE_NAME}, please check Docker.");
                    databaseExistAttemtpTimes = 0;
                    // Fail to create database then return
                    progressDetail_PostgreSQL.ProgressDescription = $"Create database \"{DATABASE_NAME}\"";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_PostgreSQL.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                    return;
                }
            };

            // database "uneo_web" exist, then delegate pass
            progressDetail_PostgreSQL.ProgressDescription = $"Create database \"{DATABASE_NAME}\"";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            // Cleanup the temp file
            File.Delete(containerIdFile);

            // Init database tables by dump-postgres file
            progressDetail_PostgreSQL.ProgressDescription = $"Initialize database \"{DATABASE_NAME}\"";
            progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);

            string containerSqlFilePath = "/tmp/dump.sql";
            string postgreSQLPath = Path.Combine(AppContext.BaseDirectory, "PostgreSQL", "dump-postgres.sql");

            if (!File.Exists(postgreSQLPath))
            {
                try
                {
                    int searchPostgreSQLPathAttempTimes = 0;
                    DirectoryInfo? di = Directory.GetParent(AppContext.BaseDirectory)?.Parent;
                    while (!File.Exists(postgreSQLPath) && searchPostgreSQLPathAttempTimes <= 15)
                    {
                        searchPostgreSQLPathAttempTimes++;
                        if (di == null || di.Parent == null)
                        {
                            continue;
                        }
                        else
                        {
                            string projectRoot = di.FullName;
                            postgreSQLPath = Path.Combine(projectRoot, "PostgreSQL", "dump-postgres.sql");
                            di = di.Parent;
                        }                            
                    }
                    if (!File.Exists(postgreSQLPath))
                    {
                        progressDetail_PostgreSQL.ProgressDescription = $"Initialize database \"{DATABASE_NAME}\"";
                        progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_PostgreSQL.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.E(TAG, "Cant find dump-postgreSQL file path.");
                    progressDetail_PostgreSQL.ProgressDescription = $"Initialize database \"{DATABASE_NAME}\"";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_PostgreSQL.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                    return;
                }
            }

            //Debug.WriteLine("File Path: " + postgreSQLPath);
            //Debug.WriteLine("File Path: " + File.Exists(postgreSQLPath));

            //if database already has any tables, it cant be restore with SQL dump-temp
            string checkTablesCommand = $"docker exec {environmentVariableScript}{containerId} psql -U postgres -d {DATABASE_NAME} -tAc \"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';\"";
            string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkTablesCommand, "Checking if uneo_web has any tables");
            int table_existed_count = int.TryParse(result.Trim(), out int tableCount) ? tableCount : 0;
            if (table_existed_count == 0)
            {
                // Check again after initialize database
                int checkDatabaseHasTablesAttemptTimes = 0;
                string checkAgainResult;
                do
                {
                    //Copy SQL dump file to container
                    await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                        $"docker cp \"{postgreSQLPath}\" {containerId}:{containerSqlFilePath}",
                        "Copy SQL dump into container");

                    //Restore SQL dump into the new database
                    await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                        $"docker exec {environmentVariableScript}{containerId} pg_restore -U postgres -d {DATABASE_NAME} {containerSqlFilePath}",
                        $"Restore database {DATABASE_NAME}");

                    await Task.Delay(1000);
                    checkAgainResult = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkTablesCommand, "Checking if uneo_web has any tables");
                    table_existed_count = int.TryParse(checkAgainResult.Trim(), out int tableCountAgain) ? tableCountAgain : 0;

                } while (table_existed_count == 0 && checkDatabaseHasTablesAttemptTimes <= OVERALL_ATTEMPT_TIMES);
                if(table_existed_count > 0)
                {
                    progressDetail_PostgreSQL.ProgressDescription = $"Initialize database \"{DATABASE_NAME}\"";
                    progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                }

            }
            else
            {
                Log.E(TAG, $"Database {DATABASE_NAME} with tables has already existed, delete it first before restore process continue to execute.");
                progressDetail_PostgreSQL.ProgressDescription = $"Initialize database \"{DATABASE_NAME}\"";
                progressDetail_PostgreSQL.StatusStatePD = (int)EProgressStatus.Fail;
                progressDetail_PostgreSQL.IsFinish = true;
                delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
                return;
            }
            

            // invoke progress monitor that all process finish
            progressDetail_PostgreSQL.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_PostgreSQL);
        }
        private async Task WebAPIInstallProcess(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_WebAPI = new ProgressDetail();
            progressDetail_WebAPI.ProgressParentID = (int)EInstallID.WebAPI;

            // Get setting param - image name, container name, ports
            imageName_WebAPI = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            
            List<DictionaryInput> portList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portsScript = "";
            foreach (var port in portList)
            {
                portsScript += $"-p {port.DictionaryValue} ";
            }

            containerName_WebAPI = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // Check if WebAPI image already exist, if NO, then pull image
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
            progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);

            bool isImageExists_WebAPI = await CheckImageExistence(imageName_WebAPI);
            if (isImageExists_WebAPI)
            {
                Log.I(TAG, $"Image {imageName_WebAPI} has already exist.");
                progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
            }
            else
            {
                Log.I(TAG, $"Image {imageName_WebAPI} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
                await PullImage(imageName_WebAPI);

                // Check again after pull image
                int attemptTimes = 0;
                bool isImageExistAfterPull_WebAPI;
                do
                {
                    isImageExistAfterPull_WebAPI = await CheckImageExistence(imageName_WebAPI);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < OVERALL_ATTEMPT_TIMES && !isImageExistAfterPull_WebAPI);

                if (isImageExistAfterPull_WebAPI)
                {

                    Log.I(TAG, "Pull WebAPI image PASS");
                    progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                }
                else
                {
                    Log.E(TAG, "Pull WebAPI image FAIL");
                    progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_WebAPI.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                    // failed then return
                    return;
                }
            }


            // Check if WebAPI container already exist, if No, then containerize the image
            progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
            progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);

            bool isContainerExists_WEBAPI = await CheckContainerExistenceUsingImageName(imageName_WebAPI);

            if (isContainerExists_WEBAPI)
            {
                //container exist, check if it is running
                int attemptTimes = 0;
                bool isContainerRunning_WebAPI = false;
                do
                {
                    isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                    if (!isContainerRunning_WebAPI)
                    {
                        await RunContainerUsingContainerName(containerName_WebAPI);
                    }
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_WebAPI && attemptTimes < OVERALL_ATTEMPT_TIMES);

                Log.I(TAG, "Container UmonitorWebAPI exists locally.");
                progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
            }
            else
            {
                Log.I(TAG, "Container UmonitorWebAPI does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");
                await ContainerizeImage(imageName_WebAPI, containerName_WebAPI, portsScript);

                //Check again if the container is running
                int attemptTimes = 0;
                bool isContainerRunning_WebAPI = false;
                do
                {
                    isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_WebAPI && attemptTimes < OVERALL_ATTEMPT_TIMES);

                if (isContainerRunning_WebAPI)
                {
                    Log.I(TAG, "Containerize WebAPI image PASS");
                    progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                }
                else
                {
                    Log.E(TAG, "Containerize WebAPI image FAIL");
                    progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_WebAPI.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                    // failed then return
                    return;
                }
            }
            // invoke progress monitor that all process finish
            progressDetail_WebAPI.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);
        }
        private async Task WebsiteInstallProcess(List<Setting> settingList, ProgressDetail progressDetail_Website)
        {


            // Get setting param - image name, container name, ports, envirionment variables
            imageName_Website = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            
            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portsList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_Website = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // Check if Website image already exist, if NO, then pull image
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_Website.ProgressDescription = "Pull Website image";
            progressDetail_Website.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_Website);
            
            bool isImageExists_Website = await CheckImageExistence(imageName_Website);
            if (isImageExists_Website)
            {
                Log.I(TAG, $"Image {imageName_Website} has already exist.");
                progressDetail_Website.ProgressDescription = "Pull Website image";
                progressDetail_Website.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_Website);
            }
            else
            {
                Log.I(TAG, $"Image {imageName_Website} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                progressDetail_Website.ProgressDescription = "Pull Website image";
                progressDetail_Website.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_Website);
                await PullImage(imageName_Website);

                // Check again after pull image
                int attemptTimes = 0;
                bool isImageExistAfterPull_Website;
                do
                {
                    isImageExistAfterPull_Website = await CheckImageExistence(imageName_Website);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < OVERALL_ATTEMPT_TIMES && !isImageExistAfterPull_Website);

                if (isImageExistAfterPull_Website)
                {

                    Log.I(TAG, "Pull Website image PASS");
                    progressDetail_Website.ProgressDescription = "Pull Website image";
                    progressDetail_Website.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_Website);
                }
                else
                {
                    Log.E(TAG, "Pull Website image FAIL");
                    progressDetail_Website.ProgressDescription = "Pull Website image";
                    progressDetail_Website.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_Website.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_Website);
                    // failed then return
                    return;
                }
            }


            // Check if WebAPI container already exist, if No, then containerize the image
            progressDetail_Website.ProgressDescription = "Containerize Website image";
            progressDetail_Website.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_Website);

            bool isContainerExists_Website = await CheckContainerExistenceUsingImageName(imageName_Website);

            if (isContainerExists_Website)
            {
                //container exist, check if it is running
                int attemptTimes = 0;
                bool isContainerRunning_Website = false;
                do
                {
                    isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                    if (!isContainerRunning_Website)
                    {
                        await RunContainerUsingContainerName(containerName_Website);
                    }
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_Website && attemptTimes < OVERALL_ATTEMPT_TIMES);

                Log.I(TAG, "Container Website exists locally.");
                progressDetail_Website.ProgressDescription = "Containerize Website image";
                progressDetail_Website.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_Website);
            }
            else
            {
                Log.I(TAG, "Container Website does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");
                await ContainerizeImage(imageName_Website, containerName_Website, portScript, environmentVariableScript);

                //Check again if the container is running
                int attemptTimes = 0;
                bool isContainerRunning_Website = false;
                do
                {
                    isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_Website);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_Website && attemptTimes < OVERALL_ATTEMPT_TIMES);

                if (isContainerRunning_Website)
                {
                    Log.I(TAG, "Containerize Website image PASS");
                    progressDetail_Website.ProgressDescription = "Containerize Website image";
                    progressDetail_Website.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_Website);
                }
                else
                {
                    Log.E(TAG, "Containerize Website image FAIL");
                    progressDetail_Website.ProgressDescription = "Containerize Website image";
                    progressDetail_Website.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_Website.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_Website);
                    // failed then return
                    return;
                }
            }
            // invoke progress monitor that all process finish
            progressDetail_Website.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_Website);

        }
        private async Task UMonitorSocketServerInstallProcess(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UMonitorSocketServer = new ProgressDetail();
            progressDetail_UMonitorSocketServer.ProgressParentID = (int)EInstallID.UMonitorSocketServer;

            // Initialize UMonitorSocketServer appsettings
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
            progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);

            // load and 
            ObservableCollection<DictionaryInput> appSettingList = new ObservableCollection<DictionaryInput>();
            List<DictionaryInput> temp_appSettingList = settingList.First(s => s.SettingName == "App Settings").KeyValueItems.ToList();
            foreach (var appSetting in temp_appSettingList)
            {
                appSettingList.Add(appSetting);
            }
            PublicFunction.WriteJsonFile(appSettingList, PublicFunction.USocketServer_AppSettings_JSON_FilePath);

            await StartUMonitorSocketServer(progressDetail_UMonitorSocketServer);
            
            progressDetail_UMonitorSocketServer.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
        }
        private async Task UMonitorServiceInstallProcess(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_UMonitorServices = new ProgressDetail();
            progressDetail_UMonitorServices.ProgressParentID = (int)EInstallID.UMonitorService;

            // Get setting param - image name, container name, envirionment variables
            imageName_UMonitorServices = settingList.First(s => s.SettingName == "Image Name").SettingValue;

            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var ev in environmentVariablesList)
            {
                environmentVariableScript += $"-e {ev.DictionaryKey}={ev.DictionaryValue} ";
            }
            containerName_UMonitorServices = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // Check if UMonitorServices image already exist, if NO, then pull image
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_UMonitorServices.ProgressDescription = "Pull UMonitorServices image";
            progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UMonitorServices);

            bool isImageExists_Website = await CheckImageExistence(imageName_UMonitorServices);
            if (isImageExists_Website)
            {
                Log.I(TAG, $"Image {imageName_UMonitorServices} has already exist.");
                progressDetail_UMonitorServices.ProgressDescription = "Pull UMonitorServices image";
                progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
            }
            else
            {
                Log.I(TAG, $"Image {imageName_UMonitorServices} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                progressDetail_UMonitorServices.ProgressDescription = "Pull UMonitorServices image";
                progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
                await PullImage(imageName_UMonitorServices);

                // Check again after pull image
                int attemptTimes = 0;
                bool isImageExistAfterPull_Website;
                do
                {
                    isImageExistAfterPull_Website = await CheckImageExistence(imageName_UMonitorServices);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < OVERALL_ATTEMPT_TIMES && !isImageExistAfterPull_Website);

                if (isImageExistAfterPull_Website)
                {

                    Log.I(TAG, "Pull UMonitorServices image PASS");
                    progressDetail_UMonitorServices.ProgressDescription = "Pull UMonitorServices image";
                    progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
                }
                else
                {
                    Log.E(TAG, "Pull UMonitorServices image FAIL");
                    progressDetail_UMonitorServices.ProgressDescription = "Pull UMonitorServices image";
                    progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_UMonitorServices.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
                    // failed then return
                    return;
                }
            }


            // Check if WebAPI container already exist, if No, then containerize the image
            progressDetail_UMonitorServices.ProgressDescription = "Containerize UMonitorServices image";
            progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_UMonitorServices);

            bool isContainerExists_Website = await CheckContainerExistenceUsingImageName(imageName_UMonitorServices);

            if (isContainerExists_Website)
            {
                //container exist, check if it is running
                int attemptTimes = 0;
                bool isContainerRunning_Website = false;
                do
                {
                    isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_UMonitorServices);
                    if (!isContainerRunning_Website)
                    {
                        await RunContainerUsingContainerName(containerName_UMonitorServices);
                    }
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_Website && attemptTimes < OVERALL_ATTEMPT_TIMES);

                Log.I(TAG, "Container UMonitorServices exists locally.");
                progressDetail_UMonitorServices.ProgressDescription = "Containerize UMonitorServices image";
                progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
            }
            else
            {
                Log.I(TAG, "Container UMonitorServices does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");
                await ContainerizeImage(imageName_UMonitorServices, containerName_UMonitorServices, environmentVariableScript);

                //Check again if the container is running
                int attemptTimes = 0;
                bool isContainerRunning_Website = false;
                do
                {
                    isContainerRunning_Website = await CheckContainerRunningUsingImage(imageName_UMonitorServices);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_Website && attemptTimes < OVERALL_ATTEMPT_TIMES);

                if (isContainerRunning_Website)
                {
                    Log.I(TAG, "Containerize UMonitorServices image PASS");
                    progressDetail_UMonitorServices.ProgressDescription = "Containerize UMonitorServices image";
                    progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
                }
                else
                {
                    Log.E(TAG, "Containerize UMonitorServices image FAIL");
                    progressDetail_UMonitorServices.ProgressDescription = "Containerize UMonitorServices image";
                    progressDetail_UMonitorServices.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_UMonitorServices.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
                    // failed then return
                    return;
                }
            }
            // invoke progress monitor that all process finish
            progressDetail_UMonitorServices.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_UMonitorServices);
        }
        private async Task StartUMonitorSocketServer(ProgressDetail progressDetail_UMonitorSocketServer)
        {
            // Check UMonitorSocketServer has run
            string processName = "UMonitorSocketServer";
            bool isUMonitorSocketServerRunning = await CheckUMonitorSocketServerIsRunning(processName);
            if (!isUMonitorSocketServerRunning) // Process not found
            {
                if (Config.IsCheckWebAPIContainerRunning)
                {
                    bool isWebAPIContainerRunning = false;
                    int count = 0;
                    do
                    {
                        isWebAPIContainerRunning = await CheckContainerRunningUsingImage(imageName_WebAPI);
                        if (!isWebAPIContainerRunning)
                        {
                            Log.I(TAG, "The WebAPI container isn't running yet.");
                            Log.I(TAG, "Waiting for 5 seconds...");
                            await Task.Delay(1000);
                            count++;
                        }

                        if (count >= 30)
                        {
                            Log.E(TAG, "Failed to start WebAPI container, please check Docker.");

                            progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
                            progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_UMonitorSocketServer.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                            return;
                        }

                    } while (!isWebAPIContainerRunning);

                    if (isWebAPIContainerRunning)
                    {
                        progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
                        progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Pass;
                        delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                    }
                }
                else
                {
                    // skip checking webapi container running, so "initialize" porgress detail delegate fake "PASS"
                    progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
                    progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                }

                // START umonitorsocketserver
                
                progressDetail_UMonitorSocketServer.ProgressDescription = "Start UMonitorSocketServer";
                progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);

                string uMonitorSocketServerExeFilePath = Path.Combine(AppContext.BaseDirectory, "UMonitorSocketServer", "publish", "UMonitorSocketServer.exe");

                //if (!File.Exists(uMonitorSocketServerExeFilePath))
                //{
                //    string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
                //    uMonitorSocketServerExeFilePath = Path.Combine(projectRoot, "UMonitorSocketServer", "publish", "UMonitorSocketServer.exe");
                //}
                if (!File.Exists(uMonitorSocketServerExeFilePath))
                {
                    try
                    {
                        int searchUMonitorSocketServerPathAttempTimes = 0;
                        DirectoryInfo? di = Directory.GetParent(AppContext.BaseDirectory)?.Parent;
                        while (!File.Exists(uMonitorSocketServerExeFilePath) && searchUMonitorSocketServerPathAttempTimes <= 15)
                        {
                            searchUMonitorSocketServerPathAttempTimes++;
                            if (di == null || di.Parent == null)
                            {
                                continue;
                            }
                            else
                            {
                                string projectRoot = di.FullName;
                                uMonitorSocketServerExeFilePath = Path.Combine(projectRoot, "UMonitorSocketServer", "publish", "UMonitorSocketServer.exe");
                                di = di.Parent;
                            }
                        }

                        if (!File.Exists(uMonitorSocketServerExeFilePath))
                        {
                            progressDetail_UMonitorSocketServer.ProgressDescription = $"Start UMonitorSocketServer";
                            progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Fail;
                            progressDetail_UMonitorSocketServer.IsFinish = true;
                            delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.E(TAG, "Cant find dump-UMonitorSocketServer file path.");
                        progressDetail_UMonitorSocketServer.ProgressDescription = $"Start UMonitorSocketServer";
                        progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Fail;
                        progressDetail_UMonitorSocketServer.IsFinish = true;
                        delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                        return;
                    }
                }

                await Task.Delay(5000);
                await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                        $"cmd.exe /c \"powershell -ExecutionPolicy Bypass -WindowStyle Hidden -Command Start-Process '{uMonitorSocketServerExeFilePath}' -WindowStyle Minimized\"",
                        "Run UMonitorSocketServer.exe bypassing SmartScreen");


                if (await CheckUMonitorSocketServerIsRunning(processName))
                {
                    progressDetail_UMonitorSocketServer.ProgressDescription = $"Start UMonitorSocketServer";
                    progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                }
                else
                {
                    progressDetail_UMonitorSocketServer.ProgressDescription = $"Start UMonitorSocketServer";
                    progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Fail;
                    progressDetail_UMonitorSocketServer.IsFinish = true;
                    delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
                    return;
                }
                
            }
            else
            {
                Log.I(TAG, "UMonitorSocketServer is already running. Skipping execution.");
                progressDetail_UMonitorSocketServer.ProgressDescription = "Initialize UMonitorSocketServer";
                progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Pass;
                progressDetail_UMonitorSocketServer.ProgressDescription = "Start UMonitorSocketServer";
                progressDetail_UMonitorSocketServer.StatusStatePD = (int)EProgressStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_UMonitorSocketServer);
            }
        }
        public static async Task StopUMonitorSocketServerAsync()
        {
            string processName = "UMonitorSocketServer";
            string checkProcessCommand = $"tasklist /FI \"IMAGENAME eq {processName}.exe\" | findstr /I {processName}";

            // Check if the process is running
            string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkProcessCommand, "Check if UMonitorSocketServer is already running");

            if (result.Contains(processName, StringComparison.OrdinalIgnoreCase)) // Process not found
            {
                Log.I(TAG, "UMonitorSocketServer is already running. Stopping it first...");

                await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                    $"taskkill /F /IM {processName}.exe",
                    "Stopping UMonitorSocketServer.exe");
            }
        }
        private async Task<bool> CheckUMonitorSocketServerIsRunning(string processName)
        {
            string checkProcessCommand = $"tasklist /FI \"IMAGENAME eq {processName}.exe\" | findstr /I {processName}";
            string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkProcessCommand, "Check if UMonitorSocketServer is already running");
            return result.Contains(processName, StringComparison.OrdinalIgnoreCase);
        }
        // METHOD
        private async Task<bool> CheckImageExistence(string imageName)
        {
            bool commandSuccess = await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                $"docker image inspect {imageName}",
                $"Checking if image '{imageName}' exists."
            );
            return commandSuccess;
        }
        private async Task PullImage(string imageName)
        {
            await Task.Delay(DEBUG_WAITING_TIME);
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"docker pull {imageName}", $"Pull {imageName} image");
        }
        /// <summary>
        /// this script return container name whether container is running or not as long as container exist
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private async Task<bool> CheckContainerExistenceUsingImageName(string imageName)
        {
            //return bool if result has string
            bool commandSuccess = await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync(
                $"docker ps -a --filter \"ancestor={imageName}\" --format \"{{.Names}}\" | findstr .",
                $"Check if {imageName} container exists locally."
            );
            return commandSuccess;
        }
        /// <summary>
        /// this script return container name whether container is running or not as long as container exist
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private async Task<string> GetExistedContainerNameUsingImageName(string imageName)
        {
            //return bool if result has string
            string containerName = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                $"docker ps -a --filter \"ancestor={imageName}\" --format \"{{{{.Names}}}}\" | findstr .",
                $"Check if {imageName} container exists locally."
            );
            return containerName;
        }
        private async Task ContainerizeImage(string imageName, string containerName, string ports = "", string environmentVariables = "")
        {
            await Task.Delay(DEBUG_WAITING_TIME);

            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"docker run -d --restart unless-stopped {environmentVariables}{ports}--name {containerName} {imageName}", $"Containerize {imageName} image");
        }
        /// <summary>
        /// this script return container ID only if container is exist and running
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private async Task<bool> CheckContainerRunningUsingImage(string imageName)
        {
            // if container is running return containerID, if not then return null
            string containerId = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                    $"docker ps -q -f \"ancestor={imageName}\"",
                    $"Check if {imageName} container is running");

            return !string.IsNullOrEmpty(containerId); // Check if output is NOT empty
        }
        private async Task RunContainerUsingContainerName(string containerName)
        {
            await Task.Delay(DEBUG_WAITING_TIME);
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"docker start {containerName}", $"Run {containerName} container");
        }
        private async Task CreateDatabaseLocalFolder(string databaseLocalFolderPath)
        {            
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"mkdir {databaseLocalFolderPath}", "Created database folder on local PC");
        }
        private async Task ContainerizeDatabaseImage(string imageName, string containerName, string folderPath, string ports, string environmentVariables = "")
        {
#if DEBUG
            Debug.WriteLine($"docker run -d --restart unless-stopped -e ALLOW_EMPTY_PASSWORD=yes {ports}-v {folderPath}:/bitnami/postgresql --name {containerName} {imageName}");
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"docker run -d --restart unless-stopped -e ALLOW_EMPTY_PASSWORD=yes {ports}-v {folderPath}:/bitnami/postgresql --name {containerName} {imageName}", "Containerized postgresql image");
#else
            await CommandExecutor.Instance.RunCommandAsAdminReturnBoolAsync($"docker run -d --restart unless-stopped {environmentVariables}{ports}-v {folderPath}:/bitnami/postgresql --name {containerName} {imageName}", "Containerized postgresql image");
#endif
        }
        private async Task<string> GetContainerIdUsingImageName(string imageName)
        {
            string containerID = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                    $"docker ps -q -f \"ancestor={imageName}\"",
                    $"Get {imageName} container ID");
            return containerID;
        }
        private async Task StopContainerUsingContainerName(string containerName)
        {
            await CommandExecutor.Instance.RunCommandAsAdminAsync(
                            $"docker stop {containerName}",
                            $"Stopping the {containerName} container");
        }
        private async Task DeleteContainerUsingContainerName(string containerName)
        {
            await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker rm {containerName}",
                $"Deleting the {containerName} container");
        }
        private async Task DeleteImage(string imageName)
        {
            await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker rmi -f {imageName}", "Deleting the image");
        }
    }
}
