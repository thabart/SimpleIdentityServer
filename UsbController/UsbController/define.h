//ISO14443A
#define  REQA			0x03
#define  Anticoll_A		0x04
#define  Select_A		0x05
#define  Halt_A			0x06

//ISO14443B
#define  ReqB								0x09
#define  AnticollB							0x0A
#define  Attrib_TypeB						0x0B
#define  Rst_TypeB							0x0C
#define  ISO14443_TypeB_Transfer_Command	0x0D

//MF

#define MF_Read							0x20
#define MF_Write						0x21
#define MF_InitVal						0x22
#define MF_Decrement					0x23
#define MF_Increment					0x24
#define	MF_GET_SNR						0x25
#define ISO14443_TypeA_Transfer_Command	0x28

//ISO15693
#define ISO15693_Inventory						0x10
#define ISO15693_Read							0x11
#define ISO15693_Write							0x12
#define ISO15693_Lockblock						0x13
#define ISO15693_StayQuiet						0x14
#define ISO15693_Select							0x15
#define ISO15693_Resetready						0x16
#define ISO15693_Write_Afi						0x17
#define ISO15693_Lock_Afi						0x18
#define ISO15693_Write_Dsfid					0x19
#define ISO15693_Lock_Dsfid						0x1A
#define ISO15693_Get_Information				0x1B
#define ISO15693_Get_Multiple_Block_Security	0x1C
#define ISO15693_Transfer_Command				0x1D

//ultralight
#define CMD_UL_HLRead  0xE0
#define CMD_UL_HLWrite 0xE1
#define CMD_UL_Request 0xE3

//system setting
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

#define	STX		0xAA
#define ETX		0xBB

#define PID		0xffff
#define VID		0x0035