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

int RfidDevice::GetVersionNumber(unsigned char *buffer)
{
	int Statue;
	int result = SendCommand(Get_VersionNum, NULL, 0, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int RfidDevice::CheckData(unsigned char *data, int h, int t)
{
	int check = data[h];
	for (int i = h + 1; i <= t; i++)
	{
		check = check^data[i];
	}

	return check;
}

void RfidDevice::ClearBuffer()
{
	int i;
	for (i = 0; i < 8; i++)
	{
		Buffer[i] = 0x00;
	}

	p = 8;
	Buffer[0] = 0x01;
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
	int result, length, BCC, i, receive;
	ClearBuffer();
	WriteBuffer(STX);
	WriteBuffer(0x00);
	WriteBuffer(sDLen + 1);
	WriteBuffer(command);
	WriteBuffers(sDATA, sDLen);
	BCC = CheckData(Buffer, 10, 10 + sDLen + 1);
	WriteBuffer(BCC);
	WriteBuffer(ETX);
	Buffer[6] = p - 8;
	// SEND DATA
	SendData();
	// READ DATA
	return GetData(rDATA);
}

int RfidDevice::SendData()
{
	// SEND DATA
	printf("Send data:");
	for (int i = 0; i < DATALEN; i++)
		printf("%02x ", Buffer[i]);
	printf("\n");
	return libusb_control_transfer(
		_handler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_OUT,
		LIBUSB_REQUEST_SET_CONFIGURATION,
		0x301,
		0,
		Buffer,
		DATALEN,
		500);
	/*
	printf("Receive data:");
	for (i = 0; i<DATALEN; i++)
		printf("%02x ", Buffer[i]);
	printf("\n");
	return 0;*/
}

int RfidDevice::GetData(unsigned char *rDATA)
{
	return libusb_control_transfer(
		_handler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_IN,
		LIBUSB_REQUEST_CLEAR_FEATURE,
		0x302,
		0,
		rDATA,
		DATALEN,
		500);
}