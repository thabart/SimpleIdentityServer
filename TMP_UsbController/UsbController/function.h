//System Setting

int API_SetSerNum(unsigned char *newValue, unsigned char *buffer);

int API_GetSerNum(unsigned char *buffer);

int WriteUserInfo(int num_blk, int num_length, char *user_info);

int ReadUserInfo(int num_blk, int num_length, char *user_info);

int GetVersionNum(char *VersionNum);

int API_ControlLED(unsigned char freq, unsigned char duration, unsigned char *buffer);

int API_ControlBuzzer(unsigned char freq, unsigned char duration, unsigned char *buffer);

//ISO14443A
int MF_Request(unsigned char inf_mode, unsigned char *buffer);

int MF_Anticoll(unsigned char *snr, unsigned char *tatus);

int MF_Select(unsigned char *snr);

int MF_Halt(void);

int API_PCDInitVal(unsigned char mode, unsigned char SectNum, unsigned char *snr, unsigned char *value);

int API_PCDDec(unsigned char mode, unsigned char SectNum, unsigned char *snr, unsigned char *value);

int API_PCDInc(unsigned char   mode, unsigned char SectNum, unsigned char *snr, unsigned char *value);

int GET_SNR(unsigned char mode, unsigned char API_halt, unsigned char *snr, unsigned char*value);

//ISO14443B
int RequestType_B(unsigned char *buffer);

int AntiType_B(unsigned char *buffer);

int SelectType_B(unsigned char*SerialNum);

int Request_AB(unsigned char* buffer);

int API_ISO14443TypeBTransCOSCmd(unsigned char *cmd, int cmdSize, unsigned char *buffer);

//ISO15693
int API_ISO15693_Inventory(unsigned char flag, unsigned char afi, unsigned char *pData, unsigned char *nrOfCard, unsigned char *pBuffer);

int API_ISO15693Read(unsigned char flags, unsigned char blk_add, unsigned char num_blk, unsigned char *uid, unsigned char *buffer);

int API_ISO15693Write(unsigned char flags, unsigned char blk_add, unsigned char num_blk, unsigned char *uid, unsigned char *data);

int API_ISO15693Lock(unsigned char flags, unsigned char num_blk, unsigned char *uid, unsigned char *buffer);

int API_ISO15693StayQuiet(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_ISO15693Select(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_ResetToReady(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_WriteAFI(unsigned char flags, unsigned char afi, unsigned char *uid, unsigned char *buffer);

int API_LockAFI(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_WriteDSFID(unsigned char flags, unsigned char DSFID, unsigned char *uid, unsigned char *buffer);

int API_LockDSFID(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_ISO15693_GetSysInfo(unsigned char flags, unsigned char *uid, unsigned char *buffer);

int API_ISO15693_GetMulSecurity(unsigned char flags, unsigned char blkAddr, unsigned char blkNum, unsigned char  *uid, unsigned char *pBuffer);

int API_ISO15693TransCOSCmd(unsigned char *cmd, int cmdSize, unsigned char *buffer);

//ultralight
int UL_HLRead(unsigned char mode, unsigned char blk_add, unsigned char *snr, unsigned char *buffer);

int	UL_HLWrite(unsigned char mode, unsigned char blk_add, unsigned char *snr, unsigned char *buffer);

int UL_Request(unsigned char mode, unsigned char *snr);