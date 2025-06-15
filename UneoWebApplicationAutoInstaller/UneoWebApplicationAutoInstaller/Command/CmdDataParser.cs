using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using UneoWebApplicationAutoInstaller.Models;

namespace UneoWebApplicationAutoInstaller.Command
{
    public class CmdDataParser
    {
        private DelegateProgressResult? delegateProgressResult = null;
        public CmdDataParser() { }
        public void SetDelegateProgressResult(DelegateProgressResult del)
        {
            delegateProgressResult = del;
        }
        public void DataParser(Dictionary<int, ObservableCollection<Setting>> installationData)
        {

        }
    }
}
