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
        private string POSTGRESQL_IMAGE = "bitnami/postgresql:latest";
        private string POSTGRESQL_CONTAINER_NAME = "UNEO_DATABASE";
        private string DATABASE_NAME = "uneo_web";
        private string imageName_WebAPI = "uneotw/umonitorwebapi:latest";
        private string containerName_WebAPI = "UNEO_WEBAPI";
        private string WEBSITE_IMAGE = "p2211/uext-v1:latest";
        private string WEBSITE_CONTAINER_NAME = "UNEO_WEBSITE";
        private string UMONITORSERVICES_IMAGE = "p2211/umonitorservices:latest";
        private string UMONITORSERVICES_CONTAINER_NAME = "UNEO_SERVICES";

        private List<int> OngoingPass;
        private List<int> OngoingFail;
        private Dictionary<int, List<int>> Test_Progress;
        public bool IsTestSuccess { get; set; } = false;
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
        public async Task<bool> DataParser(Dictionary<int, List<Setting>> installationData)
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
            return IsTestSuccess;
        }
        
        private async void PostgreSQLDatabaseInstallProcess(List<Setting> settingList)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); //C:/Users/uneo
            string POSTGRESQL_FOLDER = Path.Combine(userProfilePath, "postgres_data");
            POSTGRESQL_IMAGE = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            POSTGRESQL_CONTAINER_NAME = settingList.First(s => s.SettingName == "Container Name").SettingValue;
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var item in environmentVariablesList)
            {
                environmentVariableScript += $"-e {item.DictionaryKey}={item.DictionaryValue} ";
            }
            DATABASE_NAME = "uneo_web";

            //1. Set local database file "postgres_data" in C:\Users\uneo\
            if (!PublicFunction.CheckFileExist(POSTGRESQL_FOLDER))
            {
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"mkdir {POSTGRESQL_FOLDER}", "Created database folder on local PC");
            }
            Debug.WriteLine("Result: " + IsTestSuccess);

            //2. docker pull bitnami/postgresql:latest
            bool is_PostgreSQL_Image_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker image inspect {POSTGRESQL_IMAGE}", "check if postgresql image has already exist.");
            if (is_PostgreSQL_Image_Exists)
            {
                Log.I(TAG, $"Image {POSTGRESQL_IMAGE} has already exist.");
            }
            else
            {
                Log.I(TAG, $"Image {POSTGRESQL_IMAGE} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker pull {POSTGRESQL_IMAGE}", $"Pull {POSTGRESQL_IMAGE} image");
            }
            //3. docker run -d --restart unless-stopped -e POSTGRESQL_PASSWORD=uccc07568009 -p 5432:5432 -v C:\Users\uneo\postgres_data:/bitnami/postgresql --name UNEO_DATABASE bitnami/postgresql:latest
            bool is_PostgreSQL_Container_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker ps -a --filter \"ancestor={POSTGRESQL_IMAGE}\" --format \"{{.Names}}\" | findstr .",
                "Check if PostgreSQL container exists locally."
            );

            if (is_PostgreSQL_Container_Exists)
            {
                Log.I(TAG, "Container PostgreSQL exists locally.");
            }
            else
            {
                Log.I(TAG, "Container PostgreSQL does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");

#if DEBUG
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker run -d --restart unless-stopped -e ALLOW_EMPTY_PASSWORD=yes -p 5430:5432 -v {POSTGRESQL_FOLDER}:/bitnami/postgresql --name {POSTGRESQL_CONTAINER_NAME} {POSTGRESQL_IMAGE}", "Containerized postgresql image");
#else
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker run -d --restart unless-stopped -e POSTGRESQL_PASSWORD={POSTGRESQL_PASSWORD} -p 5432:5432 -v {POSTGRESQL_FOLDER}:/bitnami/postgresql --name {POSTGRESQL_CONTAINER_NAME} {POSTGRESQL_IMAGE}", "Containerized postgresql image");
#endif
            }

            //4. create database "uneo_web"
            //Check if the container has run
            bool isRunning = false;
            int count = 0;
            do
            {
                string postgreSQLContainerId = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                    $"docker ps -q -f \"ancestor={POSTGRESQL_IMAGE}\"",
                    "Check if PostgreSQL container is running");

                isRunning = !string.IsNullOrEmpty(postgreSQLContainerId); // Check if output is NOT empty

                if (!isRunning)
                {
                    Log.I(TAG, "The PostgreSQL container isn't running yet.");
                    Log.I(TAG, "Waiting for 5 seconds...");
                    await Task.Delay(5000);
                    count++;
                }

                if (count >= 5)
                {
                    Log.E(TAG, "Failed to start PostgreSQL container, please check Docker.");
                    count = 0;
                    break;
                }

            } while (!isRunning);

            if (isRunning)
            {
                Log.I(TAG, "PostgreSQL container is now running.");
            }

            //Get container ID from image name
            string containerIdFile = "container_id.txt";

            // Retrieve the container ID and store it temporarily
            await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker ps -q -l -f \"ancestor={POSTGRESQL_IMAGE}\" > {containerIdFile}",
                "Get PostgreSQL container ID from image name");

            // Read the container ID from file
            string containerId = File.ReadAllText(containerIdFile).Trim();
            Log.I(TAG, containerId);
            if (string.IsNullOrEmpty(containerId))
            {
                Debug.WriteLine("Postgresql container not running.");
                return;
            }

            //Use container ID to create database
            string checkDbCommand = $"docker exec {environmentVariableScript}{containerId} psql -U postgres -tAc \"SELECT 1 FROM pg_database WHERE datname=\'{DATABASE_NAME}\';\"";
            while (!((await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkDbCommand, $"Check if {DATABASE_NAME} database exists")).Trim() == "1"))
            {
                Log.I(TAG, "Wait for 5 seconds");
                count++;
                await Task.Delay(5000);
                await CommandExecutor.Instance.RunCommandAsAdminAsync(
                    $"docker exec -e {environmentVariableScript}{containerId} psql -U postgres -c \"CREATE DATABASE {DATABASE_NAME};\"",
                    $"Creating database {DATABASE_NAME}");

                if (count >= 5)
                {
                    Log.E(TAG, $"Failed to start {DATABASE_NAME}, please check Docker.");
                    count = 0;
                    break;
                }
            };

            // Cleanup the temp file
            File.Delete(containerIdFile);

            //5. restore template via sql file "dump-postgres.sql"
            string containerSqlFilePath = "/tmp/dump.sql";
            string postgreSQLPath = Path.Combine(AppContext.BaseDirectory, "PostgreSQL", "dump-postgres.sql");
            if (!File.Exists(postgreSQLPath))
            {
                string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
                postgreSQLPath = Path.Combine(projectRoot, "PostgreSQL", "dump-postgres.sql");
            }
            //Debug.WriteLine("File Path: " + postgreSQLPath);
            //Debug.WriteLine("File Path: " + File.Exists(postgreSQLPath));

            //if database already has any tables, it cant be restore with SQL dump-temp
            string checkTablesCommand = $"docker exec -e {environmentVariableScript}{containerId} psql -U postgres -d {DATABASE_NAME} -tAc \"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';\"";
            string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkTablesCommand, "Checking if uneo_web has any tables");
            int table_existed_count = int.TryParse(result.Trim(), out int tableCount) ? tableCount : 0;
            if (table_existed_count == 0)
            {
                //Copy SQL dump file to container
                await CommandExecutor.Instance.RunCommandAsAdminAsync(
                    $"docker cp \"{postgreSQLPath}\" {containerId}:{containerSqlFilePath}",
                    "Copy SQL dump into container");

                //Restore SQL dump into the new database
                await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                    $"docker exec {environmentVariableScript}{containerId} pg_restore -U postgres -d {DATABASE_NAME} {containerSqlFilePath}",
                    $"Restore database {DATABASE_NAME}");
            }
            else
            {
                Log.I(TAG, $"Database {DATABASE_NAME} with tables has already existed, delete it first before restore process continue to execute.");
            }
        }
        private async void WebAPIInstallProcess(List<Setting> settingList)
        {
            // Init progress result and send to progress monitor
            ProgressDetail progressDetail_WebAPI = new ProgressDetail();
            progressDetail_WebAPI.ProgressParentID = 1;

            // Get setting param - image name, container name, ports
            imageName_WebAPI = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            
            List<DictionaryInput> portList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var port in portList)
            {
                portScript += $"-p {port.DictionaryValue} ";
            }

            containerName_WebAPI = settingList.First(s => s.SettingName == "Container Name").SettingValue;

            // Check if WebAPI image already exist, if NO, then pull image
            await Task.Delay(100); // system run too fast, need to wait it delegate
            progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
            progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);

            bool isImageExists_WebAPI = await CheckImageExistence(imageName_WebAPI);
            if (isImageExists_WebAPI)
            {
                Log.I(TAG, $"Image {imageName_WebAPI} has already exist.");
                progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
            }
            else
            {
                Log.I(TAG, $"Image {imageName_WebAPI} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Ongoing;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
                PullImage(imageName_WebAPI);

                // Check again after pull image
                int attemptTimes = 0;
                bool isImageExistAfterPull_WebAPI;
                do
                {
                    isImageExistAfterPull_WebAPI = await CheckImageExistence(imageName_WebAPI);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (attemptTimes < 20 && !isImageExistAfterPull_WebAPI);

                if (isImageExistAfterPull_WebAPI)
                {

                    Log.I(TAG, "Pull WebAPI image PASS");
                    progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                }
                else
                {
                    Log.I(TAG, "Pull WebAPI image FAIL");
                    progressDetail_WebAPI.ProgressDescription = "Pull WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Fail;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                    // failed then return
                    return;
                }
            }


            // Check if WebAPI container already exist, if No, then containerize the image
            progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
            progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Ongoing;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);

            bool isContainerExists_WEBAPI = await CheckContainerExistenceUsingImageName(imageName_WebAPI);

            if (isContainerExists_WEBAPI)
            {
                // TO-DO 
                // TO-DO 
                // TO-DO 
                // TO-DO 
                // TO-DO 
                // TO-DO 
                Log.I(TAG, "Container UmonitorWebAPI exists locally.");
                progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Pass;
                delegateProgressResult?.Invoke(progressDetail_WebAPI);
            }
            else
            {
                Log.I(TAG, "Container UmonitorWebAPI does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");
                ContainerizeImage(imageName_WebAPI, containerName_WebAPI, portScript);

                //Check again if the container is running
                int attemptTimes = 0;
                bool isContainerRunning_WebAPI = false;
                do
                {
                    isContainerRunning_WebAPI = await CheckContainerRunningUsingImage(imageName_WebAPI);
                    attemptTimes++;
                    await Task.Delay(1000);
                } while (!isContainerRunning_WebAPI && attemptTimes < 20);

                if (isContainerRunning_WebAPI)
                {
                    Log.I(TAG, "Containerize WebAPI image PASS");
                    progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Pass;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                }
                else
                {
                    Log.I(TAG, "Containerize WebAPI image FAIL");
                    progressDetail_WebAPI.ProgressDescription = "Containerize WebAPI image";
                    progressDetail_WebAPI.StatusStatePD = (int)EInstallStatus.Fail;
                    delegateProgressResult?.Invoke(progressDetail_WebAPI);
                    // failed then return
                    return;
                }
            }
            // invoke progress monitor that all process finish
            progressDetail_WebAPI.IsFinish = true;
            delegateProgressResult?.Invoke(progressDetail_WebAPI);

        }
        private async void WebsiteInstallProcess(List<Setting> settingList)
        {
            WEBSITE_IMAGE = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            WEBSITE_CONTAINER_NAME = settingList.First(s => s.SettingName == "Container Name").SettingValue;
            List<DictionaryInput> portsList = settingList.First(s => s.SettingName == "Ports").InputList.ToList();
            string portScript = "";
            foreach (var item in portsList)
            {
                portScript += $"-p {item.DictionaryValue} ";
            }
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var item in environmentVariablesList)
            {
                environmentVariableScript += $"-e {item.DictionaryKey}={item.DictionaryValue} ";
            }
            //8. docker pull p2211/uext-v1:latest
            bool is_WEBSITE_Image_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker image inspect {WEBSITE_IMAGE}", $"check if {WEBSITE_IMAGE} has already exist.");
            if (is_WEBSITE_Image_Exists)
            {
                Log.I(TAG, $"Image {WEBSITE_IMAGE} has already exist.");
            }
            else
            {
                Log.I(TAG, $"Image {WEBSITE_IMAGE} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker pull {WEBSITE_IMAGE}", $"Pull {WEBSITE_IMAGE} image");
            }


            //9. docker run -d --restart unless-stopped -p 8005:5173 -e VITE_WEBAPI_URL=192.9.120.142 -e VITE_SOCKETSERVER_URL=192.168.2.200 --name UNEO_WEBSITE p2211/uext-v1:latest

            bool is_WEBSITE_Container_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker ps -a --filter \"ancestor={WEBSITE_IMAGE}\" --format \"{{.Names}}\" | findstr .",
                "Check if WEBSITE container exists locally."
            );

            if (is_WEBSITE_Container_Exists)
            {
                Log.I(TAG, "Container WEBSITE exists locally.");
            }
            else
            {
                Log.I(TAG, "Container WEBSITE does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");

                Debug.WriteLine("script: " + $"docker run -d --restart unless-stopped {portScript}{environmentVariableScript}--name {WEBSITE_CONTAINER_NAME} {WEBSITE_IMAGE}");
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker run -d --restart unless-stopped {portScript}{environmentVariableScript}--name {WEBSITE_CONTAINER_NAME} {WEBSITE_IMAGE}", "Containerize Website image");

            }

        }
        private async void UMonitorSocketServerInstallProcess(List<Setting> settingList)
        {
            //10. run UMONITORSOCKETSERVER after ALL docker images containerize
            await RunUMonitorSocketServerAsync();

            Debug.WriteLine("\nEND OF PROCESS!!!");
        }
        private async void UMonitorServiceInstallProcess(List<Setting> settingList)
        {
            UMONITORSERVICES_IMAGE = settingList.First(s => s.SettingName == "Image Name").SettingValue;
            UMONITORSERVICES_CONTAINER_NAME = settingList.First(s => s.SettingName == "Container Name").SettingValue;
            List<DictionaryInput> environmentVariablesList = settingList.First(s => s.SettingName == "Environment Variables").KeyValueItems.ToList();
            string environmentVariableScript = "";
            foreach (var item in environmentVariablesList)
            {
                environmentVariableScript += $"-e {item.DictionaryKey}={item.DictionaryValue} ";
            }
            //8. docker pull p2211/umonitorservices:latest
            bool is_UMONITORSERVICES_Image_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker image inspect {UMONITORSERVICES_IMAGE}", $"check if {UMONITORSERVICES_IMAGE} has already exist.");
            if (is_UMONITORSERVICES_Image_Exists)
            {
                Log.I(TAG, $"Image {UMONITORSERVICES_IMAGE} has already exist.");
            }
            else
            {
                Log.I(TAG, $"Image {UMONITORSERVICES_IMAGE} doesn't exist.");
                Log.I(TAG, "Start to pull image");
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker pull {UMONITORSERVICES_IMAGE}", $"Pull {UMONITORSERVICES_IMAGE} image");
            }


            //9. docker run -d --restart unless-stopped -e WEBAPI_URL=192.9.120.142 -e LOCAL_URL=192.168.2.200 --name UNEO_SERVICES p2211/umonitorservices:latest

            bool is_UMONITORSERVICES_Container_Exists = await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker ps -a --filter \"ancestor={UMONITORSERVICES_IMAGE}\" --format \"{{.Names}}\" | findstr .",
                "Check if UMONITORSERVICES container exists locally."
            );

            if (is_UMONITORSERVICES_Container_Exists)
            {
                Log.I(TAG, "Container WEBSITE exists locally.");
            }
            else
            {
                Log.I(TAG, "Container WEBSITE does NOT exist locally.");
                Log.I(TAG, "Start to containerize image");

                Debug.WriteLine("script: " + $"docker run -d --restart unless-stopped {environmentVariableScript}--name {UMONITORSERVICES_CONTAINER_NAME} {UMONITORSERVICES_IMAGE}");
                await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker run -d --restart unless-stopped {environmentVariableScript}--name {UMONITORSERVICES_CONTAINER_NAME} {UMONITORSERVICES_IMAGE}", "Containerize UMONITORSERVICES image");
            }
        }
        public async Task RunUMonitorSocketServerAsync()
        {
            string processName = "UMonitorSocketServer";
            string checkProcessCommand = $"tasklist /FI \"IMAGENAME eq {processName}.exe\" | findstr /I {processName}";

            // Check if the process is running
            string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(checkProcessCommand, "Check if UMonitorSocketServer is already running");

            if (!result.Contains(processName, StringComparison.OrdinalIgnoreCase)) // Process not found
            {              

                bool isRunning;
                int count = 0;
                do
                {
                    string webAPIContainerID = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                        $"docker ps -q -f \"ancestor={imageName_WebAPI}\"",
                        "Check if WebAPI container is running");

                    isRunning = !string.IsNullOrEmpty(webAPIContainerID); // Check if output is NOT empty
                    Debug.WriteLine(webAPIContainerID);
                    if (!isRunning)
                    {
                        Log.I(TAG, "The WebAPI container isn't running yet.");
                        Log.I(TAG, "Waiting for 5 seconds...");
                        await Task.Delay(5000);
                        count++;
                    }

                    if (count >= 5)
                    {
                        Log.E(TAG, "Failed to start WebAPI container, please check Docker.");
                        break;
                    }

                } while (!isRunning);

                string uMonitorSocketServerExeFilePath = Path.Combine(AppContext.BaseDirectory, "UMonitorSocketServer", "publish", "UMonitorSocketServer.exe");
                if (!File.Exists(uMonitorSocketServerExeFilePath))
                {
                    string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
                    uMonitorSocketServerExeFilePath = Path.Combine(projectRoot, "UMonitorSocketServer", "publish", "UMonitorSocketServer.exe");
                }

                for (int i = 10; i > 0; i--)
                {
                    Log.I(TAG, $"Executing UMonitorSocketServer in {i} seconds...");
                    await Task.Delay(1000);
                }
                await CommandExecutor.Instance.RunCommandAsAdminAsync(
                        $"cmd.exe /c \"powershell -ExecutionPolicy Bypass -WindowStyle Hidden -Command Start-Process '{uMonitorSocketServerExeFilePath}' -WindowStyle Minimized\"",
                        "Run UMonitorSocketServer.exe bypassing SmartScreen");
            }
            else
            {
                Log.I(TAG, "UMonitorSocketServer is already running. Skipping execution.");
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

                await CommandExecutor.Instance.RunCommandAsAdminAsync(
                    $"taskkill /F /IM {processName}.exe",
                    "Stopping UMonitorSocketServer.exe");
            }
        }
        private async Task<bool> CheckImageExistence(string imageName)
        {
            bool commandSuccess = await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker image inspect {imageName}",
                $"Checking if image '{imageName}' exists."
            );
            return commandSuccess;
        }
        private async void PullImage(string imageName)
        {
            await Task.Delay(3000);
            await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker pull {imageName}", $"Pull {imageName} image");
        }
        private async Task<bool> CheckContainerExistenceUsingImageName(string imageName)
        {
            bool commandSuccess = await CommandExecutor.Instance.RunCommandAsAdminAsync(
                $"docker ps -a --filter \"ancestor={imageName}\" --format \"{{.Names}}\" | findstr .",
                $"Check if {imageName} container exists locally."
            );
            return commandSuccess;
        }
        private async void ContainerizeImage(string imageName, string containerName, string ports = "", string environmentVariables = "")
        {
            await Task.Delay(3000);

            await CommandExecutor.Instance.RunCommandAsAdminAsync($"docker run -d --restart unless-stopped {ports}--name {containerName} {imageName}", $"Containerize {imageName} image");
        }
        private async Task<bool> CheckContainerRunningUsingImage(string imageName)
        {
            string postgreSQLContainerId = await CommandExecutor.Instance.RunCommandAsAdminReturnStringAsync(
                    $"docker ps -q -f \"ancestor={imageName}\"",
                    $"Check if {imageName} container is running");

            return !string.IsNullOrEmpty(postgreSQLContainerId); // Check if output is NOT empty
        }

    }
}
