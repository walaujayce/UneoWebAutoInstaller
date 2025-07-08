using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UneoWebApplicationAutoInstaller.Models;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using ProcessOrigin = System.Diagnostics.Process;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    public class InformationCenterViewModel : ViewModelBase
    {
        #region DELEGATE METHODS

        private DelegateOverlayShow? delegateOverlayShow = null;
        public void SetVMDelegateOverlayShow(DelegateOverlayShow del)
        {
            this.delegateOverlayShow = del;
        }
        #endregion

        private ObservableCollection<Information> _informationList = new();
        public ObservableCollection<Information> InformationList
        {
            get
            {
                return _informationList;
            }
            set
            {
                _informationList = value;
                OnPropertyChanged(nameof(InformationList));
            }
        }
        private string _informationTitle = "";
        public string InformationTitle
        {
            get
            {
                return _informationTitle;
            }
            set
            {
                _informationTitle = value;
                OnPropertyChanged(nameof(InformationTitle));
            }
        }
        public InformationCenterViewModel()
        {            
        }
        public void DeInit()
        {
            delegateOverlayShow?.Invoke(false);
        }
        public void InformationListener(string title, List<string> informationList)
        {
            InformationTitle = title;

            InformationList.Clear();
            int index = 1;
            foreach (var information in informationList)
            {
                InformationList.Add(new Information()
                {
                    InformationNumber = index,
                    InformationText = information
                });
                index++;
            }
        }
        public async void DownloadFile()
        {
            List<DictionaryInput> dictionaryInputs = new List<DictionaryInput>();
            string? downloadUrl;
            try
            {
                dictionaryInputs = PublicFunction.LoadJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")).ToList();
                DictionaryInput temp = dictionaryInputs.First(d => d.DictionaryKey == "LibUsbDownloadUrl");
                downloadUrl = temp.DictionaryValue.ToString();
                if (downloadUrl == null || downloadUrl == "") return;

                string downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string destinationFile = Path.Combine(downloadFolder, $"LibUsbDotNet_Setup(2.2.8).exe");
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMinutes(15)
                };
                var data = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(destinationFile, data);
                ProcessOrigin.Start(new ProcessStartInfo
                {
                    FileName = destinationFile,
                    UseShellExecute = true
                });
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString()); 
            }
        }
    }
}
