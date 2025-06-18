using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class Enums
    {
        public enum ENavigatePage
        {
            ProcessSelectionPage = 0,
            InstallProcessPage = 1,
            UpdateProcessPage = 2,
            UninstallPage = 3,
            ProgressMonitorPage = 4,
        }
        public enum ESettingType
        {
            SingleInput = 0,
            MultipleInput = 1,
            MultipleKeyValue = 2,
            MultipleKeyValueWithReference = 3,
        }
        public enum EInstallID
        {
            PostgreSQLDatabase = 0,
            WebAPI = 1,
            UMonitorSocketServer = 2,
            Website = 3,
            UMonitorService = 4,            
        }
        public enum EInstallStatus
        {
            Fail = 0,
            Pass = 1,
            Ongoing = 2,
            Pending = 3,
            Warning = 4,
        }
    }
}
