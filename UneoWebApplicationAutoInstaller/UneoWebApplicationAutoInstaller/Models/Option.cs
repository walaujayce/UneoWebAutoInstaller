using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Option : ModelBase
    {
        private string _optionName { get; set; } = "";
        public string OptionName
        {
            get
            {
                return _optionName;
            }
            set
            {
                _optionName = value;
                OnPropertyChanged(nameof(OptionName));
            }
        }
    }
}
