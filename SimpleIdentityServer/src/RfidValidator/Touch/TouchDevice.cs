using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace RfidValidator.Touch
{
    public interface ITouchDevice
    {
        void ReadTouchPoints();
    }

    public class TouchDevice : ITouchDevice
    {
        private const int MIN_PRESSURE = 5;
        private const int CS_PIN = 1;
        private const byte CMD_START = 0x80;
        private const byte CMD_12BIT = 0x00;
        private const byte CMD_8BIT = 0x08;
        private const byte CMD_DIFF = 0x00;
        private const byte CMD_X_POS = 0x10;
        private const byte CMD_Z1_POS = 0x30;
        private const byte CMD_Z2_POS = 0x40;
        private const byte CMD_Y_POS = 0x50;

        private readonly SpiDevice _spiDevice;

        private TouchDevice(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        public static async Task<TouchDevice> Get()
        {
            var touchSettings = new SpiConnectionSettings(CS_PIN);
            touchSettings.ClockFrequency = 125000;
            touchSettings.Mode = SpiMode.Mode0;
            var deviceAqs = SpiDevice.GetDeviceSelector("SPI0");
            var deviceInfo = await DeviceInformation.FindAllAsync(deviceAqs);
            var spiDevice = await SpiDevice.FromIdAsync(deviceInfo[0].Id, touchSettings);
            return new TouchDevice(spiDevice);
        }

        public void ReadTouchPoints()
        {
            int p, a1, a2, b1, b2, x , y;
            byte[] writeBuffer24 = new byte[3],
                readBuffer24 = new byte[3];

            // 1. Get pressure
            writeBuffer24[0] = (byte)(CMD_START | CMD_8BIT | CMD_DIFF | CMD_Z1_POS);
            _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
            a1 = readBuffer24[1] & 0x7F;
            writeBuffer24[0] = (byte)(CMD_START | CMD_8BIT | CMD_DIFF | CMD_Z2_POS);
            _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
            b1 = 255 - readBuffer24[1] & 0x7F;
            p = a1 + b1;

            if (p > MIN_PRESSURE)
            {
                // 1.1 Get X data
                writeBuffer24[0] = (byte)(CMD_START | CMD_12BIT | CMD_DIFF | CMD_X_POS);
                _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
                a1 = readBuffer24[1];
                b1 = readBuffer24[2];
                writeBuffer24[0] = (byte)(CMD_START | CMD_12BIT | CMD_DIFF | CMD_X_POS);
                _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
                a2 = readBuffer24[1];
                b2 = readBuffer24[2];

                if (a1 == a2)
                {
                    x = ((a2 << 4) | (b2 >> 4)); //12bit: ((a<<4)|(b>>4)) //10bit: ((a<<2)|(b>>6))

                    //Het Y data
                    writeBuffer24[0] = (byte)(CMD_START | CMD_12BIT | CMD_DIFF | CMD_Y_POS);
                    _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
                    a1 = readBuffer24[1];
                    b1 = readBuffer24[2];
                    writeBuffer24[0] = (byte)(CMD_START | CMD_12BIT | CMD_DIFF | CMD_Y_POS);
                    _spiDevice.TransferFullDuplex(writeBuffer24, readBuffer24);
                    a2 = readBuffer24[1];
                    b2 = readBuffer24[2];
                    if (a1 == a2)
                    {
                        y = ((a2 << 4) | (b2 >> 4)); //12bit: ((a<<4)|(b>>4)) //10bit: ((a<<2)|(b>>6))
                        if (x > 0 && y > 0)
                        {

                        }
                    }
                }
            }

            string s = "";
        }
    }
}
