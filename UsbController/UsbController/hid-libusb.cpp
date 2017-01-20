/*******************************************************
HIDAPI - Multi-Platform library for
communication with HID devices.

Alan Ott
Signal 11 Software

8/22/2009
Linux Version - 6/2/2010
Libusb Version - 8/13/2010

Copyright 2009, All Rights Reserved.

At the discretion of the user of this library,
this software may be licensed under the terms of the
GNU Public License v3, a BSD-Style license, or the
original HIDAPI license as outlined in the LICENSE.txt,
LICENSE-gpl3.txt, LICENSE-bsd.txt, and LICENSE-orig.txt
files located at the root of the source distribution.
These files may also be found in the public source
code repository located at:
http://github.com/signal11/hidapi .
********************************************************/

#define _GNU_SOURCE // needed for wcsdup() before glibc 2.10

/* C */
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <ctype.h>
#include <locale.h>
#include <errno.h>

/* Unix */
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <wchar.h>

/* GNU / LibUSB */
#include "libusb.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifdef DEBUG_PRINTF
#define LOG(...) fprintf(stderr, __VA_ARGS__)
#else
#define LOG(...) do {} while (0)
#endif



	struct hid_device_info  HID_API_EXPORT * hid_enumerate(unsigned short vendor_id, unsigned short product_id)
	{
		libusb_device **devs;
		libusb_device *dev;
		libusb_device_handle *handle;
		ssize_t num_devs;
		int i = 0;

		struct hid_device_info *root = NULL; // return object
		struct hid_device_info *cur_dev = NULL;

		if (!initialized)
			hid_init();

		num_devs = libusb_get_device_list(NULL, &devs);
		if (num_devs < 0)
			return NULL;
		while ((dev = devs[i++]) != NULL) {
			struct libusb_device_descriptor desc;
			struct libusb_config_descriptor *conf_desc = NULL;
			int j, k;
			int interface_num = 0;

			int res = libusb_get_device_descriptor(dev, &desc);
			unsigned short dev_vid = desc.idVendor;
			unsigned short dev_pid = desc.idProduct;

			/* HID's are defined at the interface level. */
			if (desc.bDeviceClass != LIBUSB_CLASS_PER_INTERFACE)
				continue;

			res = libusb_get_active_config_descriptor(dev, &conf_desc);
			if (res < 0)
				libusb_get_config_descriptor(dev, 0, &conf_desc);
			if (conf_desc) {
				for (j = 0; j < conf_desc->bNumInterfaces; j++) {
					const struct libusb_interface *intf = &conf_desc->interface[j];
					for (k = 0; k < intf->num_altsetting; k++) {
						const struct libusb_interface_descriptor *intf_desc;
						intf_desc = &intf->altsetting[k];
						if (intf_desc->bInterfaceClass == LIBUSB_CLASS_HID) {
							interface_num = intf_desc->bInterfaceNumber;

							/* Check the VID/PID against the arguments */
							if ((vendor_id == 0x0 && product_id == 0x0) ||
								(vendor_id == dev_vid && product_id == dev_pid)) {
								struct hid_device_info *tmp;

								/* VID/PID match. Create the record. */
								tmp = calloc(1, sizeof(struct hid_device_info));
								if (cur_dev) {
									cur_dev->next = tmp;
								}
								else {
									root = tmp;
								}
								cur_dev = tmp;

								/* Fill out the record */
								cur_dev->next = NULL;
								cur_dev->path = make_path(dev, interface_num);

								res = libusb_open(dev, &handle);

								if (res >= 0) {
									/* Serial Number */
									if (desc.iSerialNumber > 0)
										cur_dev->serial_number =
										get_usb_string(handle, desc.iSerialNumber);

									/* Manufacturer and Product strings */
									if (desc.iManufacturer > 0)
										cur_dev->manufacturer_string =
										get_usb_string(handle, desc.iManufacturer);
									if (desc.iProduct > 0)
										cur_dev->product_string =
										get_usb_string(handle, desc.iProduct);

									libusb_close(handle);
								}
								/* VID/PID */
								cur_dev->vendor_id = dev_vid;
								cur_dev->product_id = dev_pid;

								/* Release Number */
								cur_dev->release_number = desc.bcdDevice;

								/* Interface Number */
								cur_dev->interface_number = interface_num;
							}
						}
					} /* altsettings */
				} /* interfaces */
				libusb_free_config_descriptor(conf_desc);
			}
		}

		libusb_free_device_list(devs, 1);

		return root;
	}

}
