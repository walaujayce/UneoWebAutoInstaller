using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class ProgressBarItem : ModelBase
    {
        public SolidColorBrush ProgressBarColor { get; set; } = new SolidColorBrush(Colors.LightGreen);

    }
}
