#include <iostream>
#include <libusb.h>
#include "RfidDevice.h"

using namespace std;


boolean RfidDevice::connect(uint16_t vendorId, uint16_t productId) {
	int result;
	result = libusb_init(NULL);
	if (result < 0) {
		return false;
	}

	_handler = libusb_open_device_with_vid_pid(NULL, vendorId, productId);
	if (_handler == NULL) {
		return false;
	}

	result = libusb_reset_device(_handler);
	if (result != 0) {
		return false;
	}

	return true;
}

int RfidDevice::ControlLed(unsigned char freq, unsigned char duration, unsigned char* buffer)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = freq;
	DATA[1] = duration;
	int result = SendCommand(Control_Led2, DATA, 2, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int RfidDevice::ControlBuzzer(unsigned char freq, unsigned char duration, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = freq;
	DATA[1] = duration;
	int result = SendCommand(Control_Buzzer, DATA, 2, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int RfidDevice::CheckData(unsigned char *data, int h, int t)
{
	int check;
	int i;
	check = data[h];
	for (i = h + 1; i <= t; i++)
	{
		check = check^data[i];
	}
	return check;
}

void RfidDevice::ClearBuffer()
{
	int i;
	Data[0] = 0x01;
	for (i = 0; i < 8; i++)
	{
		Data[i] = 0x00;
	}

	p = 1;
	Buffer[0] = 0x01;
	// Buffer[0] = STX;
}

void RfidDevice::WriteBuffer(unsigned char data)
{
	Buffer[p] = data;
	p++;
}

void RfidDevice::WriteBuffers(unsigned char *data, int length)
{
	int i;
	for (i = 0; i < length; i++)
	{
		WriteBuffer(data[i]);
	}
}

int RfidDevice::SendCommand(int command, unsigned char *sDATA, int sDLen, unsigned char *rDATA, int*Statue)
{
	int result;
	ClearBuffer();
	WriteBuffer(0x00);
	WriteBuffer(sDLen + 1);
	WriteBuffer(command);
	WriteBuffers(sDATA, sDLen);
	SendData();
	return 0;
}

int RfidDevice::SendData()
{
	int length, BCC, i, receive;
	BCC = CheckData(Buffer, 1, p - 1);
	WriteBuffer(BCC);
	WriteBuffer(ETX);
	Data[6] = p;

	printf("Send data:");
	for (i = 0; i<p; i++)
		printf("%02x ", Buffer[i]);
	printf("\n");
	receive = libusb_control_transfer(
		_handler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_OUT,
		LIBUSB_REQUEST_SET_CONFIGURATION,
		0x301,
		0,
		Data,
		DATALEN,
		500);
	return 0;
}