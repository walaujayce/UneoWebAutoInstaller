using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using static UneoWebApplicationAutoInstaller.ViewModels.ConfigurationViewModel;

namespace UneoWebApplicationAutoInstaller.Host
{
    public class LibUSB
    {
        private UsbDevice usbDevice;
        private UsbEndpointReader epReader;
        private UsbEndpointWriter epWriter;
        private Thread readThread;
        private ErrorCode ec;
        private CancellationTokenSource _cts;
        private static LibUSB _instance;
        private static readonly object _lock = new object();

        public DelegateLibUSBData? delegateLibUSBData;

        public DelegateLibUSBStatus? delegateLibUSBStatus;
        public LibUSB()
        {

        }
        public static LibUSB Instance
        {
            get
            {
                // Ensure thread safety
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LibUSB();
                    }
                }
                return _instance;
            }
        }
        public void SetDelegate(DelegateLibUSBData callback)
        {
            delegateLibUSBData = callback;
        }
        public void SetDelegate(DelegateLibUSBStatus callback)
        {
            delegateLibUSBStatus = callback;
        }
        public void Open(int vid, int pid)
        {
            UsbDeviceFinder usbFinder = new UsbDeviceFinder(vid, pid);
            usbDevice = UsbDevice.OpenUsbDevice(usbFinder);

            if (usbDevice == null)
            {
                // 多次尝试连接USB设备
                int count = 0;
                while (count < 10 && usbDevice == null)
                {
                    Thread.Sleep(100);
                    usbDevice = UsbDevice.OpenUsbDevice(usbFinder);
                    count++;
                }
                Debug.WriteLine("打開USB設備失敗");

            }

            if (usbDevice != null)
            {
                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUSBDevice = usbDevice as IUsbDevice;
                if (wholeUSBDevice != null)
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUSBDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUSBDevice.ClaimInterface(0);

                    // Set connection status true
                    delegateLibUSBStatus?.Invoke(true);
                }

                // open read endpoint 1.
                epReader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                //epReader.ReadBufferSize = 2160;

                // open write endpoint 1.
                epWriter = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                _cts = new CancellationTokenSource();
                readThread = new Thread(SPI_Read);
                readThread.IsBackground = true;
                readThread.Start();
            }
        }
        public void Close()
        {
            try
            {
                _cts.Cancel();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }

        }

        public bool IsOpen()
        {
            if (usbDevice == null)
            {
                return false;
            }

            return usbDevice.IsOpen;
        }
        public void singleWrite(byte[] writeBuf)
        {
            if (epWriter == null)
            {
                return;
            }
            ec = epWriter.Write(writeBuf, 1000, out int byteWrite);
            Debug.WriteLine("Write data: " + string.Join(" ", writeBuf));
            if (ec != ErrorCode.None)
            {
                Debug.WriteLine($"Write Error, {UsbDevice.LastErrorString}");
            }
        }
        private void SPI_Read()
        {
            byte[] header = new byte[] { 0x55, 0x55 };
            int headerLength = header.Length;

            byte[] readBuffer = new byte[128];
            byte[] buffer;

            ec = new ErrorCode();

            while ((ec == ErrorCode.None || ec == ErrorCode.IoTimedOut) && !_cts.IsCancellationRequested)
            {
                if (epReader == null) return;
                // If the device hasn't sent data in the last 1000 milliseconds,
                // a timeout error (ec = IoTimedOut) will occur. 
                ec = epReader.Read(readBuffer, 1000, out int byteRead);
                Debug.WriteLine($"byte read count: {byteRead}");
                // 只有当超时的时候才会有byteRead为0，也就是结束
                if (byteRead != 0)
                {
                    buffer = new byte[byteRead];
                    Array.Copy(readBuffer, buffer, byteRead);
                    //Debug.WriteLine("Read data: " + string.Join(" ", buffer));
                    delegateLibUSBData?.Invoke(buffer);
                    Array.Clear(buffer, 0, byteRead);
                }
                else
                {
                    Debug.WriteLine("讀取數據結束" + ec);
                    // error code win32 error means disconnection
                    if (ec.Equals(ErrorCode.Win32Error))
                    {
                        delegateLibUSBStatus?.Invoke(false);
                    }                    
                }
            }
        }
    }
}
