using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace RfidValidator.Rfid
{
    public class RfidRc522
    {
        private const Int32 RESET_PIN = 18;
        private const Int32 SPI_CHIP_SELECT_LINE = 0;
        private GpioController _gpioController;
        private GpioPin _gpioResetPin;
        private SpiDevice _spi;

        public async Task Start()
        {
            try
            {
                _gpioController = await GpioController.GetDefaultAsync();
                _gpioResetPin = _gpioController.OpenPin(RESET_PIN);
                _gpioResetPin.Write(GpioPinValue.High);
                _gpioResetPin.SetDriveMode(GpioPinDriveMode.Output);

                SpiController controller = await SpiController.GetDefaultAsync();
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 1000000;
                settings.Mode = SpiMode.Mode0;
                _spi = controller.GetDevice(settings);
                string result = "";
            }
            catch(Exception ex)
            {
                string s = "";
            }
        }
    }
}
