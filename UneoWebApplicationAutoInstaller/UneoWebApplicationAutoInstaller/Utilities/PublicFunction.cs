using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UneoWebApplicationAutoInstaller.Models;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using Process = System.Diagnostics.Process;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class PublicFunction
    {
        private const string TAG = "Public function";
        /// <summary>
        /// Read JSON file
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<DictionaryInput> LoadJsonFile()
        {
            ObservableCollection<DictionaryInput> AppSettingsList = new()
            {
                new DictionaryInput()
                {
                    DictionaryKey = "Null",
                    DictionaryValue = "Null"
                }
            };
            try
            {
                
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UMonitorSocketServer", "publish", "appsettings.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Dictionary<string, object>? AppSettingsJSON = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (AppSettingsJSON == null) return AppSettingsList;
                    AppSettingsList.Clear();
                    foreach (var item in AppSettingsJSON)
                    {
                        AppSettingsList.Add(new DictionaryInput()
                        {
                            DictionaryKey = item.Key,
                            DictionaryValue = item.Value
                        });
                    }
                    return AppSettingsList;
                }
                else
                {
                    Debug.WriteLine("appsetting.json not found.");
                    return AppSettingsList;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading appsetting.json: " + ex.Message);
                return AppSettingsList;
            }
        }
        public static string GetInstallationName(int installationID)
        { 
            switch (installationID)
            {
                case (int)EInstallID.PostgreSQLDatabase:
                    return "PostgreSQL Database";
                case (int)EInstallID.WebAPI:
                    return "WebAPI";
                case (int)EInstallID.UMonitorSocketServer:
                    return "UMonitorSocketServer";
                case (int)EInstallID.Website:
                    return "Website";
                case (int)EInstallID.UMonitorService:
                    return "UMonitorService";
                default:
                    return "";
            }
        }
        /// <summary>
        /// 詳列安裝事項
        /// </summary>
        /// <param name="installationName"></param>
        /// <returns></returns>
        public static List<string> EnumerateInstallToDoList(int installationID)
        {   
            List<string> list = new();
            switch (installationID)
            {
                case (int)EInstallID.PostgreSQLDatabase:
                    list = new List<string>()
                    {
                        "Set up local database file",
                        "Pull PostgreSQL image \"bitnami/postgresql:latest\"",
                        "Containerize PostgreSQL image",
                        "Create \"uneo_web\" database",
                        "Check PostgreSQL container is running"
                    };
                    break;
                case (int)EInstallID.WebAPI:
                    list = new List<string>()
                    {
                        "Pull WebAPI image \"uneotw/umonitorwebapi:latest\"",
                        "Containerize WebAPI image",
                        "Check WebAPI container is running"
                    };
                    break;
                case (int)EInstallID.Website:
                    list = new List<string>()
                    {
                        "Pull Website image \"p2211/uext-v1:latest\"",
                        "Containerize Website image",
                        "Check Website container is running"
                    };
                    break;
                case (int)EInstallID.UMonitorSocketServer:
                    list = new List<string>()
                    {
                        "Run UMonitorSocketServer",
                        "Check UMonitorSocketServer is running"
                    };
                    break;
                case (int)EInstallID.UMonitorService:
                    list = new List<string>()
                    {
                        "Pull UMonitorService image \"p2211/umonitorservices:latest\"",
                        "Containerize UMonitorService image",
                        "Check UMonitorService container is running"
                    };
                    break;
            }
            return list;
        }

        /// <summary>
        /// 取得所有網路介面卡名稱
        /// </summary>
        /// <returns>Return nullable value (Interface name : IP Adress)</returns>
        public static Dictionary<string, string>? GetAllInterfaceNameAndIP()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Ignore vEthernet interfaces
                if (networkInterface.Name.StartsWith("vEthernet")) continue;
                //if (networkInterface.Name.StartsWith("Loopback")) continue; 
                //if (networkInterface.Name.StartsWith("Wi-Fi")) continue; 

                // Only check interfaces that are up
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {

                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        // Only consider IPv4 addresses
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // Console all interface names and IPs
                            //Debug.WriteLine($"Interface: {networkInterface.Name}, IP: {ip.Address}");
                            keyValuePairs[networkInterface.Name] = ip.Address.ToString();
                        }
                    }
                }
            }
            return keyValuePairs.Count() > 0 ? keyValuePairs : null;
        }
        /// <summary>
        /// 取得 Ethernet IP
        /// </summary>
        /// <returns></returns>
        public static string GetEthernetIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Ignore vEthernet interfaces
                if (networkInterface.Name.StartsWith("vEthernet")) continue;

                // 只檢查啟用狀態的網絡介面
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        // 選擇 IPv4 地址
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // 判斷介面類型
                            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 取得 Wireless80211 IP
        /// </summary>
        /// <returns></returns>
        public static string GetWireless80211IPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Ignore vEthernet interfaces
                if (networkInterface.Name.StartsWith("vEthernet")) continue;

                // 只檢查啟用狀態的網絡介面
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        // 選擇 IPv4 地址
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // 判斷介面類型
                            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 取得 Docker Container Bridge IP
        /// </summary>
        /// <returns></returns>
        public static string GetDockerContainerBridgeIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.Name == "eth0" &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 取得Docker Container 的 Gateway
        /// </summary>
        /// <returns></returns>
        public static string GetDockerContainerBridgeGatewayAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Check for the network interface named "eth0"
                if (networkInterface.Name == "eth0" &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    // Find the first IPv4 gateway address
                    foreach (GatewayIPAddressInformation gateway in networkInterface.GetIPProperties().GatewayAddresses)
                    {
                        if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return gateway.Address.ToString();
                        }
                    }
                }
            }
            return "";
        }
        //public static bool IsTaskScheduleExecuted()
        //{
            //try
            //{
            //    using (var process = Process.GetCurrentProcess())
            //    using (var parent = GetParentProcess(process.Id))
            //    {
            //        if (parent == null) return true; // No parent process found, assume manual execution.

            //        string parentName = parent.ProcessName.ToLower();

            //        // If parent is "taskeng" or "svchost", it's likely from Task Scheduler.
            //        return (parentName.Contains("taskeng") || parentName.Contains("svchost"));
            //    }
            //}
            //catch
            //{
            //    return false; // If there's an error retrieving the parent process, assume manual execution.
            //}
        //}

        //private static Process GetParentProcess(int processId)
        //{
        //    try
        //    {
        //        using (var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}"))
        //        {
        //            var results = query.Get().OfType<ManagementObject>().FirstOrDefault();
        //            if (results != null)
        //            {
        //                int parentId = Convert.ToInt32(results["ParentProcessId"]);
        //                return Process.GetProcessById(parentId);
        //            }
        //        }
        //    }
        //    catch { }

        //    return null;
        //}

        public static bool CheckDockerIsRunning()
        {
            bool isDockerRunning = Process.GetProcessesByName("com.docker.backend").Any();

            if (isDockerRunning)
            {
                Log.S(TAG, "Docker Desktop is running.");
                return true;
            }
            else
            {
                Console.WriteLine("\nDocker Desktop is not running.");
                return false;
            }
        }
        public static bool CheckIsDockerAutoRestart()
        {
            string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Docker", "settings-store.json");

            if (File.Exists(settingsFilePath))
            {
                string jsonContent = File.ReadAllText(settingsFilePath);
                JObject settings = JObject.Parse(jsonContent);

                bool autoStart = settings["AutoStart"]?.ToObject<bool>() ?? false;

                if (autoStart)
                {
                    Log.S(TAG, "Docker Desktop is set to start on login.");
                    return true;
                }
                else
                {
                    Log.S(TAG, "Docker Desktop is NOT set to start on login.");
                    return false;
                }
            }
            else
            {
                Log.E(TAG, "Docker settings file not found.");
                return true;
            }
        }
        //public static async Task IsTaskScheduledAsync(string taskName)
        //{
        //string checkTaskCommand = $"schtasks /query /tn \"{taskName}\"";

        //string result = await CommandExecutor.Instance.RunCommandAsAdminReturnStringWithoutPrintingAsync(checkTaskCommand, "Checking if Auto Detect IP task exists");
        //try
        //{
        //    if (!result.Contains(taskName, StringComparison.OrdinalIgnoreCase))// If result is not empty, the task exists
        //    {
        //        string exePath = Process.GetCurrentProcess().MainModule.FileName;

        //        string cmdCommand = $"schtasks /create /tn \"{taskName}\" /tr \"{exePath}\" /sc onlogon /rl highest /f";

        //        await CommandExecutor.Instance.RunCommandAsAdminAsync(cmdCommand, "Creating Windows Task Scheduler Task for Auto Detect IP");


        //        Log.I(TAG, $"Task Scheduler '{taskName}' has been created.");
        //    }
        //    else
        //    {
        //        Log.I(TAG, $"Task Scheduler '{taskName}' is already scheduled.");
        //        return;
        //    }

        //}
        //catch (Exception ex)
        //{
        //    Log.E(TAG, $"{ex.Message}");
        //}
        //}


        //public static async Task DeleteAutoDetectIPTaskAsync(string taskName)
        //{
        //await CommandExecutor.Instance.RunCommandAsAdminAsync($"powershell -Command \"Unregister-ScheduledTask -TaskName '{taskName}' -Confirm:$false\"", "Removing Windows Task Scheduler Task for Auto Detect IP");

        //Console.WriteLine($"Task '{taskName}' has been deleted.");
        //}

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>bool</returns>
        public static bool CheckFileExist(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Log.I(TAG, "Folder already exists.");
                return true;
            }
            else
            {
                Log.I(TAG, "Folder is not exist.");
                return false;
            }
        }

    }
}
