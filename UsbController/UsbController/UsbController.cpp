// UsbController.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <vector>
#include <iostream>
#include <istream>
#include <sstream>
#include <libusb.h>
#include "RfidDevice.h"

using namespace std;

#define DATALEN 0x100;
#define STX 0xAA;
#define ETX 0xBB;
#define Control_Buzzer	0x89;

void printdev(libusb_device *dev) {
	libusb_device_descriptor desc;
	int r = libusb_get_device_descriptor(dev, &desc);
	if (r < 0) {
		cout << "failed to get device descriptor" << endl;
		return;
	}

	cout << "Number of possible configurations: " << (int)desc.bNumConfigurations << "  ";
	cout << "Device Class: " << (int)desc.bDeviceClass << "  ";
	cout << "VendorID: " << desc.idVendor << "  ";
	cout << "ProductID: " << desc.idProduct << endl;
	cout << "device sub class " << (int)desc.bDeviceSubClass << endl;
}

int main()
{
	bool isConnected;
	unsigned char* verNumbers = new unsigned char[0x100];
	unsigned char* serNumbers = new unsigned char[0x100];
	RfidDevice* rfidDevice = new RfidDevice();
	isConnected = rfidDevice->connect(65535, 53);
	if (!isConnected) {
		cout << "the device is not connected" << endl;
		return 1;
	}
	
	cout << "the device is connected :) " << endl;

	unsigned char buffer;
	// rfidDevice->ControlBuzzer(0x0a, 0x0a, &buffer);
	// 1. Read the version number
	int verLength = rfidDevice->GetVersionNumber(verNumbers);
	if (verLength <= 0)
	{
		cout << "the serial number cannot be read" << endl;
	}
	else
	{
		printf("Version number : ");
		for (int i = 0; i < verLength; i++)
			printf("%02x ", verNumbers[i]);
		printf("\n");
	}

	// 2. Get the serial number
	int serLength = rfidDevice->GetSerialNumber(serNumbers);
	if (serLength <= 0) 
	{
		cout << "the serial number cannot be read" << endl;
	}
	else
	{
		printf("Serial number : ");
		for (int i = 0; i < serLength; i++)
			printf("%02x ", serNumbers[i]);
		printf("\n");
	}

	string input;
	getline(cin, input);

	/*
	libusb_device** devs;
	libusb_context *ctx = NULL;
	libusb_device_handle * devHandler = NULL;
	int r;
	ssize_t cnt;
	r = libusb_init(&ctx);
	if (r < 0) {
		cout << "Init error " << r << endl;
		return 1;
	}

	libusb_set_debug(ctx, 3);
	cnt = libusb_get_device_list(ctx, &devs);
	if (cnt < 0) {
		cout << "Get device error" << endl;
		return 1;
	}

	cout << cnt << " Devices in list." << endl;
	ssize_t i;
	for (i = 0; i < cnt; i++) {
		printdev(devs[i]);
	}

	devHandler = libusb_open_device_with_vid_pid(ctx, 65535, 53);
	if (devHandler == NULL) {
		cout << "Cannot open the device" << endl;
		return 1;
	}

	libusb_reset_device(devHandler);
	libusb_free_device_list(devs, 1); //free the list, unref the devices in it	
	r = libusb_claim_interface(devHandler, 0);
	if (r < 0) {
		cout << "cannot claim interface" << endl;
		return 1;
	}

	int actual;
	unsigned char* data = new unsigned char[0x100];
	int ind = 0;
	for (ind = 0; ind < 256; ind++) {
		data[ind] = 0x00;
	}
	data[0] = 0x01; data[6] = 0x08; data[8] = 0xAA; data[10] = 0x03; data[11] = Control_Buzzer; data[12] = 0x12;
	data[13] = 0x09; data[14] = 0x91; data[15] = 0xBB;
	printf("Send data:");
	for (ind = 0; ind<256; ind++)
		printf("%02x ", data[ind]);
	printf("\n");

	r = libusb_control_transfer(devHandler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_OUT,
		LIBUSB_REQUEST_SET_CONFIGURATION,
		0x301,
		0,
		data,
		0x100,
		500);
	if (r != 0x100) {
		cout << "error " << endl;
	}

	string input = "";
	getline(cin, input);*/
    return 0;
}

