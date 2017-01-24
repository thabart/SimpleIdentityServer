using System;
using System.Diagnostics;
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
            // Enable short frames
            WriteRegister(Registers.BitFraming, 0x07);

            // Transceive the Request command to the tag
            Transceive(false, PiccCommands.Request);

            // Disable short frames
            WriteRegister(Registers.BitFraming, 0x00);

            var fifoLevel = GetFifoLevel();
            var fifoShort = ReadFromFifoShort();

            Debug.WriteLine(fifoLevel+" "+fifoShort);

            // Check if we found a card
            return fifoLevel == 2 && fifoShort == PiccResponses.AnswerToRequest;
        }

        public Uid ReadUid()
        {
            // Run the anti-collision loop on the card
            Transceive(false, PiccCommands.Anticollision_1, PiccCommands.Anticollision_2);

            // Return tag UID from FIFO
            return new Uid(ReadFromFifo(5));
        }

        public void HaltTag()
        {
            // Transceive the Halt command to the tag
            Transceive(false, PiccCommands.Halt_1, PiccCommands.Halt_2);
        }

        public bool SelectTag(Uid uid)
        {
            // Send Select command to tag
            var data = new byte[7];
            data[0] = PiccCommands.Select_1;
            data[1] = PiccCommands.Select_2;
            uid.FullUid.CopyTo(data, 2);

            Transceive(true, data);

            return GetFifoLevel() == 1 && ReadFromFifo() == PiccResponses.SelectAcknowledge;
        }

        internal byte[] ReadBlock(byte blockNumber, Uid uid, byte[] keyA = null, byte[] keyB = null)
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
            
            // Read block
            Transceive(PcdCommands.Transceive, true, PiccCommands.Read, blockNumber);
            return ReadFromFifo(16);
        }

        internal bool WriteBlock(byte blockNumber, Uid uid, byte[] data, byte[] keyA = null, byte[] keyB = null)
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
                return false;
            }

            // Write block
            Transceive(true, PiccCommands.Write, blockNumber);
            var resp = ReadFromFifo();
            if (resp != PiccResponses.Acknowledge)
            {
                return false;
            }

            // Make sure we write only 16 bytes
            var buffer = new byte[16];
            data.CopyTo(buffer, 0);

            Transceive(true, buffer);

            return ReadFromFifo() == PiccResponses.Acknowledge;
        }
        
        protected void MifareAuthenticate(byte command, byte blockNumber, Uid uid, byte[] key)
        {
            const int MF_KEY_SIZE = 6;
            byte waitIRq = 0x10;
            // Create Authentication packet
            var sendData = new byte[12];
            sendData[0] = command;
            sendData[1] = 7; // TODO : Pass in parameter : (Trailer block).
            for (var i = 0; i < MF_KEY_SIZE; i++) // 6 key bytes.
            {
                sendData[2 + i] = key[i];
            }

            for (var i = 0; i < 4; i++) // The last 4 bytes of the UID.
            {
                sendData[8 + i] = uid.Bytes[i + uid.Bytes.Count() - 4];
            }

            Transceive(command, false, sendData);
        }

        protected void Transceive(bool enableCrc, params byte[] data)
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

        protected void Transceive(byte command, bool enableCrc, params byte[] data)
        {
            if (enableCrc)
            {
                // Enable CRC
                SetRegisterBits(Registers.TxMode, 0x80);
                SetRegisterBits(Registers.RxMode, 0x80);
            }

            byte txLastBits = 0;
            byte bitFraming = 0;

            WriteRegister(Registers.Command, PcdCommands.Idle); //  Put reader in Idle mode
            WriteRegister(Registers.ComIrq, 0x7F);              // Clear all seven interrupt request bits
            SetRegisterBits(Registers.FifoLevel, 0x80);         // FlushBuffer = 1, FIFO initialization            
            WriteToFifo(data);                                  // Write the data to the FIFO
            WriteRegister(Registers.BitFraming, bitFraming);    // Bit adjustments
            WriteRegister(Registers.Command, command);     
            if (command == PcdCommands.Transceive)
            {
                SetRegisterBits(Registers.BitFraming, 0x80);
            }
            

            // Wait for (a generous) 25 ms
            Task.Delay(25).Wait();

            if (enableCrc)
            {
                // Disable CRC
                ClearRegisterBits(Registers.TxMode, 0x80);
                ClearRegisterBits(Registers.RxMode, 0x80);
            }
        }


        protected byte[] ReadFromFifo(int length)
        {
            var buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = ReadRegister(Registers.FifoData);
            }

            return buffer;
        }

        protected byte ReadFromFifo()
        {
            return ReadFromFifo(1)[0];
        }

        protected void WriteToFifo(params byte[] values)
        {
            foreach (var b in values)
            {
                WriteRegister(Registers.FifoData, b);
            }
        }

        protected int GetFifoLevel()
        {
            return ReadRegister(Registers.FifoLevel);
        }


        protected byte ReadRegister(byte register)
        {
            register <<= 1;
            register |= 0x80;
            var writeBuffer = new byte[] { register, 0x00 };
            return TransferSpi(writeBuffer)[1];
        }

        protected ushort ReadFromFifoShort()
        {
            var low = ReadRegister(Registers.FifoData);
            var high = (ushort)(ReadRegister(Registers.FifoData) << 8);

            return (ushort)(high | low);
        }

        protected void WriteRegister(byte register, byte value)
        {
            register <<= 1;
            var writeBuffer = new byte[] { register, value };
            TransferSpi(writeBuffer);
        }

        protected void SetRegisterBits(byte register, byte bits)
        {
            var currentValue = ReadRegister(register);
            WriteRegister(register, (byte)(currentValue | bits));
        }

        protected void ClearRegisterBits(byte register, byte bits)
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
    }
}
