using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UneoWebApplicationAutoInstaller.Host
{
    public class HostCmds
    {
        public static byte[] Read_Command_1()
        {
            byte[] cmdPackage = {
                0x55,
                0x55,
                0xA5,
                0x5A,
                0x02,
                0,
                0,
                0,
                0x50,
                0x43,
            };
            return addCRC16(cmdPackage);
        }
        public static byte[] ReadWifiConfig()
        {
            byte[] cmdPackage = {
                0x55,
                0x55,
                0x02,
                0xCD,
                0,
                0,
                0,
                0 };
            return addCRC16(cmdPackage);
        }        
        public static byte[] WriteWifiConfig(byte[] ap, byte[] lan, byte[] server)
        {
            byte[] cmdPackage = new byte[100];
            cmdPackage[0] = 0x55;
            cmdPackage[1] = 0x55;
            cmdPackage[2] = 0x82;
            cmdPackage[3] = 0xCD;
            cmdPackage[4] = 0x5C;
            cmdPackage[5] = 0;
            cmdPackage[6] = 0;
            cmdPackage[7] = 0;

            for (int i = 0; i < ap.Length; i++)
            {
                cmdPackage[8 + i] = ap[i];
            }

            for (int i = 0; i < lan.Length; i++)
            {
                cmdPackage[74 + i] = lan[i];
            }

            for (int i = 0; i < server.Length; i++)
            {
                cmdPackage[92 + i] = server[i];
            }

            return addCRC16(cmdPackage);
        }
        public static byte[] WritSystemConfig(bool enableWiFi)
        {
            byte[] cmdPackage = new byte[100];
            cmdPackage[0] = 0x55;
            cmdPackage[1] = 0x55;
            cmdPackage[2] = 0x81;
            cmdPackage[3] = 0xCD;
            cmdPackage[4] = 0x0C;
            cmdPackage[5] = 0;
            cmdPackage[6] = 0;
            cmdPackage[7] = 0;

            //Timestamp
            cmdPackage[8] = 0;
            cmdPackage[9] = 0;
            cmdPackage[10] = (byte)DateTime.UtcNow.Second;
            cmdPackage[11] = (byte)DateTime.UtcNow.Minute;
            cmdPackage[12] = (byte)DateTime.UtcNow.Hour;
            cmdPackage[13] = (byte)DateTime.UtcNow.Day;
            cmdPackage[14] = (byte)DateTime.UtcNow.Month;
            cmdPackage[15] = (byte)(DateTime.UtcNow.Year - 2000);

            //Interface
            cmdPackage[16] = (byte)(enableWiFi ? 1 : 0); //0x01; // Enable WiFi Config
            cmdPackage[17] = 0;
            cmdPackage[18] = 0;
            cmdPackage[19] = 0;

            return addCRC16(cmdPackage);
        }

        public static byte[] WriteWiFiConfigProgram()
        {
            byte[] cmdPackage = new byte[32];
            cmdPackage[0] = 0x55;
            cmdPackage[1] = 0x55;
            cmdPackage[2] = 0x8F;
            cmdPackage[3] = 0xCD;
            cmdPackage[4] = 0x20;
            cmdPackage[5] = 0;
            cmdPackage[6] = 0;
            cmdPackage[7] = 0;
            cmdPackage[8] = 0x40;
            cmdPackage[9] = 0x50;
            cmdPackage[10] = 0x33;
            cmdPackage[11] = 0x30;

            return addCRC16(cmdPackage);
        }

        public static byte[] ReadWiFiConfigProgramStatus()
        {
            byte[] cmdPackage = new byte[32];
            cmdPackage[0] = 0x55;
            cmdPackage[1] = 0x55;
            cmdPackage[2] = 0x0F;
            cmdPackage[3] = 0xCD;
            cmdPackage[4] = 0x20;
            cmdPackage[5] = 0;
            cmdPackage[6] = 0;
            cmdPackage[7] = 0;
            cmdPackage[8] = 0x52;
            cmdPackage[9] = 0x45;
            cmdPackage[10] = 0x41;
            cmdPackage[11] = 0x44;

            return addCRC16(cmdPackage);
        }    
        /// <summary>
        /// CD07指令，傳送給Server請求連上Server
        /// </summary>
        /// <param name="mac">UCB MAC Address</param>
        /// <returns></returns>
        public static byte[] SendCD07ToServer(string mac)
        {
            byte[] macBytes = MacAddressToBytes(mac);
            byte[] cmdPackage = {
                0x55,
                0x55,
                0x07,
                0xCD,
                0,
                0x0A,
                0,
                0,
                macBytes[0],
                macBytes[1],
                macBytes[2],
                macBytes[3],
                macBytes[4],
                macBytes[5],
                0,
                0 ,
                0,
                0};
            return addCRC16(cmdPackage);
        }

        /// <summary>
        ///  CD08指令，傳送給Server請求下一個動作
        /// </summary>
        /// <returns></returns>
        public static byte[] SendCD08ToServer()
        {
            byte[] cmdPackage = {
                0x55,
                0x55,
                0x08,
                0xCD,
                0,
                0,
                0,
                0
            };
            //return cmdPackage
            return addCRC16(cmdPackage);
        }

        /// <summary>
        /// CD04 指令範例
        /// </summary>
        public static void SendCD04ToServer()
        {
            // 丟假資料給Server
            // 27*72 = 1496 bytes
            // 分兩個封包送
            // frameId是流水號遞增

            int frameId = 0;
            GetCD04_1(1452, 1, DummyData(1432), frameId++);
            GetCD04_2(520, 2, DummyData(512), frameId++);
        }

        /// <summary>
        /// 產生Sensor假資料
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] DummyData(int len)
        {
            byte[] dummy = new byte[len];
            Random rand = new Random();
            for (int i = 0; i < len; i++)
            {
                dummy[i] = (byte)rand.Next(0, 256);
            }
            return dummy;
        }

        /// <summary>
        /// CD04 packet section 1
        /// </summary>
        /// <param name="len">payload length</param>
        /// <param name="packetSection">1</param>
        /// <param name="data">sensor壓力</param>
        /// <param name="frameId">流水號，每船一筆就要遞增，檢查用</param>
        /// <returns></returns>
        public static byte[] GetCD04_1(int len, int packetSection, byte[] data, int frameId)
        {
            byte[] cmdPackage = new byte[1460];

            // Packet Header: Byte 0 to 7
            byte[] packetHeader = {
                0x55,
                0x55,
                0x04,
                0xCD,
                (byte)(len & 0xFF),
                (byte)(len >> 8 & 0xFF),
                0,
                0
            };

            // Data Header: Byte 8 to 15
            var packetType = 0x82;
            var totalSection = 2;
            var totalLength = 2048;
            var packetLength = data.Length;
            byte[] dataHeader = {
                (byte)packetType,
                (byte)frameId ,
                (byte)totalSection,
                (byte)packetSection,
                (byte)(totalLength & 0xFF),
                (byte)(totalLength >> 8 & 0xFF),
                (byte)(packetLength & 0xFF),
                (byte)(packetLength >> 8 & 0xFF),
                (byte)0,    //timestamp
                (byte)0,
                (byte)0,
                (byte)0,
                (byte)0,
                (byte)0,
                (byte)0,
                (byte)0,
                (byte)0x01,
                (byte)0x08,
                (byte)0x1B,      //27
                (byte)0x48,     //72
            };

            Array.Copy(packetHeader, 0, cmdPackage, 0, packetHeader.Length);
            Array.Copy(dataHeader, 0, cmdPackage, packetHeader.Length, dataHeader.Length);
            Array.Copy(data, 0, cmdPackage, packetHeader.Length + dataHeader.Length, data.Length);

            //return cmdPackage
            return addCRC16(cmdPackage);
        }

        /// <summary>
        /// CD04 packet section 2
        /// </summary>
        /// <param name="len">payload length</param>
        /// <param name="packetSection">2</param>
        /// <param name="data">sensor壓力</param>
        /// <param name="frameId">流水號，每船一筆就要遞增，檢查用</param>
        /// <returns></returns>
        public static byte[] GetCD04_2(int len, int packetSection, byte[] data, int frameId)
        {
            byte[] cmdPackage = new byte[1460];

            // Packet Header: Byte 0 to 7
            byte[] packetHeader = {
                0x55,
                0x55,
                0x04,
                0xCD,
                (byte)(len & 0xFF),
                (byte)(len >> 8 & 0xFF),
                0,
                0
            };

            // Data Header: Byte 8 to 15
            var packetType = 0x82;
            var totalSection = 2;
            var totalLength = 2048;
            var packetLength = data.Length;
            byte[] dataHeader = {
                (byte)packetType,
                (byte)frameId ,
                (byte)totalSection,
                (byte)packetSection,
                (byte)(totalLength & 0xFF),
                (byte)(totalLength >> 8 & 0xFF),
                (byte)(packetLength & 0xFF),
                (byte)(packetLength >> 8 & 0xFF),
            };

            Array.Copy(packetHeader, 0, cmdPackage, 0, packetHeader.Length);
            Array.Copy(dataHeader, 0, cmdPackage, packetHeader.Length, dataHeader.Length);
            Array.Copy(data, 0, cmdPackage, packetHeader.Length + dataHeader.Length, data.Length);

            //return cmdPackage
            return addCRC16(cmdPackage);
        }

        /// <summary>
        /// MAC address轉bytes array
        /// </summary>
        /// <param name="macAddress">UCB MAC</param>
        /// <returns></returns>
        public static byte[] MacAddressToBytes(string macAddress)
        {
            string[] hexValues = macAddress.Split(':');
            byte[] bytes = new byte[hexValues.Length];

            for (int i = 0; i < hexValues.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexValues[i], 16);
            }

            return bytes;
        }


        /// <summary>
        /// 將CheckSum整合到封包Byte 6, Byte7位置
        /// </summary>
        /// <param name="cmdPackage"></param>
        /// <returns></returns>
        private static byte[] addCRC16(byte[] cmdPackage)
        {
            byte[] copyArray = new byte[cmdPackage.Length - 8];
            Array.Copy(cmdPackage, 8, copyArray, 0, cmdPackage.Length - 8);
            var cksum = getCheckSum(copyArray);
            cmdPackage[7] = (byte)(cksum >> 8 & 0xFF);
            cmdPackage[6] = (byte)(cksum & 0xFF);

            return cmdPackage;
        }

        /// <summary>
        /// 計算封包CheckSum
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private static int getCheckSum(byte[] byteArray)
        {
            int sum = 0;
            byte[] unsignedBytes = byteArray.Select(sb => sb).ToArray();
            for (int i = 0; i < unsignedBytes.Length; i = i + 2)
            {
                var byte1 = unsignedBytes[i] & 0xFF;
                var byte2 = unsignedBytes[i + 1] & 0xFF;
                var combined = byte2 << 8 | byte1;
                sum += combined;
            }
            sum = sum & 0xFFFF;
            return sum;
        }      
        
    }
}
