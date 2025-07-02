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
        private string _dictionaryKey = "";
        private object _dictionaryValue = "";
        private string _addBtnImageSource = "/Views/Assets/Icon_Add_FFD3D3D3.png";
        private bool _isRemovable = false;
        private int _inputType;
        private bool _isChecked = true;
        private string _checkboxImage = "/Views/Assets/Icon_Tick.png";

        public string DictionaryKey
        {
            get => _dictionaryKey;
            set
            {
                _dictionaryKey = value;
                OnPropertyChanged(nameof(DictionaryKey));
            }
        }

        public object DictionaryValue
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
        public int InputType
        {
            get => _inputType;
            set
            {
                _inputType = value;
                OnPropertyChanged(nameof(InputType));
            }
        }
        public string CheckboxImage
        {
            get => _checkboxImage;
            set
            {
                _checkboxImage = value;
                OnPropertyChanged("CheckboxImage");
            }
        }
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                if (_isChecked)
                {
                    CheckboxImage = "/Views/Assets/Icon_Tick.png";
                }
                else
                {
                    CheckboxImage = "";
                }
                OnPropertyChanged("IsChecked");
            }
        }
    }
}
