using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class Config
    {
        public static string Version { get; set; } = "V1.0.0";
        public static bool EnableDockerCheck { get; set; } = false;
        public static bool IsCheckWebAPIContainerRunning { get; set; } = false;
    }
}
