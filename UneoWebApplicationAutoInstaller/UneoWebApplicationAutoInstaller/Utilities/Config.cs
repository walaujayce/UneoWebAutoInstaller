using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class Config
    {
        public static string Version { get; set; } = "V0.0.1";
        public static bool EnableDockerCheck { get; set; } = true;
        public static bool IsCheckWebAPIContainerRunning { get; set; } = true;
    }
}
