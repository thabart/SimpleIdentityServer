using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace RfidValidator.Rfid
{
    public static class Registers
    {
        private const byte bitFraming = 0x0D;
        private const byte comIrq = 0x04;
        private const byte comIrqEnable = 0x02;
        private const byte command = 0x01;
        private const byte control = 0x0C;
        private const byte error = 0x06;
        private const byte fifoData = 0x09;
        private const byte fifoLevel = 0x0A;
        private const byte mode = 0x11;
        private const byte rxMode = 0x13;
        private const byte timerMode = 0x2A;
        private const byte timerPrescaler = 0x2B;
        private const byte timerReloadHigh = 0x2C;
        private const byte timerReloadLow = 0x2D;
        private const byte txAsk = 0x15;
        private const byte txControl = 0x14;
        private const byte txMode = 0x12;
        private const byte version = 0x37;
        private const byte divIrq = 0x05;
        private const byte cRCResultRegM = 0x21;
        private const byte cRCResultRegL = 0x22;

        public static byte CRCResultRegL
        {
            get
            {
                return cRCResultRegL;
            }
        }

        public static byte CRCResultRegM
        {
            get
            {
                return cRCResultRegM;
            }
        }

        public static byte DivIrq
        {
            get
            {
                return divIrq;
            }
        }

        public static byte BitFraming
        {
            get
            {
                return bitFraming;
            }
        }

        public static byte ComIrq
        {
            get
            {
                return comIrq;
            }
        }

        public static byte ComIrqEnable
        {
            get
            {
                return comIrqEnable;
            }
        }

        public static byte Command
        {
            get
            {
                return command;
            }
        }

        public static byte Control
        {
            get
            {
                return control;
            }
        }

        public static byte Error
        {
            get
            {
                return error;
            }
        }

        public static byte FifoData
        {
            get
            {
                return fifoData;
            }
        }

        public static byte FifoLevel
        {
            get
            {
                return fifoLevel;
            }
        }

        public static byte Mode
        {
            get
            {
                return mode;
            }
        }

        public static byte RxMode
        {
            get
            {
                return rxMode;
            }
        }

        public static byte TimerMode
        {
            get
            {
                return timerMode;
            }
        }

        public static byte TimerPrescaler
        {
            get
            {
                return timerPrescaler;
            }
        }

        public static byte TimerReloadHigh
        {
            get
            {
                return timerReloadHigh;
            }
        }

        public static byte TimerReloadLow
        {
            get
            {
                return timerReloadLow;
            }
        }

        public static byte TxAsk
        {
            get
            {
                return txAsk;
            }
        }

        public static byte TxControl
        {
            get
            {
                return txControl;
            }
        }

        public static byte TxMode
        {
            get
            {
                return txMode;
            }
        }

        public static byte Version
        {
            get
            {
                return version;
            }
        }
    }

    public static class PiccCommands
    {
        private const byte anticollision_1 = 0x93;
        private const byte anticollision_2 = 0x20;
        private const byte authenticateKeyA = 0x60;
        private const byte authenticateKeyB = 0x61;
        private const byte halt_1 = 0x50;
        private const byte halt_2 = 0x00;
        private const byte read = 0x30;
        private const byte request = 0x26;
        private const byte select_1 = 0x93;
        private const byte select_2 = 0x70;
        private const byte write = 0xA0;

        public static byte AuthenticateKeyA
        {
            get
            {
                return authenticateKeyA;
            }
        }

        public static byte AuthenticateKeyB
        {
            get
            {
                return authenticateKeyB;
            }
        }

        public static byte Halt_1
        {
            get
            {
                return halt_1;
            }
        }

        public static byte Halt_2
        {
            get
            {
                return halt_2;
            }
        }

        public static byte Read
        {
            get
            {
                return read;
            }
        }

        public static byte Request
        {
            get
            {
                return request;
            }
        }

        public static byte Select_1
        {
            get
            {
                return select_1;
            }
        }

        public static byte Select_2
        {
            get
            {
                return select_2;
            }
        }

        public static byte Write
        {
            get
            {
                return write;
            }
        }

        public static byte Anticollision_1
        {
            get
            {
                return anticollision_1;
            }
        }

        public static byte Anticollision_2
        {
            get
            {
                return anticollision_2;
            }
        }
    }

    public static class PcdCommands
    {
        private const byte idle = 0x00;
        private const byte mifareAuthenticate = 0x0E;
        private const byte transceive = 0x0C;
        private const byte calcCRC = 0x03;

        public static byte CalcCrc
        {
            get
            {
                return calcCRC;
            }
        }

        public static byte Idle
        {
            get
            {
                return idle;
            }
        }

        public static byte MifareAuthenticate
        {
            get
            {
                return mifareAuthenticate;
            }
        }

        public static byte Transceive
        {
            get
            {
                return transceive;
            }
        }
    }

    public class Uid
    {
        public byte Bcc { get; private set; }
        public byte[] Bytes { get; private set; }
        public byte[] FullUid { get; private set; }
        public bool IsValid { get; private set; }

        internal Uid(byte[] uid)
        {
            FullUid = uid;
            Bcc = uid[4];
            Bytes = new byte[4];
            Array.Copy(FullUid, 0, Bytes, 0, 4);

            foreach (var b in Bytes)
            {
                if (b != 0x00)
                {
                    IsValid = true;
                }
            }
        }

        public sealed override bool Equals(object obj)
        {
            if (!(obj is Uid))
            {
                return false;
            }

            var uidWrapper = (Uid)obj;
            for (int i = 0; i < 5; i++)
            {
                if (FullUid[i] != uidWrapper.FullUid[i])
                {
                    return false;
                }
            }

            return true;
        }

        public sealed override int GetHashCode()
        {
            int uid = 0;
            for (int i = 0; i < 4; i++)
            {
                uid |= Bytes[i] << (i * 8);
            }

            return uid;
        }

        public sealed override string ToString()
        {
            var formatString = "x" + (Bytes.Length * 2);
            return GetHashCode().ToString(formatString);
        }
    }

    public static class PiccResponses
    {
        private const ushort answerToRequest = 0x0004;
        private const byte selectAcknowledge = 0x08;
        private const byte acknowledge = 0x0A;

        public static byte Acknowledge
        {
            get
            {
                return acknowledge;
            }
        }

        public static byte SelectAcknowledge
        {
            get
            {
                return selectAcknowledge;
            }
        }

        public static ushort AnswerToRequest
        {
            get
            {
                return answerToRequest;
            }
        }
    }

    public class PiccResponse
    {
        /// <summary>
        /// Read the response.
        /// </summary>
        public IEnumerable<byte> Response { get; set; }
        /// <summary>
        /// Indicate the number of valid bits in the last received byte.
        /// </summary>
        public int ValidBits { get; set; }
    }

    public class RfidRc522
    {
        public SpiDevice _spi { get; private set; }
        public GpioController IoController { get; private set; }
        public GpioPin _resetPowerDown { get; private set; }

        /* Uncomment for Raspberry Pi 2 */
        private const string SPI_CONTROLLER_NAME = "SPI1";
        private const Int32 SPI_CHIP_SELECT_LINE = 0;
        private const Int32 RESET_PIN = 18;

        internal async Task Start()
        {
            try
            {
                IoController = GpioController.GetDefault();
                _resetPowerDown = IoController.OpenPin(RESET_PIN);
                _resetPowerDown.Write(GpioPinValue.High);
                _resetPowerDown.SetDriveMode(GpioPinDriveMode.Output);
            }
            /* If initialization fails, throw an exception */
            catch (Exception ex)
            {
                throw new Exception("GPIO initialization failed", ex);
            }

            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 1000000;
                settings.Mode = SpiMode.Mode0;
                String spiDeviceSelector = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var devices = await DeviceInformation.FindAllAsync(spiDeviceSelector);
                _spi = await SpiDevice.FromIdAsync(devices[0].Id, settings);

            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }


            Reset();
        }

        public void Reset()
        {
            _resetPowerDown.Write(GpioPinValue.Low);
            Task.Delay(50).Wait();
            _resetPowerDown.Write(GpioPinValue.High);
            Task.Delay(50).Wait();

            // Force 100% ASK modulation
            WriteRegister(Registers.TxAsk, 0x40);

            // Set CRC to 0x6363
            WriteRegister(Registers.Mode, 0x3D);

            // Enable antenna
            SetRegisterBits(Registers.TxControl, 0x03);
        }


        public bool IsTagPresent()
        {
            WriteRegister(Registers.BitFraming, 0x07);
            Transceive(false, PiccCommands.Request);
            WriteRegister(Registers.BitFraming, 0x00);
            return GetFifoLevel() == 2 && ReadFromFifoShort() == PiccResponses.AnswerToRequest;
        }

        public Uid ReadUid()
        {
            Transceive(false, PiccCommands.Anticollision_1, PiccCommands.Anticollision_2);
            return new Uid(ReadFromFifo(5));
        }

        public void HaltTag()
        {
            var sendData = new List<byte>
            {
                PiccCommands.Halt_1,
                0
            };
            sendData.AddRange(CalculateCrc(sendData.ToArray()));
            TransceiveData(sendData.ToArray(), false, 0, 0, false);
        }

        public void StopCrypto()
        {
            ClearRegisterBits(0x08, 0x08);
        }

        public bool SelectTag(Uid uid)
        {
            var data = new byte[7];
            data[0] = PiccCommands.Select_1;
            data[1] = PiccCommands.Select_2;
            uid.FullUid.CopyTo(data, 2);
            Transceive(true, data);
            return GetFifoLevel() == 1 && ReadFromFifo() == PiccResponses.SelectAcknowledge;
        }

        public byte[] ReadBlock(byte blockNumber, Uid uid, byte[] keyA = null, byte[] keyB = null)
        {
            if (keyA != null)
            {
                MifareAuthenticate(PiccCommands.AuthenticateKeyA, blockNumber, uid, keyA);
            }
            else if (keyB != null)
            {
                MifareAuthenticate(PiccCommands.AuthenticateKeyB, blockNumber, uid, keyB);
            }
            else
            {
                return null;
            }

            var sendData = new List<byte>
            {
                PiccCommands.Read,
                blockNumber
            };
            sendData.AddRange(CalculateCrc(sendData.ToArray()));
            var r = TransceiveData(sendData.ToArray(), true, 16, 0, true);
            return r.Response.ToArray();
        }

        public bool WriteBlock(byte blockNumber, Uid uid, byte[] data, byte[] keyA = null, byte[] keyB = null)
        {
            if (keyA != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyA, blockNumber, uid, keyA);
            else if (keyB != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyB, blockNumber, uid, keyB);
            else
                return false;

            // Write block
            Transceive(true, PiccCommands.Write, blockNumber);

            if (ReadFromFifo() != PiccResponses.Acknowledge)
                return false;

            // Make sure we write only 16 bytes
            var buffer = new byte[16];
            data.CopyTo(buffer, 0);

            Transceive(true, buffer);

            return ReadFromFifo() == PiccResponses.Acknowledge;
        }
        private PiccResponse TransceiveData(byte[] sendData, bool getData, byte backLength, byte rxAlign, bool checkCrc)
        {
            byte waitIrq = 0x30;
            return CommunicateWithPicc(PcdCommands.Transceive, waitIrq, sendData, getData, backLength, rxAlign, checkCrc);
        }

        private void MifareAuthenticate(byte command, byte blockNumber, Uid uid, byte[] key)
        {
            byte waitIrq = 0x10;
            // Create Authentication packet
            var sendData = new byte[12];
            sendData[0] = command;
            sendData[1] = 7;
            for (var i = 0; i < 6; i++) // 6 key bytes.
            {
                sendData[2 + i] = key[i];
            }

            for (var i = 0; i < 4; i++) // The last 4 bytes of the UID.
            {
                sendData[8 + i] = uid.Bytes[i + uid.Bytes.Count() - 4];
            }

            CommunicateWithPicc(PcdCommands.MifareAuthenticate, waitIrq, sendData, false, 0, 0, false);
        }

        private PiccResponse CommunicateWithPicc(
           byte command,
           byte waitIrq,
           byte[] sendData,
           bool getData,
           int backLength,
           byte rxAlign,
           bool checkCrc)
        {
            byte irqEn = 0x00;
            byte txLastBit = 0;
            byte bitFraming = (byte)((rxAlign << 4) + txLastBit);
            byte n;

            if (command == PcdCommands.MifareAuthenticate)
            {
                irqEn = 0x12;
            }
            else if (command == PcdCommands.Transceive)
            {
                irqEn = 0x77;
            }
                        
            WriteRegister(Registers.ComIrqEnable, (byte)(irqEn | 0x80));
            ClearRegisterBits(Registers.ComIrq, 0x80);
            SetRegisterBits(Registers.FifoLevel, 0x80);         // FlushBuffer = 1, FIFO initialization            
            WriteRegister(Registers.Command, PcdCommands.Idle); //  Put reader in Idle mode
            // WriteRegister(Registers.ComIrq, 0x7F);              // Clear all seven interrupt request bits
            WriteToFifo(sendData);                              // Write the data to the FIFO
            WriteRegister(Registers.Command, command);
            // WriteRegister(Registers.BitFraming, bitFraming);    // Bit adjustments
            if (command == PcdCommands.Transceive)
            {
                SetRegisterBits(Registers.BitFraming, 0x80);
            }

            // Values : https://www.rtp-net.org/misc/karotz/mfrc523.h
            // Set1 = 128 
            // TXIRQ  = 64 = a transmitted data stream ends.
            // RXIRQ = 32 = a received data stream ends
            // IDLEIRQ	= 16 = command execution finishes
            // HIALERTIRQ = 8 = FIFO buffer is almost full
            // LOALERTIRQ = 4 = FIFO buffer is almost empty
            // ERRIRQ = 2 = an error is dected.

            int i = 2000;
            while (true)
            {
                n = ReadRegister(Registers.ComIrq);                 // ComIrqReg[7..0] bits are: Set1 TxIRq RxIRq IdleIRq HiAlertIRq LoAlertIRq ErrIRq TimerIRq
                if ((n & waitIrq) == 0)
                {
                    break;
                }

                if ((n & 0x01) == 0 || (--i == 0))
                {
                    throw new TimeoutException("Timeout in communication");
                }
            }

            SetRegisterBits(Registers.BitFraming, 0x80);

            // WrErr = 128
            // TempErr = 64 = internal temperature sensor detects overheating
            // BuffErrOver = 16 = the host of the MFRC522's internal state machine tries to write data to the FIFO buffer.
            // CollErr = 8 = A bit-collision is detected.
            // CrcErr = 4 = CRC calculation fails.
            // PartityErr = 2 = Parity check failed.
            // ProtoErr = 1 = The number of bytes received in one data stream is incorrect.
            byte errorRegValue = ReadRegister(Registers.Error); // ErrorReg[7..0] bits are: WrErr TempErr reserved BufferOvfl CollErr CRCErr ParityErr ProtocolErr
            var r = errorRegValue & 0x1B;
            /*
            if ((errorRegValue & 0x13) == 0)
            {
                throw new InvalidOperationException("Error in communication");
            }*/

            var result = new PiccResponse();
            if (getData)
            {
                n = ReadRegister(Registers.FifoLevel); // Number of bytes in the FIFO.
                result.Response = ReadFromFifo(backLength);
                result.ValidBits = ReadRegister(Registers.Control) & 0x07;
            }

            /*
            if ((errorRegValue & 0x08) == 0)
            {
                throw new InvalidOperationException("Collision detected");
            }*/

            /*
            if (getData && checkCrc)
            {
                if (result.ValidBits == 4)
                {
                    throw new InvalidOperationException("A MIFARE PICC responded with NAK");
                }

                if (result.ValidBits != 0)
                {
                    throw new InvalidOperationException("The CRC_A does not match.");
                }

                int length = result.Response.Count();
                var tmp = result.Response.Take(length - 2).ToArray();
                var expectedCrc = CalculateCrc(tmp);
                if (result.Response.ElementAt(length - 2) != expectedCrc[0] || result.Response.ElementAt(length - 1) != expectedCrc[1])
                {
                    throw new InvalidOperationException("The CRC_A does not match.");
                }
            }*/

            return result;
        }

        private void Transceive(bool enableCrc, params byte[] data)
        {
            if (enableCrc)
            {
                // Enable CRC
                SetRegisterBits(Registers.TxMode, 0x80);
                SetRegisterBits(Registers.RxMode, 0x80);
            }

            // Put reader in Idle mode
            WriteRegister(Registers.Command, PcdCommands.Idle);

            // Clear the FIFO
            SetRegisterBits(Registers.FifoLevel, 0x80);

            // Write the data to the FIFO
            WriteToFifo(data);

            // Put reader in Transceive mode and start sending
            WriteRegister(Registers.Command, PcdCommands.Transceive);
            SetRegisterBits(Registers.BitFraming, 0x80);

            // Wait for (a generous) 25 ms
            System.Threading.Tasks.Task.Delay(25).Wait();

            // Stop sending
            ClearRegisterBits(Registers.BitFraming, 0x80);

            if (enableCrc)
            {
                // Disable CRC
                ClearRegisterBits(Registers.TxMode, 0x80);
                ClearRegisterBits(Registers.RxMode, 0x80);
            }
        }
        
        private byte[] ReadFromFifo(int length)
        {
            var buffer = new byte[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadRegister(Registers.FifoData);

            return buffer;
        }

        private byte ReadFromFifo()
        {
            return ReadFromFifo(1)[0];
        }

        private void WriteToFifo(params byte[] values)
        {
            foreach (var b in values)
            {
                WriteRegister(Registers.FifoData, b);
            }
        }

        private int GetFifoLevel()
        {
            return ReadRegister(Registers.FifoLevel);
        }

        private byte ReadRegister(byte register)
        {
            register <<= 1;
            register |= 0x80;

            var writeBuffer = new byte[] { register, 0x00 };

            return TransferSpi(writeBuffer)[1];
        }

        private ushort ReadFromFifoShort()
        {
            var low = ReadRegister(Registers.FifoData);
            var high = (ushort)(ReadRegister(Registers.FifoData) << 8);

            return (ushort)(high | low);
        }

        private void WriteRegister(byte register, byte value)
        {
            register <<= 1;
            var writeBuffer = new byte[] { register, value };
            TransferSpi(writeBuffer);
        }

        private void SetRegisterBits(byte register, byte bits)
        {
            var currentValue = ReadRegister(register);
            WriteRegister(register, (byte)(currentValue | bits));
        }

        private void ClearRegisterBits(byte register, byte bits)
        {
            var currentValue = ReadRegister(register);
            WriteRegister(register, (byte)(currentValue & ~bits));
        }

        private byte[] TransferSpi(byte[] writeBuffer)
        {
            var readBuffer = new byte[writeBuffer.Length];

            _spi.TransferFullDuplex(writeBuffer, readBuffer);

            return readBuffer;
        }

        private byte[] CalculateCrc(byte[] data)
        {
            WriteRegister(Registers.Command, PcdCommands.Idle);         //  Put reader in Idle mode
            WriteRegister(Registers.DivIrq, 0x04);                      // Clear the CRCIRq interrupt request bit
            SetRegisterBits(Registers.FifoLevel, 0x80);                 // FlushBuffer = 1, FIFO initialization      
            WriteToFifo(data);                                          // Write data to the FIFO
            WriteRegister(Registers.Command, PcdCommands.CalcCrc);      // Start the calculation

            Task.Delay(25).Wait();
            var result = new byte[2];
            result[0] = ReadRegister(Registers.CRCResultRegL);
            result[1] = ReadRegister(Registers.CRCResultRegM);
            return result;
        }
    }
}