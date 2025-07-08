using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class Information : ModelBase
    {
        private int _informationNumber;
        public int InformationNumber
        {
            get
            {
                return _informationNumber;
            }
            set
            {
                _informationNumber = value;
                OnPropertyChanged(nameof(InformationNumber));
            }
        }
        private string _informationText = "";
        public string InformationText
        {
            get
            {
                return _informationText;
            }
            set
            {
                _informationText = value;
                OnPropertyChanged(nameof(InformationText));
            }
        }
    }
}
