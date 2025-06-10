using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Models
{
    public class DictionaryInput : ModelBase
    {
        private string _dictionaryKey = "Key";
        private string _dictionaryValue = "Value";
        private string _addBtnImageSource = "/Views/Assets/Icon_Add_FFD3D3D3.png";
        private bool _isRemovable = false;

        public string DictionaryKey
        {
            get => _dictionaryKey;
            set
            {
                _dictionaryKey = value;
                OnPropertyChanged(nameof(DictionaryKey));
            }
        }

        public string DictionaryValue
        {
            get => _dictionaryValue;
            set
            {
                _dictionaryValue = value;
                OnPropertyChanged(nameof(DictionaryValue));
            }
        }

        public string AddBtnImageSource
        {
            get => _addBtnImageSource;
            set
            {
                _addBtnImageSource = value;
                OnPropertyChanged(nameof(AddBtnImageSource));
            }
        }
        public bool IsRemovable
        {
            get => _isRemovable;
            set
            {
                _isRemovable = value;
                if (_isRemovable) _addBtnImageSource = "/Views/Assets/Icon_Remove_FFD3D3D3.png";
                OnPropertyChanged(nameof(IsRemovable));
            }
        }
    }
}
