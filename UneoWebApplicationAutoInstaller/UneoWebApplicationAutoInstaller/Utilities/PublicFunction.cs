using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UneoWebApplicationAutoInstaller.Models;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class PublicFunction
    {
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
                    AppSettingsList.Clear();
                    Dictionary<string, object> AppSettingsJSON = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    foreach(var item in AppSettingsJSON)
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
        public static List<string> EnumerateInstallToDoList(string installationName)
        {   
            List<string> list = new();
            switch (installationName)
            {
                case "PostgreSQL Database":
                    list = new List<string>()
                    {
                        "Set up local database file",
                        "Pull PostgreSQL image \"bitnami/postgresql:latest\"",
                        "Containerize PostgreSQL image",
                        "Create \"uneo_web\" database",
                        "Check PostgreSQL container is running"
                    };
                    break;
                case "WebAPI":
                    list = new List<string>()
                    {
                        "Pull WebAPI image \"uneotw/umonitorwebapi:latest\"",
                        "Containerize WebAPI image",
                        "Check WebAPI container is running"
                    };
                    break;
                case "Website":
                    list = new List<string>()
                    {
                        "Pull Website image \"p2211/uext-v1:latest\"",
                        "Containerize Website image",
                        "Check Website container is running"
                    };
                    break;
                case "UMonitorSocketServer":
                    list = new List<string>()
                    {
                        "Run UMonitorSocketServer",
                        "Check UMonitorSocketServer is running"
                    };
                    break;
                case "UMonitorService":
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

    }
}
