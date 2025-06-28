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
            DiagnosticPage = 3,
            ProgressMonitorPage = 4,
            ConfigurationPage = 5,
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
        public enum EProgressStatus
        {
            Fail = 0,
            Pass = 1,
            Ongoing = 2,
            Pending = 3,
            Warning = 4,
        }
        public enum EUpdateID
        {
            Website_CONTAINER = 0,
            Website_IMAGE = 1,
            UMonitorSocketServer_APPSETTINGS = 2,
            UMonitorSocketServer_ALL = 3,
            WebAPI_CONTAINER = 4,
            WebAPI_IMAGE = 5,
            UMonitorService_CONTAINER = 6,
            UMonitorService_IMAGE = 7,
        }
        public enum EProcessMode
        { 
            Install = 0,
            Update = 1,
            Diagnostic = 2,
            Configuration = 3,
        }
        public enum EUcbConfigurationMode
        {
            LibUSB = 0,
            FT4222h = 1,
            
        }
    }
}
