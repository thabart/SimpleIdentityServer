#include <iostream>
#include <libusb.h>

// System settings
#define  SetAddress 	0x80
#define  SetBaudrate 	0x81
#define  SetSerlNum 	0x82
#define  GetSerlNum		0x83
#define  Write_UserInfo	0x84
#define  Read_UserInfo 	0x85
#define  Get_VersionNum	0x86
#define  Control_Led1	0x87
#define  Control_Led2	0x88
#define  Control_Buzzer	0x89

#define DATALEN 0x100

#define	STX		0xAA
#define ETX		0xBB



using namespace std;

class RfidDevice {
private:
	libusb_device_handle * _handler;
	unsigned char Buffer[DATALEN];
	int p = 8;
	int CheckData(unsigned char *data, int h, int t);
	void ClearBuffer();
	void WriteBuffer(unsigned char data);
	void WriteBuffers(unsigned char *data, int length);
	int SendCommand(int command, unsigned char *sDATA, int sDLen, unsigned char *rDATA, int*Statue);
	int SendData();
public:
	// Connect to the RFID device.
	boolean connect(uint16_t vendorId, uint16_t productId);
	int ControlLed(unsigned char freq, unsigned char duration, unsigned char* buffer);
	int ControlBuzzer(unsigned char freq, unsigned char duration, unsigned char *buffer);
	int GetVersionNumber(unsigned char *buffer);
};

