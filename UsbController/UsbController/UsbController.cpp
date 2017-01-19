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
}

int main()
{
	/*
	bool isConnected;
	RfidDevice* rfidDevice = NULL;
	rfidDevice = new RfidDevice();
	isConnected = rfidDevice->connect(65535, 53);
	if (!isConnected) {
		cout << "the device is not connected" << endl;
		return 1;
	}
	
	cout << "the device is connected :) " << endl;

	unsigned char buffer;
	rfidDevice->ControlBuzzer(0x0a, 0x0a, &buffer);
	string input;
	getline(cin, input);
	*/

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

	libusb_free_device_list(devs, 1); //free the list, unref the devices in it	
	r = libusb_claim_interface(devHandler, 0);
	if (r < 0) {
		cout << "cannot claim interface" << endl;
		return 1;
	}

	int actual;
	unsigned char* data = new unsigned char[8];
	data[0] = 0x01; data[1] = 0x00; data[2] = 0x03; data[3] = Control_Buzzer; data[4] = 0x0A;
	data[5] = 0x0A; data[6] = 0x8A; data[7] = 0xBB;
	r = libusb_control_transfer(devHandler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_OUT,
		LIBUSB_REQUEST_SET_CONFIGURATION,
		0x301,
		0,
		data,
		8,
		500);
	r = libusb_control_transfer(
		devHandler,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_IN,
		LIBUSB_REQUEST_CLEAR_FEATURE,
		0x302,
		0,
		data,
		8,
		500);
	if (r != 0x100) {
		cout << "error " << endl;
	}

	string input = "";
	getline(cin, input);
    return 0;
}

