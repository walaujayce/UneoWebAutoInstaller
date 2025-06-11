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

    }
}
