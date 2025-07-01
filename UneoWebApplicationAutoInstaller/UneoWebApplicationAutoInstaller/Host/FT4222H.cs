using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;
using static UneoWebApplicationAutoInstaller.ViewModels.ConfigurationViewModel;

namespace UneoWebApplicationAutoInstaller.Host
{
    public class FT4222H
    {
        //**************************************************************************
        //
        // FUNCTION IMPORTS FROM FTD2XX DLL
        //
        //**************************************************************************

        [DllImport("ftd2xx.dll")]
        static extern FTDI.FT_STATUS FT_CreateDeviceInfoList(ref UInt32 numdevs);

        [DllImport("ftd2xx.dll")]
        static extern FTDI.FT_STATUS FT_GetDeviceInfoDetail(UInt32 index, ref UInt32 flags, ref FTDI.FT_DEVICE chiptype, ref UInt32 id, ref UInt32 locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

        //[DllImportAttribute("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("ftd2xx.dll")]
        static extern FTDI.FT_STATUS FT_OpenEx(uint pvArg1, int dwFlags, ref IntPtr ftHandle);

        //[DllImportAttribute("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("ftd2xx.dll")]
        static extern FTDI.FT_STATUS FT_Close(IntPtr ftHandle);


        const byte FT_OPEN_BY_SERIAL_NUMBER = 1;
        const byte FT_OPEN_BY_DESCRIPTION = 2;
        const byte FT_OPEN_BY_LOCATION = 4;

        //**************************************************************************
        //
        // FUNCTION IMPORTS FROM LIBFT4222 DLL
        //
        //**************************************************************************

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SetClock(IntPtr ftHandle, FT4222_ClockRate clk);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_GetClock(IntPtr ftHandle, ref FT4222_ClockRate clk);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPIMaster_Init(IntPtr ftHandle, FT4222_SPIMode ioLine, FT4222_SPIClock clock, FT4222_SPICPOL cpol, FT4222_SPICPHA cpha, Byte ssoMap);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPI_SetDrivingStrength(IntPtr ftHandle, SPI_DrivingStrength clkStrength, SPI_DrivingStrength ioStrength, SPI_DrivingStrength ssoStregth);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPIMaster_SingleReadWrite(IntPtr ftHandle, ref byte readBuffer, ref byte writeBuffer, ushort bufferSize, ref ushort sizeTransferred, bool isEndTransaction);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_Init(IntPtr ftHandle);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_InitEx(IntPtr ftHandle, bool mode, ushort readTimeout, ushort writeTimeout);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_Read(IntPtr ftHandle, [Out] byte[] buffer, ushort bufferSize, ref ushort sizeOfRead);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_Write(IntPtr ftHandle, [In] byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_SetMode(IntPtr ftHandle, FT4222_SPICPOL cpol, FT4222_SPICPHA cpha);

        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT4222_STATUS FT4222_SPISlave_GetRxStatus(IntPtr ftHandle, ref ushort rxSize);


        // FT4222 Device status
        public enum FT4222_STATUS
        {
            FT4222_OK,
            FT4222_INVALID_HANDLE,
            FT4222_DEVICE_NOT_FOUND,
            FT4222_DEVICE_NOT_OPENED,
            FT4222_IO_ERROR,
            FT4222_INSUFFICIENT_RESOURCES,
            FT4222_INVALID_PARAMETER,
            FT4222_INVALID_BAUD_RATE,
            FT4222_DEVICE_NOT_OPENED_FOR_ERASE,
            FT4222_DEVICE_NOT_OPENED_FOR_WRITE,
            FT4222_FAILED_TO_WRITE_DEVICE,
            FT4222_EEPROM_READ_FAILED,
            FT4222_EEPROM_WRITE_FAILED,
            FT4222_EEPROM_ERASE_FAILED,
            FT4222_EEPROM_NOT_PRESENT,
            FT4222_EEPROM_NOT_PROGRAMMED,
            FT4222_INVALID_ARGS,
            FT4222_NOT_SUPPORTED,
            FT4222_OTHER_ERROR,
            FT4222_DEVICE_LIST_NOT_READY,

            FT4222_DEVICE_NOT_SUPPORTED = 1000,        // FT_STATUS extending message
            FT4222_CLK_NOT_SUPPORTED,     // spi master do not support 80MHz/CLK_2
            FT4222_VENDER_CMD_NOT_SUPPORTED,
            FT4222_IS_NOT_SPI_MODE,
            FT4222_IS_NOT_I2C_MODE,
            FT4222_IS_NOT_SPI_SINGLE_MODE,
            FT4222_IS_NOT_SPI_MULTI_MODE,
            FT4222_WRONG_I2C_ADDR,
            FT4222_INVAILD_FUNCTION,
            FT4222_INVALID_POINTER,
            FT4222_EXCEEDED_MAX_TRANSFER_SIZE,
            FT4222_FAILED_TO_READ_DEVICE,
            FT4222_I2C_NOT_SUPPORTED_IN_THIS_MODE,
            FT4222_GPIO_NOT_SUPPORTED_IN_THIS_MODE,
            FT4222_GPIO_EXCEEDED_MAX_PORTNUM,
            FT4222_GPIO_WRITE_NOT_SUPPORTED,
            FT4222_GPIO_PULLUP_INVALID_IN_INPUTMODE,
            FT4222_GPIO_PULLDOWN_INVALID_IN_INPUTMODE,
            FT4222_GPIO_OPENDRAIN_INVALID_IN_OUTPUTMODE,
            FT4222_INTERRUPT_NOT_SUPPORTED,
            FT4222_GPIO_INPUT_NOT_SUPPORTED,
            FT4222_EVENT_NOT_SUPPORTED,
        };

        public enum FT4222_ClockRate
        {
            SYS_CLK_60 = 0,
            SYS_CLK_24,
            SYS_CLK_48,
            SYS_CLK_80,

        };

        public enum FT4222_SPIMode
        {
            SPI_IO_NONE = 0,
            SPI_IO_SINGLE = 1,
            SPI_IO_DUAL = 2,
            SPI_IO_QUAD = 4,

        };

        public enum FT4222_SPIClock
        {
            CLK_NONE = 0,
            CLK_DIV_2,      // 1/2   System Clock
            CLK_DIV_4,      // 1/4   System Clock
            CLK_DIV_8,      // 1/8   System Clock
            CLK_DIV_16,     // 1/16  System Clock
            CLK_DIV_32,     // 1/32  System Clock
            CLK_DIV_64,     // 1/64  System Clock
            CLK_DIV_128,    // 1/128 System Clock
            CLK_DIV_256,    // 1/256 System Clock
            CLK_DIV_512,    // 1/512 System Clock

        };

        public enum FT4222_SPICPOL
        {
            CLK_IDLE_LOW = 0,
            CLK_IDLE_HIGH = 1,
        };

        public enum FT4222_SPICPHA
        {
            CLK_LEADING = 0,
            CLK_TRAILING = 1,
        };

        public enum SPI_DrivingStrength
        {
            DS_4MA = 0,
            DS_8MA,
            DS_12MA,
            DS_16MA,
        };

        // variable
        FTDI.FT_DEVICE_INFO_NODE devInfo = new FTDI.FT_DEVICE_INFO_NODE();

        IntPtr ftHandle = new IntPtr();

        FTDI.FT_STATUS ftStatus = 0;

        FT4222_STATUS ft42Status = 0;

        private AutoResetEvent dataReadyEvent = new AutoResetEvent(false);

        private bool continueReading = true;

        private Thread? readThread;

        public DelegateFt4222Data? delegateFt4222Data;

        public DelegateFt4222Status? delegateFt4222Status;

        public void SetDelegate(DelegateFt4222Data callback)
        {
            delegateFt4222Data = callback;
        }

        public void SetDelegate(DelegateFt4222Status callback)
        {
            delegateFt4222Status = callback;
        }

        private string _deviceID;
        private Dictionary<string, byte[]> deviceDataArray;
        public FT4222H(string deviceID)
        {
            _deviceID = deviceID;
            deviceDataArray = new();
        }

        public void Connection()
        {
            // Check device
            UInt32 numOfDevices = 0;
            UInt32 index = 0;
            ftStatus = FT_CreateDeviceInfoList(ref numOfDevices);

            if (numOfDevices == 0)
            {
                Debug.WriteLine("No FTDI device");
                delegateFt4222Status(false);
                return;
            }

            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            do
            {
                ftStatus = FT_GetDeviceInfoDetail(index, ref devInfo.Flags, ref devInfo.Type, ref devInfo.ID, ref devInfo.LocId,
                                            sernum, desc, ref devInfo.ftHandle);

                devInfo.SerialNumber = Encoding.ASCII.GetString(sernum, 0, 16);
                devInfo.Description = Encoding.ASCII.GetString(desc, 0, 64);
                devInfo.SerialNumber = devInfo.SerialNumber.Substring(0, devInfo.SerialNumber.IndexOf("\0"));
                devInfo.Description = devInfo.Description.Substring(0, devInfo.Description.IndexOf("\0"));

                Debug.WriteLine("LocId: {0}", devInfo.LocId);

                // Open device
                ftStatus = FT_OpenEx(devInfo.LocId, FT_OPEN_BY_LOCATION, ref ftHandle);
                Debug.WriteLine("Device Number: {0}", ftHandle);
                index++;

            } while (ftStatus != FTDI.FT_STATUS.FT_OK && (index < numOfDevices));

            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Open NG: {0}", ftStatus);
                delegateFt4222Status(false);
                return;
            }


            // Set FT4222 clock
            FT4222_ClockRate ft4222_Clock = FT4222_ClockRate.SYS_CLK_80;

            ft42Status = FT4222_SetClock(ftHandle, FT4222_ClockRate.SYS_CLK_80);
            if (ft42Status != FT4222_STATUS.FT4222_OK)
            {
                Debug.WriteLine("SetClock NG: {0}. Press Enter to continue.", ft42Status);
                delegateFt4222Status(false);
            }
            else
            {
                Debug.WriteLine("SetClock OK");

                ft42Status = FT4222_GetClock(ftHandle, ref ft4222_Clock);
                if (ft42Status != FT4222_STATUS.FT4222_OK)
                {
                    Debug.WriteLine("GetClock NG: {0}. Press Enter to continue.", ft42Status);
                    delegateFt4222Status(false);
                }
                else
                {
                    Debug.WriteLine("GetClock:" + ft4222_Clock);
                }
            }


            // Initialize the FT4222 as an SPI slave
            ft42Status = FT4222_SPISlave_InitEx(ftHandle, true, 5000, 5000);
            if (ft42Status != FT4222_STATUS.FT4222_OK)
            {
                Debug.WriteLine("SPI Slave Init Failed: {0}", ft42Status);
                delegateFt4222Status(false);
                return;
            }


            ft42Status = FT4222_SPISlave_SetMode(ftHandle, FT4222_SPICPOL.CLK_IDLE_LOW, FT4222_SPICPHA.CLK_LEADING);
            if (ft42Status != FT4222_STATUS.FT4222_OK)
            {
                Debug.WriteLine("SPI Slave Init Failed: {0}", ft42Status);
                delegateFt4222Status(false);
                return;
            }

            // Initialize and start the reading thread
            continueReading = true;
            readThread = new Thread(SPI_Read);
            readThread.IsBackground = true;
            readThread.Start();

            delegateFt4222Status(true);
        }

        public void StopReading()
        {
            continueReading = false;

            if (readThread != null)
            {
                readThread.Join();
                readThread = null;
            }

            if (ftHandle != IntPtr.Zero)
            {
                FT_Close(ftHandle);  // Properly close the device before re-opening
                ftHandle = IntPtr.Zero;
            }
        }


        public void singleWrite(byte[] writeBuf)
        {
            ushort sizeTransferred = 0;

            // Write data to the SPI master
            ft42Status = FT4222_SPISlave_Write(ftHandle, writeBuf, (ushort)writeBuf.Length, ref sizeTransferred);
            if (ft42Status != FT4222_STATUS.FT4222_OK)
            {
                Debug.WriteLine("Write NG: {0}", ft42Status);
                return;
            }
            string strWHex = BitConverter.ToString(writeBuf).Replace("-", " ");
            Debug.WriteLine("W (Hex): " + strWHex);
        }

        public void singleReadWrite(byte[] writeBuf)
        {
            ushort sizeTransferred = 0;

            // Write data to the SPI master
            ft42Status = FT4222_SPISlave_Write(ftHandle, writeBuf, (ushort)writeBuf.Length, ref sizeTransferred);
            if (ft42Status != FT4222_STATUS.FT4222_OK)
            {
                Debug.WriteLine("Write NG: {0}", ft42Status);
                return;
            }
            string strWHex = BitConverter.ToString(writeBuf).Replace("-", " ");
            Debug.WriteLine("W (Hex): " + strWHex);

            // Signal the read thread that data is ready
            dataReadyEvent.Set();
        }

        private List<byte> buffer = new List<byte>();

        private void SPI_Read()
        {
            byte[] header = new byte[] { 0x55, 0x55 };
            int headerLength = header.Length;
            int payloadLength = 266;

            while (continueReading)
            {
                if (!continueReading)
                    break;
                //Debug.WriteLine("SPI_Read Thread alive.");

                ushort rxSize = 0;
                FT4222_STATUS status = FT4222_SPISlave_GetRxStatus(ftHandle, ref rxSize);
                if (status != FT4222_STATUS.FT4222_OK)
                {
                    Debug.WriteLine("Failed to get RX status. Status: " + status);
                    continue;
                }

                //Debug.WriteLine("Bytes available in RX buffer: " + rxSize);

                if (rxSize == 0)
                {
                    // No data available, you might want to wait a bit before checking again
                    Thread.Sleep(100); // Adjust the sleep time as needed
                    continue;
                }

                ushort sizeOfRead = 0;
                byte[] readBuf = new byte[1024];

                // Read data from the SPI master
                ft42Status = FT4222_SPISlave_Read(ftHandle, readBuf, (ushort)readBuf.Length, ref sizeOfRead);
                if (ft42Status != FT4222_STATUS.FT4222_OK)
                {
                    Debug.WriteLine("Read NG: {0}", ft42Status);
                    continue;
                }

                string readBufHex = BitConverter.ToString(readBuf).Replace("-", " ");
                //Debug.WriteLine("readBuf: " + readBufHex);

                // Append received data to the buffer
                buffer.AddRange(readBuf.Take(sizeOfRead));

                // Search for the header in the buffer
                int headerStartIndex = -1;
                for (int i = 0; i <= buffer.Count - headerLength; i++)
                {
                    if (buffer.Skip(i).Take(headerLength).SequenceEqual(header))
                    {
                        headerStartIndex = i;
                        break;
                    }
                }

                if (headerStartIndex != -1 && (buffer.Count - headerStartIndex) >= payloadLength)
                {

                    payloadLength = buffer[headerStartIndex + 5] << 8 | buffer[headerStartIndex + 4] + 8;
                    //Debug.WriteLine($"payloadLength: {payloadLength}");

                    //Debug.WriteLine($"Header found at index: {headerStartIndex}");

                    // Copy the data segment of 266 bytes
                    byte[] dataSegment = buffer.Skip(headerStartIndex).Take(payloadLength).ToArray();
                    //Debug.WriteLine("Data Segment Count: " + dataSegment.Length);

                    string dataSegmentHex = BitConverter.ToString(dataSegment).Replace("-", " ");
                    //Debug.WriteLine("Data Segment (Hex): " + dataSegmentHex);
                    deviceDataArray[_deviceID] = dataSegment;
                    // Send the data segment for analysis
                    if (delegateFt4222Data != null)
                    {
                        delegateFt4222Data(deviceDataArray);
                    }

                    // Remove the sent data from the buffer
                    //buffer.RemoveRange(0, headerStartIndex + payloadLength);
                }
                else
                {
                    // Debug.WriteLine("Header not found or not enough data in the received data.");
                }
                buffer.Clear();
            }
        }
    }
}
