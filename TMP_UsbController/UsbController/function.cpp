#include    <stdio.h>
#include    <stdlib.h> 
#include    <sys/types.h>
#include    <libusb.h>
#include    <sys/stat.h>
#include    <fcntl.h> 
#include    <errno.h>
#include    "function.h"
#include    "define.h"
#include "stdafx.h"
#define DATALEN 0x100
#define FALSE -1
#define TRUE 0

unsigned char Data[DATALEN] = { 1 };
unsigned char *Buffer = &Data[8];
int p = 1;
libusb_device_handle *devs;

int openCom(void)
{
	int result;
	result = libusb_init(NULL);
	if (result < 0)
	{
		printf("libusb init error!\n");
		return FALSE;
	}
	devs = libusb_open_device_with_vid_pid(NULL, PID, VID);
	if (devs == NULL)
	{
		printf("libusb open failed!\n");
		return FALSE;
	}
	result = libusb_reset_device(devs);
	if (result != 0)
	{
		printf("libusb reset failed!\n");
		return FALSE;
	}
	return TRUE;
}

void closeCom(void)
{
	libusb_reset_device(devs);
	libusb_close(devs);
	libusb_exit(NULL);
}
void copyData(unsigned char *s, int spos, unsigned char *d, int dpos, int len)
{
	int i;
	for (i = 0; i<len; i++)
	{
		d[i + dpos] = s[i + spos];
	}
}
int checkData(unsigned char *data, int h, int t)
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

void writeBuffer(unsigned char data)
{
	Buffer[p] = data;
	p++;
}

void writeBuffers(unsigned char *data, int length)
{
	int i;
	for (i = 0; i<length; i++)
		writeBuffer(data[i]);
}

void clearBuffer(void)
{
	int i;
	Data[0] = 0x01;
	for (i = 0; i<8; i++)
		Data[i] = 0x00;
	p = 1;
	Buffer[0] = STX;
}

int writeCom(unsigned char *data, int length)
{
	int recieve;
	data[6] = length;
	recieve = libusb_control_transfer(
		devs,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_OUT,
		LIBUSB_REQUEST_SET_CONFIGURATION,
		0x301,
		0,
		data,
		DATALEN,
		500);
	if (recieve<0 || recieve != DATALEN)
		return -1;
	return length;
}

int readCom(unsigned char *data, int length)
{
	int recieve;
	recieve = libusb_control_transfer(
		devs,
		LIBUSB_RECIPIENT_INTERFACE | LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_ENDPOINT_IN,
		LIBUSB_REQUEST_CLEAR_FEATURE,
		0x302,
		0,
		data,
		DATALEN,
		500);
	if (recieve<0)
		return -1;
	return recieve - 8;
}

int sendData(void)
{
	int length;
	int BCC;
	int i;
	BCC = checkData(Buffer, 1, p - 1);
	writeBuffer(BCC);
	writeBuffer(ETX);

	printf("Send data:");
	for (i = 0; i<p; i++)
		printf("%02x ", Buffer[i]);
	printf("\n");

	if (openCom() == FALSE)
	{
		closeCom();
		return FALSE;
	}
	length = writeCom(Data, p);
	printf("length send: %02x\n", length);

	if (length != p)
		return 0x05;

	length = readCom(Data, 248);

	closeCom();

	printf("length read: %02x\n", length);
	/*
	if(length != recieve_len)
	return 0x05;*/

	p = length;

	printf("Recieve data:");
	for (i = 0; i<length; i++)
		printf("%02x ", Buffer[i]);
	printf("\n");

	if (p<6 || Buffer[0] != STX || Buffer[p - 1] != ETX || Buffer[2] + 5 != p)
		return 0x05;
	if (checkData(Buffer, 1, p - 3) != Buffer[p - 2])
		return 0x02;
	return 0;
}

int sendCommand(int command, unsigned char *sDATA, int sDLen, unsigned char *rDATA, int*Statue)
{
	int result;
	clearBuffer();
	writeBuffer(0x00);
	writeBuffer(sDLen + 1);
	writeBuffer(command);
	writeBuffers(sDATA, sDLen);

	result = sendData();

	if (result != 0)
		return result;
	copyData(Buffer, 4, rDATA, 0, Buffer[2] - 1);
	*Statue = Buffer[3];
	return 0;
}

int  API_SetSerNum(unsigned char *newValue, unsigned char *buffer)
{
	int Statue;
	int result = sendCommand(SetSerlNum, newValue, 8, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int  API_GetSerNum(unsigned char *buffer)
{
	int Statue;
	int result = sendCommand(GetSerlNum, NULL, 0, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int  WriteUserInfo(int num_blk, int num_length, char *user_info)
{
	int Statue;
	unsigned char DATA[121];
	DATA[0] = num_blk;
	DATA[1] = num_length;
	copyData((unsigned char*)user_info, 0, DATA, 2, num_length);
	int result = sendCommand(Write_UserInfo, DATA, num_length + 2, (unsigned char*)user_info, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int ReadUserInfo(int num_blk, int num_length, char *user_info)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = num_blk;
	DATA[1] = num_length;
	int result = sendCommand(Read_UserInfo, DATA, 2, (unsigned char*)user_info, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int GetVersionNum(char *VersionNum)
{
	int Statue;
	int result = sendCommand(Get_VersionNum, NULL, 0, (unsigned char*)VersionNum, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int  API_ControlLED(unsigned char freq, unsigned char duration, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = freq;
	DATA[1] = duration;
	int result = sendCommand(Control_Led2, DATA, 2, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int API_ControlBuzzer(unsigned char freq, unsigned char duration, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[2];
	DATA[0] = freq;
	DATA[1] = duration;
	int result = sendCommand(Control_Buzzer, DATA, 2, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int  MF_Request(unsigned char inf_mode, unsigned char *buffer)
{
	int Statue;
	/*
	unsigned char DATA[1];
	if (inf_mode == 0x00)
	DATA[0] = 0x52;
	else
	DATA[0] = 0x26;
	*/
	int result = sendCommand(REQA, &inf_mode, 1, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int MF_Anticoll(unsigned char *snr, unsigned char *status)
{
	int Statue;
	unsigned char DATA[5];
	int result = sendCommand(Anticoll_A, NULL, 0, DATA, &Statue);
	if (result != 0)
		return result;
	*status = DATA[0];
	copyData(DATA, 1, snr, 0, 4);
	return Statue;
}


int MF_Select(unsigned char *snr)
{
	int Statue;
	int result = sendCommand(Select_A, snr, 4, snr, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int MF_Halt()
{
	int Statue;
	unsigned char error;
	int result = sendCommand(Halt_A, NULL, 0, &error, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int API_PCDInitVal(unsigned char mode, unsigned char SectNum, unsigned char *snr, unsigned char *value)
{
	int Statue;
	unsigned char DATA[12];
	DATA[0] = mode;
	DATA[1] = SectNum;
	copyData(snr, 0, DATA, 2, 6);
	copyData(value, 0, DATA, 8, 4);
	int result = sendCommand(MF_InitVal, DATA, 12, DATA, &Statue);
	if (result != 0)
		return result;
	copyData(DATA, 0, snr, 0, 4);
	return Statue;
}



int  API_PCDDec(unsigned char mode, unsigned char SectNum, unsigned char *snr, unsigned char *value)
{
	int Statue;
	unsigned char DATA[12];
	DATA[0] = mode;
	DATA[1] = SectNum;
	copyData(snr, 0, DATA, 2, 6);
	copyData(value, 0, DATA, 8, 4);
	int result = sendCommand(MF_Decrement, DATA, 12, DATA, &Statue);
	if (result != 0)
		return result;
	copyData(DATA, 0, snr, 0, 4);
	copyData(DATA, 4, value, 0, 4);
	return Statue;
}

int  API_PCDInc(unsigned char   mode, unsigned char SectNum, unsigned char *snr, unsigned char *value)
{
	int Statue;
	unsigned char DATA[12];
	DATA[0] = mode;
	DATA[1] = SectNum;
	copyData(snr, 0, DATA, 2, 6);
	copyData(value, 0, DATA, 8, 4);
	int result = sendCommand(MF_Increment, DATA, 12, DATA, &Statue);
	if (result != 0)
		return result;
	copyData(DATA, 0, snr, 0, 4);
	copyData(DATA, 4, value, 0, 4);
	return Statue;
}

int GET_SNR(unsigned char mode, unsigned char API_halt, unsigned char *snr, unsigned char*value)
{
	int Statue;
	unsigned char DATA[5];
	DATA[0] = mode;
	DATA[1] = API_halt;
	int result = sendCommand(MF_GET_SNR, DATA, 2, DATA, &Statue);
	if (result != 0)
		return result;
	snr[0] = DATA[0];
	copyData(DATA, 1, value, 0, 4);
	return Statue;
}


int RequestType_B(unsigned char *buffer)
{
	int Statue;
	int result = sendCommand(ReqB, NULL, 0, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int AntiType_B(unsigned char *buffer)
{
	int Statue;
	int result = sendCommand(AnticollB, NULL, 0, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int SelectType_B(unsigned char*SerialNum)
{
	int Statue;
	int result = sendCommand(Attrib_TypeB, SerialNum, 4, SerialNum, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int Request_AB(unsigned char* buffer)
{
	int Statue;
	int result = sendCommand(Rst_TypeB, buffer, 4, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int  API_ISO14443TypeBTransCOSCmd(unsigned char *cmd, int cmdSize, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[256];
	DATA[0] = cmdSize;
	copyData(buffer, 0, DATA, 1, cmdSize);
	int result = sendCommand(ISO14443_TypeB_Transfer_Command, buffer, cmdSize + 1, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int  API_ISO15693_Inventory(unsigned char flag, unsigned char afi, unsigned char *pData, unsigned char *nrOfCard, unsigned char *pBuffer)
{
	int Statue;
	unsigned char DATA[256];
	DATA[0] = flag;
	DATA[1] = afi;
	DATA[2] = 0;
	copyData(pData, 0, DATA, 3, 8);
	int result = sendCommand(ISO14443_TypeB_Transfer_Command, DATA, 11, DATA, &Statue);
	if (result != 0)
		return result;
	*nrOfCard = DATA[0];
	copyData(DATA, 1, pBuffer, 0, 8 * DATA[0]);
	return Statue;
}


int  API_ISO15693Read(unsigned char flags, unsigned char blk_add, unsigned char num_blk, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[11];
	DATA[0] = flags;
	DATA[1] = blk_add;
	DATA[2] = num_blk;
	num = 3;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 3, 8);
		num = 11;
	}
	int result = sendCommand(ISO15693_Read, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int  API_ISO15693Write(unsigned char flags, unsigned char blk_add, unsigned char num_blk, unsigned char *uid, unsigned char *data)
{
	int Statue;
	int num, k;
	unsigned char DATA[256];
	k = 4;
	DATA[0] = flags;
	DATA[1] = blk_add;
	DATA[2] = num_blk;
	num = num_blk*k + 3;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 3, 8);
		num = num_blk*k + 11;
	}
	copyData(data, 0, DATA, 3, num_blk * 4);
	int result = sendCommand(ISO15693_Write, DATA, num, data, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693Lock(unsigned char flags, unsigned char num_blk, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[10];
	DATA[0] = flags;
	DATA[1] = num_blk;
	num = 2;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 2, 8);
		num = 10;
	}
	int result = sendCommand(ISO15693_Lockblock, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693StayQuiet(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[9];
	DATA[0] = flags;
	copyData(uid, 0, DATA, 1, 8);
	int result = sendCommand(ISO15693_StayQuiet, DATA, 9, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693Select(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[9];
	DATA[0] = flags;
	copyData(uid, 0, DATA, 1, 8);
	int result = sendCommand(ISO15693_Select, DATA, 9, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ResetToReady(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[9];
	DATA[0] = flags;
	num = 1;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 1, 8);
		num = 9;
	}
	int result = sendCommand(ISO15693_Resetready, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int  API_WriteAFI(unsigned char flags, unsigned char afi, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[10];
	DATA[0] = flags;
	DATA[1] = afi;
	num = 2;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 2, 8);
		num = 10;
	}
	int result = sendCommand(ISO15693_Write_Afi, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_LockAFI(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[9];
	DATA[0] = flags;
	num = 1;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 1, 8);
		num = 9;
	}
	int result = sendCommand(ISO15693_Lock_Afi, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_WriteDSFID(unsigned char flags, unsigned char DSFID, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[10];
	DATA[0] = flags;
	DATA[1] = DSFID;
	num = 2;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 2, 8);
		num = 10;
	}
	int result = sendCommand(ISO15693_Write_Dsfid, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_LockDSFID(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[9];
	DATA[0] = flags;
	num = 1;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 1, 8);
		num = 9;
	}
	int result = sendCommand(ISO15693_Lock_Dsfid, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693_GetSysInfo(unsigned char flags, unsigned char *uid, unsigned char *buffer)
{
	int Statue;
	int num;
	unsigned char DATA[9];
	DATA[0] = flags;
	num = 1;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 1, 8);
		num = 9;
	}
	int result = sendCommand(ISO15693_Get_Information, DATA, num, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693_GetMulSecurity(unsigned char flags, unsigned char blkAddr, unsigned char blkNum, unsigned char  *uid, unsigned char *pBuffer)
{
	int Statue;
	int num;
	unsigned char DATA[11];
	DATA[0] = flags;
	DATA[1] = blkAddr;
	DATA[2] = blkNum;
	num = 3;
	if (flags == 0x22)
	{
		copyData(uid, 0, DATA, 3, 8);
		num = 11;
	}
	int result = sendCommand(ISO15693_Get_Multiple_Block_Security, DATA, num, pBuffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int API_ISO15693TransCOSCmd(unsigned char *cmd, int cmdSize, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[256];
	DATA[0] = cmdSize;
	copyData(buffer, 0, DATA, 1, cmdSize);
	int result = sendCommand(ISO15693_Transfer_Command, DATA, cmdSize, buffer, &Statue);
	if (result != 0)
		return result;
	return Statue;
}

int	UL_HLRead(unsigned char mode, unsigned char blk_add, unsigned char *snr, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[23];
	DATA[0] = mode;
	DATA[1] = blk_add;
	int result = sendCommand(CMD_UL_HLRead, DATA, 2, DATA, &Statue);
	if (result != 0)
		return result;
	copyData(DATA, 0, buffer, 0, 16);
	copyData(DATA, 16, snr, 0, 7);
	return Statue;
}


int UL_HLWrite(unsigned char mode, unsigned char blk_add, unsigned char *snr, unsigned char *buffer)
{
	int Statue;
	unsigned char DATA[7];
	DATA[0] = mode;
	DATA[1] = 1;
	DATA[2] = blk_add;
	copyData(buffer, 0, DATA, 3, 4);
	int result = sendCommand(CMD_UL_HLWrite, DATA, 7, snr, &Statue);
	if (result != 0)
		return result;
	return Statue;
}


int UL_Request(unsigned char mode, unsigned char *snr)
{
	int Statue;
	unsigned char DATA[1];
	DATA[0] = mode;
	int result = sendCommand(CMD_UL_Request, DATA, 1, snr, &Statue);
	if (result != 0)
		return result;
	return Statue;
}
