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
        }
        public enum EInstallID
        {
            PostgreSQL = 0,
            WebAPI = 1,
            UMonitorSocketServer = 2,
            Website = 3,
            UMonitorService = 4,
        }
    }
}
