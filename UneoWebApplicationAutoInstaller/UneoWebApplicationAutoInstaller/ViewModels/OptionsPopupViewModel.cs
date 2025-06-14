using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class OptionsPopupViewModel : ViewModelBase
    {
        private ObservableCollection<Option> _optionsList;
        public ObservableCollection<Option> OptionsList
        {
            get
            {
                return _optionsList;
            }
            set
            {
                _optionsList = value;
                OnPropertyChanged(nameof(OptionsList));
            }
        }
        private Option _optionSelected;
        public Option OptionSelected
        {
            get
            {
                return _optionSelected;
            }
            set
            {
                _optionSelected = value;
                Debug.WriteLine("Selected: " + _optionSelected.OptionName);
                OnPropertyChanged(nameof(OptionSelected));
            }
        }
        public OptionsPopupViewModel()
        {
            _optionsList = new ObservableCollection<Option>();
        }
        public void Init()
        {
            _optionsList.Clear();
            Dictionary<string, string>? tempIPList = PublicFunction.GetAllInterfaceNameAndIP();
            if (tempIPList != null)
            {
                foreach (var item in tempIPList)
                {
                    _optionsList.Add(new Option() { OptionName = item.Value});
                }
            }
            else
            {
                _optionsList.Add(new Option());
            }
        }
    }
}
