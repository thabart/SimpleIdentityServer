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
	WriteCommand(Control_Led2, DATA, 2);
	return SendData();
}

int RfidDevice::ControlBuzzer(unsigned char freq, unsigned char duration, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = freq;
	DATA[1] = duration;
	WriteCommand(Control_Buzzer, DATA, 2);
	return SendData();
}

int RfidDevice::GetVersionNumber(unsigned char * rData)
{
	WriteCommand(Get_VersionNum, NULL, 0);
	SendData();
	return GetData(rData);
}

int RfidDevice::GetSerialNumber(unsigned char* rData) {
	WriteCommand(GetSerlNum, NULL, 0);
	SendData();
	return GetData(rData);
}

int RfidDevice::GetCardNumber(unsigned char mode, unsigned char apiHalt, unsigned char* rData)
{
	unsigned char data[2];
	data[0] = mode;
	data[1] = apiHalt;
	WriteCommand(MF_GET_SNR, data, 2);
	SendData();
	return GetData(rData);
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

void RfidDevice::WriteCommand(int command, unsigned char *sDATA, int sDLen)
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

int RfidDevice::GetData(unsigned char * rData)
{
	int startIndex = 13;
	unsigned char * result = new unsigned char[DATALEN];
	int length = libusb_control_transfer(
		_handler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_IN,
		LIBUSB_REQUEST_CLEAR_FEATURE,
		0x302,
		0,
		result,
		DATALEN,
		500);
	if (length < 0)
	{
		return NULL;
	}

	char dataLength = result[10] - 2;
	int index = 0;
	for (int indice = startIndex; indice < dataLength + startIndex; indice++)
	{
		rData[index] = result[indice];
		index++;
	}

	printf("Receive data:");
	for (int i = 0; i<length; i++)
		printf("%02x ", result[i]);
	printf("\n");

	return dataLength;
}