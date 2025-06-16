using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using UneoWebApplicationAutoInstaller.Command;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using UneoWebApplicationAutoInstaller.ViewModels;

namespace UneoWebApplicationAutoInstaller.Tests.CommandTests
{
    public class CmdDataParserTests
    {
        private readonly CmdDataParser _cmdDataParser;
        private readonly InstallProcessViewModel _installProcessVM;
        private ObservableCollection<Install> _installSelection;

        public CmdDataParserTests() 
        { 
            _cmdDataParser = new CmdDataParser();  
            _installProcessVM = new InstallProcessViewModel();
        }

        [Theory]
        [InlineData(true, 0)]
        public void DataParser_Return(bool isFilter, int filterID)
        {
            _installSelection = _installProcessVM.InstallSelection;
            Dictionary<int, List<Setting>> SettingList = new Dictionary<int, List<Setting>>();
            foreach (var item in _installSelection)
            {
                if (isFilter && item.InstallID != filterID) continue;
                SettingList[item.InstallID] = item.SettingList.ToList();
            }

            _= _cmdDataParser.DataParser(SettingList);

            bool result = _cmdDataParser.IsTestSuccess;
            result.Should().BeTrue(); 

        }

    }
}
