#region copyright
// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Runtime.InteropServices;

namespace SimpleIdentityServer.Rfid
{
    internal enum RequestModes
    {
        KeyA = 0x00,
        KeyB = 0x01,
        RequestIdle = 0x26,
        RequestAll = 0x52
    }

    internal static class Reader
    {
        #region System settings

        [DllImport("function.dll")]
        public static extern int SetSerNum(byte[] newValue, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int GetSerNum([In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int GetVersionNum([In]byte[] strVersionNum);

        /// <summary>
        /// Set LED's action mode such as on/off duration and flash times.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="duration"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int ControlLED(int freq, int duration, [In]byte[] buffer);
        
        /// <summary>
        /// Set Beep's action module such as on/off and beeping times.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="duration"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int ControlBuzzer(int freq, int duration, [In]byte[] buffer);

        /// <summary>
        /// Get user information from the module.
        /// </summary>
        /// <param name="block">Number of the data area.</param>
        /// <param name="length">The length of data</param>
        /// <param name="buffer">Returned data.</param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int ReadUserInfo(byte block, byte length, [In]byte[] buffer);

        /// <summary>
        /// The module provider the four data blocks to user.
        /// Each block have 120 bytes space.
        /// </summary>
        /// <param name="block">The number of the data area.</param>
        /// <param name="length">The length of the data (the length must be less than 120).</param>
        /// <param name="buffer">Data</param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int WriteUserInfo(byte block, byte length, [In] byte[] buffer);

        #endregion

        #region 14443A-MF

        /// <summary>
        /// The Read command integrates the low level commands (Request, Anti-Collision, Select, Authentication)
        /// Let the user to select the card and read data from the memory blocks by a single command.
        /// </summary>
        /// <param name="mode">Mode control</param>
        /// <param name="blk_add">Number of blocks to be read (Max 4).</param>
        /// <param name="num_blk">The start address of blocks to be read (the range is 0~63).</param>
        /// <param name="snr">The six bytes block key.</param>
        /// <param name="buffer">Data read fropm the card.</param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int MF_Read(byte mode, byte blk_add, byte num_blk, [In]byte[] snr, [In]byte[] buffer);

        /// <summary>
        /// The write command integrates the low level commands (Request, Anti-Collision, Select, Authentication)
        /// and let the user to select the card and write data to the memory blocks by a single command.
        /// </summary>
        /// <param name="mode">Mode control</param>
        /// <param name="blk_add">The start address of blocks to be write.</param>
        /// <param name="num_blk">Number of blocks to be write.</param>
        /// <param name="snr">16 bytes block key.</param>
        /// <param name="buffer">Data.</param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int MF_Write(byte mode, byte blk_add, byte num_blk, [In]byte[] snr, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int MF_InitValue(byte mode, byte SectNum, [In]byte[] snr, [In]byte[] value);

        [DllImport("function.dll")]
        public static extern int MF_Dec(byte mode, byte SectNum, [In]byte[] snr, [In]byte[] value);

        [DllImport("function.dll")]
        public static extern int MF_Inc(byte mode, byte SectNum, [In]byte[] snr, [In]byte[] value);

        [DllImport("function.dll")]
        public static extern int MF_Request([In]byte[] commHandle, int DeviceAdddress, byte inf_mode, [In]byte[] Buffer);

        [DllImport("function.dll")]
        public static extern int MF_Select([In]byte[] commHandle, int DeviiceAddress, byte inf_mode, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int MF_Halt();

        [DllImport("function.dll")]
        public static extern int MF_Anticoll([In]byte[] commHandle, int DeviceAddress, [In]byte[] snr, [In]byte[] status);

        [DllImport("function.dll")]
        public static extern int MF_Restore([In]byte[] commHandle, int DeviceAddress, byte mode, byte cardlength, [In]byte[] carddata);

        /// <summary>
        /// The High level Command integrates the low level commands (Request, AntiColl1, Select) and get the SNR of selected card.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="halt"></param>
        /// <param name="snr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DllImport("function.dll")]
        public static extern int MF_Getsnr(int mode, int halt, [In]byte[] snr, [In]byte[] value);

        #endregion

        #region Ultralight

        [DllImport("function.dll")]
        public static extern int UL_Request(byte mode, [In]byte[] snr);

        [DllImport("function.dll")]
        public static extern int UL_HLRead(byte mode, byte blk_add, [In]byte[] snr, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int UL_HLWrite(byte mode, byte blk_add, [In]byte[] snr, [In]byte[] buffer);

        #endregion

        #region ISO14443TypeB

        [DllImport("function.dll")]
        public static extern int TypeB_Request([In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int TYPEB_SFZSNR(byte mode, byte halt, [In]byte[] snr, [In]byte[] value);

        [DllImport("function.dll")]
        public static extern int TypeB_TransCOS([In]byte[] cmd, int cmdSize, [In]byte[] buffer);

        #endregion

        #region ISO15693
        
        [DllImport("function.dll")]
        public static extern int ISO15693_Inventory([In]byte[] Cardnumber, [In]byte[] pBuffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_Read(byte flags, byte blk_add, byte num_blk, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_Write(byte flag, byte blk_add, byte num_blk, [In]byte[] uid, [In]byte[] data);

        [DllImport("function.dll")]
        public static extern int ISO15693_GetSysInfo(byte flag, [In]byte[] uid, [In]byte[] Buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_Lock(byte flags, byte num_blk, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_Select(byte flags, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_WriteAFI(byte flags, byte afi, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_LockAFI(byte flags, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_WriteDSFID(byte flags, byte DSFID, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_LockDSFID(byte flags, [In]byte[] uid, [In]byte[] buffer);

        [DllImport("function.dll")]
        public static extern int ISO15693_GetMulSecurity(byte flag, byte blkAddr, byte blkNum, [In]byte[] uid, [In]byte[] pBuffer);

        #endregion
    }
}
