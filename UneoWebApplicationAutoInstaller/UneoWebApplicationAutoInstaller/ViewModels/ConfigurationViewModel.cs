using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using UneoWebApplicationAutoInstaller.Host;
using UneoWebApplicationAutoInstaller.Models;
using static UneoWebApplicationAutoInstaller.Utilities.Enums;
using UneoWebApplicationAutoInstaller.Utilities;
using static UneoWebApplicationAutoInstaller.ViewModels.MainWindowViewModel;
using UneoWebApplicationAutoInstaller.Views;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.CodeDom.Compiler;
using System.IO;
using System.Security.Cryptography;

namespace UneoWebApplicationAutoInstaller.ViewModels
{
    //Region DataTemplate Selector
    #region SETTING TEMPLATE SELECTOR
    public class InputTypeTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate CheckBoxTemplate { get; set; }
        public required DataTemplate ComboBoxTemplate { get; set; }
        public required DataTemplate InputBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DictionaryInput dictionaryInput)
            {
                switch (dictionaryInput.InputType)
                {
                    case 0:
                        return CheckBoxTemplate;
                    case 1:
                        return ComboBoxTemplate;
                    case 2:
                        return InputBoxTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
    #endregion
    public class ConfigurationViewModel : ViewModelBase
    {       
        #region GET SET
        private SolidColorBrush _libUsbForeground = new SolidColorBrush(Colors.White);
        public SolidColorBrush LibUsbForeground
        {
            get { return _libUsbForeground; }
            set
            {
                _libUsbForeground = value;
                OnPropertyChanged(nameof(LibUsbForeground));
            }
        }
        private SolidColorBrush _ft4222hForeground = new SolidColorBrush(Colors.Black);
        public SolidColorBrush Ft4222hForeground
        {
            get { return _ft4222hForeground; }
            set
            {
                _ft4222hForeground = value;
                OnPropertyChanged(nameof(Ft4222hForeground));
            }
        }
        private int _configurationMode = (int)EUcbConfigurationMode.LibUSB;
        public int ConfigurationMode
        {
            get { return _configurationMode; }
            set
            {
                _configurationMode = value;
                switch (_configurationMode)
                {
                    case (int)EUcbConfigurationMode.LibUSB:
                        LibUsbForeground = new SolidColorBrush(Colors.White);
                        Ft4222hForeground = new SolidColorBrush(Colors.Black);
                        break;
                    case (int)EUcbConfigurationMode.FT4222h:
                        LibUsbForeground = new SolidColorBrush(Colors.Black);
                        Ft4222hForeground = new SolidColorBrush(Colors.White);
                        break;
                }
                OnPropertyChanged(nameof(ConfigurationMode));
            }
        }
        private ObservableCollection<WifiSetting> wifiSettingsList;
        public ObservableCollection<WifiSetting> WifiSettingsList
        {
            get { return wifiSettingsList; }
            set
            {
                wifiSettingsList = value;
                OnPropertyChanged(nameof(WifiSettingsList));
            }
        }
        private DictionaryInput selectedInputBox;
        public DictionaryInput InputBoxSelected
        {
            get { return selectedInputBox; }
            set
            {
                selectedInputBox = value;
                OnPropertyChanged(nameof(InputBoxSelected));
            }
        }
        private ObservableCollection<DictionaryInput> ipAddressList;
        public ObservableCollection<DictionaryInput> IpAddressList
        {
            get { return ipAddressList; }
            set
            {
                ipAddressList = value;
                OnPropertyChanged(nameof(IpAddressList));
            }
        }
        private string _connectedWifiName;
        public string ConnectedWifiName
        {
            get { return _connectedWifiName; }
            set
            {
                _connectedWifiName = value;
                OnPropertyChanged(nameof(ConnectedWifiName));
            }
        }
        private string _connectedWifiPassword;
        public string ConnectedWifiPassword
        {
            get { return _connectedWifiPassword; }
            set
            {
                _connectedWifiPassword = value;
                OnPropertyChanged(nameof(ConnectedWifiPassword));
            }
        }
        private DictionaryInput _ipAddressSelected;
        public DictionaryInput IpAddressSelected
        {
            get { return _ipAddressSelected; }
            set
            {
                _ipAddressSelected = value;
                OnPropertyChanged(nameof(IpAddressSelected));
            }
        }
        private string _connectStatusImage = "/Views/Assets/Icon_RedCircle.png";
        public string ConnectStatusImage
        {
            get { return _connectStatusImage; }
            set
            {
                _connectStatusImage = value;
                OnPropertyChanged(nameof(ConnectStatusImage));
            }
        }
        private string _connectStatusText = "Offline"; 
        public string ConnectStatusText
        {
            get { return _connectStatusText; }
            set
            {
                _connectStatusText = value;
                OnPropertyChanged(nameof(ConnectStatusText));
            }
        }
        private double _connectStatusOpacity = 0.5;
        public double ConnectStatusOpacity
        {
            get { return _connectStatusOpacity; }
            set
            {
                _connectStatusOpacity = value;
                OnPropertyChanged(nameof(ConnectStatusOpacity));
            }
        }
        private string _statusMessageText = ""; 
        public string StatusMessageText
        {
            get { return _statusMessageText; }
            set
            {
                _statusMessageText = value;
                OnPropertyChanged(nameof(StatusMessageText));
            }
        }
        private string _saveTemplateImage = "/Views/Assets/Icon_Save.png";
        public string SaveTemplateImage
        {
            get { return _saveTemplateImage; }
            set
            {
                _saveTemplateImage = value;
                OnPropertyChanged(nameof(SaveTemplateImage));
            }            
        }
        private bool _isWriteTemplateBtnEnabled = false;
        public bool IsWriteTemplateBtnEnabled
        {
            get { return _isWriteTemplateBtnEnabled; }
            set
            {
                _isWriteTemplateBtnEnabled = value;
                OnPropertyChanged(nameof(IsWriteTemplateBtnEnabled));
            }
        }
        private double _writeTemplateImageOpacity = 0.5;
        public double WriteTemplateImageOpacity
        {
            get { return _writeTemplateImageOpacity; }
            set
            {
                _writeTemplateImageOpacity = value;
                OnPropertyChanged(nameof(WriteTemplateImageOpacity));
            }
        }
        private string _saveTemplateText = "";
        public string SaveTemplateText
        {
            get { return _saveTemplateText; }
            set
            {
                _saveTemplateText = value;
                OnPropertyChanged(nameof(SaveTemplateText));
            }            
        }
        private Visibility _saveTemplateVisibility = Visibility.Hidden;
        public Visibility SaveTemplateVisibility
        {
            get { return _saveTemplateVisibility; }
            set
            {
                _saveTemplateVisibility = value;
                OnPropertyChanged(nameof(SaveTemplateVisibility));
            }
        }
        
        #endregion

        private bool _isConnecting = false;
        private bool isSkipReadWifiValue = false;

        private List<string> informationList;

        public ConfigurationViewModel()
        {
            IpAddressList = new ObservableCollection<DictionaryInput>();
            GetIpAddressList();
            GetWifiNameAndPassword();

            WifiSettingsList = new ObservableCollection<WifiSetting>()
            {
                new WifiSetting()
                {
                    SettingID = (int)EWifiSettingID.WirelessAP,
                    SettingTitle = "Wireless AP Configuration",
                    SettingList =
                    {
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.CheckBox,
                            DictionaryKey = "DHCP",
                            IsChecked = WifiSetting.DHCP,
                            DictionaryValue = WifiSetting.DHCP == true ? "1" : "0"

                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.ComboBox,
                            DictionaryKey = "Security",
                            DictionaryValue = WifiSetting.Security
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "SSID",
                            DictionaryValue = WifiSetting.SSID
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "PSK",
                            DictionaryValue = WifiSetting.PSK
                        },
                    }
                },
                new WifiSetting()
                {
                    SettingID = (int)EWifiSettingID.LAN,
                    SettingTitle = "LAN Configuration",
                    SettingList =
                    {
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "MAC",
                            DictionaryValue = WifiSetting.MAC
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "UCB IP",
                            DictionaryValue = WifiSetting.UCB_IP
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "Netmask",
                            DictionaryValue = WifiSetting.Netmask
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "Gateway",
                            DictionaryValue = WifiSetting.Gateway
                        },
                    }
                },
                new WifiSetting()
                {
                    SettingID = (int)EWifiSettingID.SERVER,
                    SettingTitle = "SERVER Configuration",
                    SettingList =
                    {
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "Server IP",
                            DictionaryValue = WifiSetting.Server_IP
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "Port",
                            DictionaryValue = WifiSetting.PORT
                        },
                        new DictionaryInput()
                        {
                            InputType = (int)EWifiInputType.InputBox,
                            DictionaryKey = "RSSI",
                            DictionaryValue = WifiSetting.RSSI
                        },
                    }
                },
            };
            informationList = new List<string>
            {
                "If you have installed LibUsbDotNet_Setup.2.2.8, skip the following steps.",
                "Plug in USB to PC and UCB, execute LibUsbDotNet_Setup.2.2.8.",
                "After \"libusb-win32 filter installer\" program show up, go \"Install a device filter\", then \"Next\"",
                "Select [Hardware ID] VID : 0F5A PID:0001 , REV:0200, then \"Install\"."
            };
        }
        //////////////////////////////////////// CONNECT with LibUSB ////////////////////////////////////////
        private LibUSB? libUSB = null;
        public delegate void DelegateLibUSBData(byte[] data);
        public delegate void DelegateLibUSBStatus(bool isConnected);
        public async void ConnectAndReadLibUSB()
        {         
            isSkipReadWifiValue = false;

            // if not connecting then connect
            if (!_isConnecting)
            {
                await ConnectLibUSB();
            }
            // if connecting then read only
            else
            {
                ReadWiFiConfigLibUSBAsync();
            }
        }
        private async Task ConnectLibUSB()
        {
            try
            {
                UsbRegDeviceList allDevices = UsbDevice.AllDevices;
                Debug.WriteLine("Found {0} devices", allDevices.Count);
                foreach (UsbRegistry usb in allDevices)
                {
                    Debug.WriteLine("----------------");
                    Debug.WriteLine($"PID: {usb.Pid}, VID: {usb.Vid}");

                    if (usb.Device.DriverMode == UsbDevice.DriverModeType.LibUsb && usb.Pid == 1 && usb.Vid == 3930)
                    {
                        libUSB = LibUSB.Instance;
                        libUSB.SetDelegate(new DelegateLibUSBData(LibUSBDataListener));
                        libUSB.SetDelegate(new DelegateLibUSBStatus(LibUSBStatusListener));
                        libUSB.Open(0x0F5A, 0x0001);
                    }
                }
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void LibUSBDataListener(byte[] data)
        {
            try
            {
                Debug.WriteLine("Read data: " + BitConverter.ToString(data).Replace("-", " "));
                if (data[2] == 0x02 && data[3] == 0xCD)
                {
                    ProcessWiFiConfig(data);
                }                
                else if (data[2] == 0x82 && data[3] == 0xCD)
                {
                    Debug.WriteLine("UCB response CD82");
                }
                else if (data[2] == 0xA5 && data[3] == 0x5A)
                {
                    if (data[6] == 0x01)
                    {
                        Debug.WriteLine("WIFI CONFIG PASS");
                        //MessageBox.Show("WIFI CONFIG PASS", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);

                        // async the status message 
                        StatusMessageText = "Write successfully.";
                    }
                    else if (data[6] == 0x00)
                    {
                        Debug.WriteLine("WIFI CONFIG FAIL");
                        //MessageBox.Show("WIFI CONFIG FAIL", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);

                        // async the status message 
                        StatusMessageText = "Failed to write.";

                    }
                }
                else
                {
                    Debug.WriteLine("No classified read data: " + string.Join(" ", data));
                }
            }
            catch (Exception ex) { }
        }

        private void LibUSBStatusListener(bool isConnected)
        {
            if (isConnected)
            {
                _isConnecting = true;
                ReadWiFiConfigLibUSBAsync();

                // async the status message 
                ConnectStatusImage = "/Views/Assets/Icon_GreenCircle.png";
                ConnectStatusText = "Online";
                StatusMessageText = "Connected successfully.";
                ConnectStatusOpacity = 1;
            }
            else
            {
                _isConnecting = false;
                libUSB?.Close();
                libUSB = null;

                // async the status message 
                ConnectStatusImage = "/Views/Assets/Icon_RedCircle.png";
                ConnectStatusText = "Offline";
                StatusMessageText = string.Empty;
                ConnectStatusOpacity = 0.5;
            }
        }
        //////////////////////////////////////// WIFI READ ////////////////////////////////////////
        public async void ReadWiFiConfigLibUSBAsync()
        {
            // if not connecting and write button click directly, the latest values in input box will be replaced by default value in UCB
            if (isSkipReadWifiValue) return;
            await Task.Delay(100);
            await Task.Run(() =>
            {
                libUSB?.singleWrite(HostCmds.ReadWifiConfig());
            });

        }
        public void ProcessWiFiConfig(byte[] dataSegment)
        {
            if (dataSegment.Length >= 100 && dataSegment[2] == 0x02 && dataSegment[3] == 0xCD)
            {
                // Wireless AP Configuration
                byte dhcp = dataSegment[8];
                byte security = dataSegment[9];
                string ssid = Encoding.ASCII.GetString(dataSegment, 10, 32).Trim().Replace("\0", "").Replace("\r", "").Replace("\n", "");
                string psk = Encoding.ASCII.GetString(dataSegment, 42, 32).Trim().Replace("\0", "").Replace("\r", "").Replace("\n", "");

                Debug.WriteLine($"DHCP: {dhcp}");
                Debug.WriteLine($"Security: {security}");
                Debug.WriteLine($"SSID: {ssid}");
                Debug.WriteLine($"PSK: {psk}");

                var wirelessApConfiguration = WifiSettingsList.FirstOrDefault(x => x.SettingID == (int)EWifiSettingID.WirelessAP);
                if (wirelessApConfiguration != null)
                {
                    foreach (var item in wirelessApConfiguration.SettingList)
                    {
                        switch (item.DictionaryKey)
                        {
                            case "DHCP":
                                item.IsChecked = dhcp == 1;
                                break;
                            case "Security":
                                item.DictionaryValue = security.ToString();
                                break;
                            case "SSID":
                                item.DictionaryValue = ssid;
                                break;
                            case "PSK":
                                item.DictionaryValue = psk;
                                break;
                        }
                    }
                }

                // LAN Configuration
                string mac = BitConverter.ToString(dataSegment, 74, 6).Replace("-", ":");
                string local = string.Join(".", dataSegment.Skip(80).Take(4));
                string netmask = string.Join(".", dataSegment.Skip(84).Take(4));
                string gateway = string.Join(".", dataSegment.Skip(88).Take(4));

                Debug.WriteLine($"MAC: {mac}");
                Debug.WriteLine($"Local: {local}");
                Debug.WriteLine($"Netmask: {netmask}");
                Debug.WriteLine($"Gateway: {gateway}");

                var UcbConfiguration = WifiSettingsList.FirstOrDefault(x => x.SettingID == (int)EWifiSettingID.LAN);
                if (UcbConfiguration != null)
                {
                    foreach (var item in UcbConfiguration.SettingList)
                    {
                        switch (item.DictionaryKey)
                        {
                            case "MAC":
                                item.DictionaryValue = mac;
                                break;
                            case "UCB IP":
                                item.DictionaryValue = local;
                                break;
                            case "Netmask":
                                item.DictionaryValue = netmask;
                                break;
                            case "Gateway":
                                item.DictionaryValue = gateway;
                                break;
                        }
                    }
                }
                // Server Configuration
                string server = string.Join(".", dataSegment.Skip(92).Take(4));
                UInt16 port = BitConverter.ToUInt16(dataSegment, 96);
                UInt16 rssi = BitConverter.ToUInt16(dataSegment, 98);

                Debug.WriteLine($"Server: {server}");
                Debug.WriteLine($"Port: {port}");
                Debug.WriteLine($"RSSI: {rssi} dBm");

                var ServerConfiguration = WifiSettingsList.FirstOrDefault(x => x.SettingID == (int)EWifiSettingID.SERVER);
                if (ServerConfiguration != null)
                {
                    foreach (var item in ServerConfiguration.SettingList)
                    {
                        switch (item.DictionaryKey)
                        {
                            case "Server IP":
                                item.DictionaryValue = server;
                                break;
                            case "Port":
                                item.DictionaryValue = port;
                                break;
                            case "RSSI":
                                item.DictionaryValue = rssi;
                                break;
                        }
                    }
                }

                // async the status message 
                StatusMessageText = "Read successfully.";
            }
            else
            {
                Debug.WriteLine("Not enough data received to parse the payload.");
            }
        }
        //////////////////////////////////////// WIFI WRITE ////////////////////////////////////////
        public async Task WriteWiFiConfigAsync()
        {
            foreach(var item in WifiSettingsList)
            {
                List<DictionaryInput> settingList = item.SettingList.ToList();
                foreach(var value in settingList)
                {
                    Debug.WriteLine($"Key: {value.DictionaryKey} | Value: {value.DictionaryValue}");
                }
            }
            List<DictionaryInput> WirelessApSettingList;
            if (isWriteWithTemplate)
            {
                WirelessApSettingList = tempWifiSettingList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.WirelessAP).SettingList.ToList();
            }
            else
            {
                WirelessApSettingList = WifiSettingsList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.WirelessAP).SettingList.ToList();
            }
            foreach (var item in WirelessApSettingList)
            {
                switch (item.DictionaryKey)
                {
                    case "DHCP":
                        WifiSetting.DHCP = (bool)item.IsChecked;
                        break;
                    case "Security":
                        WifiSetting.Security = TryParseInt(item.DictionaryValue);
                        break;
                    case "SSID":
                        WifiSetting.SSID = (string)item.DictionaryValue;
                        break;
                    case "PSK":
                        WifiSetting.PSK = (string)item.DictionaryValue;
                        break;
                }
            }

            byte[] newLine = Encoding.ASCII.GetBytes("\r\n");
            byte[] ap = new byte[66];
            byte[] ssidArr = StringToBytes(WifiSetting.SSID);
            byte[] pskArr = StringToBytes(WifiSetting.PSK);
            ap[0] = Convert.ToByte(WifiSetting.DHCP);
            ap[1] = Convert.ToByte(WifiSetting.Security);
            Array.Copy(ssidArr, 0, ap, 2, ssidArr.Length);
            Array.Copy(newLine, 0, ap, 2 + ssidArr.Length, newLine.Length);
            Array.Copy(pskArr, 0, ap, 34, pskArr.Length);
            Array.Copy(newLine, 0, ap, 34 + pskArr.Length, newLine.Length);

            List<DictionaryInput> LanSettingList;
            if (isWriteWithTemplate)
            {
                LanSettingList = tempWifiSettingList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.LAN).SettingList.ToList();
            }
            else
            {
                LanSettingList = WifiSettingsList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.LAN).SettingList.ToList();
            }
            foreach (var item in LanSettingList)
            {
                switch (item.DictionaryKey)
                {
                    case "MAC":
                        WifiSetting.MAC = (string)item.DictionaryValue;
                        break;
                    case "UCB IP":
                        WifiSetting.UCB_IP = (string)item.DictionaryValue;
                        break;
                    case "Netmask":
                        WifiSetting.Netmask = (string)item.DictionaryValue;
                        break;
                    case "Gateway":
                        WifiSetting.Gateway = (string)item.DictionaryValue;
                        break;
                }
            }

            byte[] lan = new byte[18];
            byte[] macArr = MacAddressToBytes(WifiSetting.MAC);
            byte[] localArr = IPAddressToBytes(WifiSetting.UCB_IP);
            byte[] netMaskArr = IPAddressToBytes(WifiSetting.Netmask);
            byte[] gateWayArr = IPAddressToBytes(WifiSetting.Gateway);

            Array.Copy(macArr, 0, lan, 0, macArr.Length);
            Array.Copy(localArr, 0, lan, 6, localArr.Length);
            Array.Copy(netMaskArr, 0, lan, 10, netMaskArr.Length);
            Array.Copy(gateWayArr, 0, lan, 14, gateWayArr.Length);

            List<DictionaryInput> ServerSettingList;
            if (isWriteWithTemplate)
            {
                ServerSettingList = tempWifiSettingList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.SERVER).SettingList.ToList();
            }
            else
            {
                ServerSettingList = WifiSettingsList.FirstOrDefault(w => w.SettingID == (int)EWifiSettingID.SERVER).SettingList.ToList();
            }
            foreach (var item in ServerSettingList)
            {
                switch (item.DictionaryKey)
                {
                    case "Server IP":
                        WifiSetting.Server_IP = (string)item.DictionaryValue;
                        break;
                    case "Port":
                        WifiSetting.PORT = TryParseInt(item.DictionaryValue);
                        break;
                    case "RSSI":
                        WifiSetting.RSSI = TryParseInt(item.DictionaryValue);
                        break;
                }
            }
            byte[] server = new byte[8];
            byte[] ipArr = IPAddressToBytes(WifiSetting.Server_IP);
            byte[] portArr = UInt16ToBytes((ushort)WifiSetting.PORT);
            byte[] rssiArr = UInt16ToBytes((ushort)WifiSetting.RSSI);
            Array.Copy(ipArr, 0, server, 0, ipArr.Length);
            Array.Copy(portArr, 0, server, 4, portArr.Length);
            Array.Copy(rssiArr, 0, server, 6, rssiArr.Length);

            // 寫入拆兩包
            byte[] input = HostCmds.WriteWifiConfig(ap, lan, server);
            byte[] output1 = new byte[64]; // First 64 bytes
            byte[] output2 = new byte[input.Length - 64]; // Remaining bytes
            Array.Copy(input, 0, output1, 0, 64);
            Array.Copy(input, 64, output2, 0, input.Length - 64);

            await Task.Run(() =>
            {
                libUSB?.singleWrite(output1);
            });
            await Task.Run(() =>
            {
                libUSB?.singleWrite(output2);
            });
            await Task.Run(() =>
            {
                libUSB?.singleWrite(HostCmds.Read_Command_1());
            });

        }
        //////////////////////////////////////// Save Template ////////////////////////////////////////
        private List<WifiSetting> tempWifiSettingList = new List<WifiSetting>();
        private bool isWriteWithTemplate = false;
        public void SaveCurrentValueAsTemplate()
        {
            if (CheckWifiSettingListIsNullOrEmpty())
            {
                StatusMessageText = "One or more value is unavailable.";
                return;
            }
            tempWifiSettingList.Clear();
            foreach (var item in WifiSettingsList)
            {
                tempWifiSettingList.Add(CloneWifiSetting(item));
            }

            // async the status message 
            StatusMessageText = "Save current values as template.";
            SaveTemplateImage = "/Views/Assets/Icon_SaveOK.png";
            SaveTemplateVisibility = Visibility.Visible;
            IsWriteTemplateBtnEnabled = true;
            WriteTemplateImageOpacity = 1.0;

            string _saveTemplateText = "";
            foreach(var settingList in tempWifiSettingList)
            {
                foreach(var item in settingList.SettingList)
                {
                    _saveTemplateText += $"{item.DictionaryKey} : {item.DictionaryValue}\n";
                }
            }
            SaveTemplateText = _saveTemplateText;

        }
        //////////////////////////////////////// METHOD ////////////////////////////////////////
        private byte[] IPAddressToBytes(string ipString)
        {
            if (IPAddress.TryParse(ipString, out IPAddress ipAddress))
            {
                return ipAddress.GetAddressBytes();
            }
            else
            {
                throw new ArgumentException("Invalid IP address format", nameof(ipString));
            }
        }
        private byte[] MacAddressToBytes(string macString)
        {
            if (string.IsNullOrEmpty(macString))
            {
                throw new ArgumentException("MAC address cannot be null or empty", nameof(macString));
            }

            // Remove any possible separators and split by ':' or '-'
            string[] macParts = macString.Split(new[] { ':', '-' });

            if (macParts.Length != 6)
            {
                throw new ArgumentException("Invalid MAC address format", nameof(macString));
            }

            // Convert each part from hex string to byte
            return macParts.Select(part => Convert.ToByte(part, 16)).ToArray();
        }
        private byte[] StringToBytes(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            // Use UTF8 encoding to convert the string to bytes
            return Encoding.UTF8.GetBytes(str.Trim());
        }
        private byte[] UInt16ToBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }
        private int TryParseInt(object obj)
        {
            if (obj is string stringValue && int.TryParse(stringValue, out int value))
            {
                return value;
            }
            else if (obj is int intValue)
            {
                return intValue;
            }
            else
            {
                return 0; // fallback if parsing fails
            }
        }
        public void GetWifiNameAndPassword()
        {
            //Get Connected Wifi Name and Password
            var wifiNameAndPassword = PublicFunction.GetConnectedWifiNameAndPassword();
            if (wifiNameAndPassword != null)
            {
                ConnectedWifiName = wifiNameAndPassword["SSID"];
                ConnectedWifiPassword = wifiNameAndPassword["PASSWORD"];
            }
            else
            {
                ConnectedWifiName = "null";
                ConnectedWifiPassword = "null";
            }
        }
        public void GetIpAddressList()
        {
            Dictionary<string, string>? allInterfaceNameAndIP = PublicFunction.GetAllInterfaceNameAndIP();

            //Dictionary<string, string>? allInterfaceNameAndIP = new()
            //{
            //    {"Wifi", "188.88.20.102" },
            //    {"Ethernet", "192.9.120.145" },
            //    {"Local Ip and WIFI", "168.2.200.202" },
            //};

            IpAddressList.Clear();
            if (allInterfaceNameAndIP != null)
            {
                foreach (var ip in allInterfaceNameAndIP)
                {
                    IpAddressList.Add(new DictionaryInput()
                    {
                        DictionaryKey = ip.Key,
                        DictionaryValue = ip.Value
                    });
                }
            }
            else
            {
                IpAddressList.Add(new DictionaryInput()
                {
                    DictionaryKey = "null",
                    DictionaryValue = "null",
                });
            }
        }
        private WifiSetting CloneWifiSetting(WifiSetting original)
        {
            return new WifiSetting
            {
                SettingID = original.SettingID,
                SettingTitle = original.SettingTitle,
                SettingList = new ObservableCollection<DictionaryInput>(
                original.SettingList.Select(i => new DictionaryInput()
                {
                    DictionaryKey = i.DictionaryKey,
                    DictionaryValue = i.DictionaryValue,
                    IsChecked = i.IsChecked,
                    InputType = i.InputType,
                    CheckboxImage = i.CheckboxImage,    
                }))
            };
        }
        public async void WriteWiFiConfig()
        {
            if (CheckWifiSettingListIsNullOrEmpty()) return;

            isWriteWithTemplate = false;
            isSkipReadWifiValue = true;
            if (!_isConnecting)
            {
                await ConnectLibUSB();
            }
            await WriteWiFiConfigAsync();
        }
        public async void WriteWiFiConfigWithTemplate()
        {
            if (CheckWifiSettingListIsNullOrEmpty()) return;

            isWriteWithTemplate = true;

            if (!_isConnecting)
            {
                await ConnectLibUSB();
            }
            await WriteWiFiConfigAsync();

            // need to set read wifi true again
            isSkipReadWifiValue = false;
            ReadWiFiConfigLibUSBAsync();
            // async the status message 
            StatusMessageText = "Write successfully.";
        }
        private bool CheckWifiSettingListIsNullOrEmpty()
        {
            foreach(var item in WifiSettingsList)
            {
                foreach (var settingList in item.SettingList)
                {
                    if (string.IsNullOrEmpty(settingList.DictionaryValue.ToString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void PrintCurrentSelectedInputBoxValue()
        {
            Debug.WriteLine($"{InputBoxSelected.DictionaryKey} | {InputBoxSelected.DictionaryValue}");
        }
        public void DeInit()
        {
            LibUSBStatusListener(false);
            tempWifiSettingList.Clear();
            SaveTemplateImage = "/Views/Assets/Icon_Save.png";
            SaveTemplateVisibility = Visibility.Hidden;
            IsWriteTemplateBtnEnabled = false;
            WriteTemplateImageOpacity = 0.5;
        }

        private DelegateInformation? delegateInformation = null;
        public void SetVMDelegateInformation(DelegateInformation del)
        {
            this.delegateInformation = del;
        }
        public void ProceedToHelpCenter()
        {
            delegateInformation?.Invoke("Configuration Information", informationList);
        }
    }
}
