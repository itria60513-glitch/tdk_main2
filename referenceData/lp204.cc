
#define _UnclampInE84
//#define _OfflineTest
#define _REENTRANT
#include <stdio.h>
#include <sys/time.h>
#include <unistd.h>
#include <stdlib.h>
#include <time.h>
#include <dirent.h>
#include <string.h>
#include <sys/stat.h>
#include <pthread.h>
#include <semaphore.h>
#include <fcntl.h>
#include <errno.h>
#include <termios.h>
#include "seaio.h"
#include <exception>
#include <memory>
#include <sys/utsname.h>
#include <math.h>
using namespace std;


///////////////////////////////////////////////////////////
//Global variable: initialized at start up from config file
int _LP1IdReader = 2;//1: use barcode; 2: use Hermos RFID; 3: use Omron RFID(ASCII); 4: use Omron RFID(HEX)
int _LP2IdReader = 2;//1: use barcode; 2: use Hermos RFID; 3: use Omron RFID(ASCII); 4: use Omron RFID(HEX)

int _LTCEnDis = 0;//0: Disable; 1: Enable-InTransfer;  2:Enable-Always
int _LTCOnLevel = 1;//0: 0V for ON; 1: 24V for ON

int _ESMode = 0;//0: Normal Mode; 1: Always ON
int _PreservE84FieldSignal = 1;//0: No preserve; 1: Preserve
int _SpecialRequirement = 0;//REQ_NORMAL, no special requirement.
int _FoupStable_debouce = 9;//stabel time about 1 second
int _SBCVersion = -1;//1: Original SBC; 2: NEW SBC; -1: cannot recognize.

bool _WriteLog = true;//true;

unsigned int update_timeout = 0;

int _N2PurgeNozzleDown_InE84 = 0;//For Intel N2 Purge Requirement
int msgMaxRetries = 10;
bool m_readBcodeFinish = false;
//End Global variable
////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////
//W_E84OUTPUT SOURCE
#define SRC_HOST_out_e84_Brkout_e84		0
#define SRC_HOST_ene84_DisableAMHS		1
#define SRC_LOCAL_AmhsThreadBody		2

////////////////////////////////////////////////////////////
//Special Customers
#define REQ_NORMAL		0
#define REQ_REXCHIP		1

////////////////////////////////////////////////////////////
//m_mchStatus MASK definations
#define BIT0_MCH_DOOR	1
#define BIT1_MCH	2
#define BIT2_MCH	4
#define BIT3_MCH	8
#define BIT4_MCH	16
#define BIT5_MCH	32
#define BIT6_MCH	64
#define BIT7_MCH	128

///////////////////////////////////////////////////////////////////////////
//Special event definitions
//1. event 0x8000: TAS300 Error code, also controll program start up event.
//2. event 0x8001: TAS300 Status code;
//	 event 0x8002: TAS300 Operation return code.
//	 event 0x8003: TAS300 N2 Purge status code.
//	 event 0x8004: TAS300 N2 Purge PGWFL event.
//3. event 0x8010: Programm update and compile error code.
//4. event 0x8021: Barcode reading error code: 
//					0x01 for Motor ON failure;
//					0x02 for Reading error, usually time out;
//					0x03 for NG result;
//					0x04 for ERROR result;
//					0x05 for Read Barcode finish (m_readBcodeFinish == false);
//5. event 0x8024: Hermos reading/writing error code:
//					0x0e with e in {0,1, 2...9, A, :, ;, B};
//					0x10 for time out(usually is);
//6. event 0x8027: Omron reading/writing error code
//					0x1* for Comm error; 
//					0x7* for Trans and hardware error;
//					0xF0 for time out(usually is)
//7. event 0x8030: e84 input and output,FOUP status in e84
//8. event 0x8031: error code in e84 loop
///////////////////////////////////////////////////////////////////////////
//////////////////////////
//Begin CConfig
class CConfig {
public:
	char *menu[4];
	char mainmenuStr[500];
	char *lpsubmenu[4];
	char *lpsubmenuStr;

	char *lp1submenu[6];
	char *lp1submenuStr;
	char *lp2submenu[6];
	char *lp2submenuStr;

	char *ltsubmenu[6];
	char *ltsubmenuStr;
	char orgcfgfStr[20];//Hold the orignal cfg when program started.
	char cfgfileStr[20];//Hold the status of configuration
	char *cfgfileExplnStr;
	int getchoice(char *greet, char *choices[]);
	int setmmStrnVar(char lpcfchgedfrmorg, char *mmString, int *lp1reader,
		int *lp2reader, int *ltEnDis, int *ltOnlevl, char *conf);
	void rdCfgnSet(char *mmenuStr, char *cfgfStr);
	void wrCfgnSet(char orgchged, char *mmenuStr, char *cfgfStr);

	//prmSel: 1-_N2PurgeNozzleDown_InE84; 
	int rdSingleCfgFile(int prmSel);
	int wrSingleCfgFile(int prmSel, int prmVal);

	//originally public
	CConfig();
	void InitConfig();
	int MenuSelection();
};
//End CConfig
//////////////////////////////

//////////////////////////////
//Begin CLog
void *LogMaintainThread(void *arg);
void *TimerThread(void *arg);

class CLog {
private:
	char m_curTime[128];
	char m_logPath[128];
	char m_logFile[128];
	bool m_bWriteLog;
public:
	CLog(char *file) {
		strcpy(m_logPath, "/usr/tdk/log");
		mkdir(m_logPath, O_RDWR);
		if (strlen(file) > 127)
			strcpy(m_logFile, "default.log");
		else
			strcpy(m_logFile, file);
		m_bWriteLog = _WriteLog;
	}

	inline bool WriteLog(const char *msg) {
		if (!m_bWriteLog)
			return false;
		long unsigned int millisec;
		struct tm *tm_ptr;
		struct timeval tv;
		char s[2048];

		time_t the_time;
		time(&the_time);

		gettimeofday(&tv, NULL);
		millisec = lrint(tv.tv_usec / 1000.0);

		if (millisec >= 1000) { // Allow for rounding up to nearest second
			millisec -= 1000;
			tv.tv_sec++;
		}

		tm_ptr = localtime(&the_time);
		strftime(m_curTime, 128, "%m/%d/%Y %H:%M:%S:", tm_ptr);

		char logFullName[128];
		//sprintf(logFullName, "%s/%s_%d%02d%02d.log", m_logPath, m_logFile,
		//	    tm_ptr->tm_year+1900, tm_ptr->tm_mon+1, tm_ptr->tm_mday);
		sprintf(logFullName, "%s/%s%02d_%d.log", m_logPath, m_logFile,
			tm_ptr->tm_mon + 1, tm_ptr->tm_year + 1900);
		FILE *stream = NULL;
		if ((stream = fopen(logFullName, "ab")) == NULL)
			return false;
		fwrite(m_curTime, sizeof(char), strlen(m_curTime), stream);

#ifdef debug_log
		sprintf(s, "%3ld   ", millisec);
		fwrite(s, sizeof(char), strlen(s), stream);
#endif

		fwrite(msg, sizeof(char), strlen(msg), stream);
		fwrite("\n", sizeof(char), 1, stream);
		fclose(stream);
		return true;
	}

	inline bool WriteLog(const char *msg, const char *data, int datalen) {
		if (!m_bWriteLog)
			return false;
		long unsigned int millisec;
		struct tm *tm_ptr;
		struct timeval tv;
		char s[2048];

		time_t the_time;
		time(&the_time);

		gettimeofday(&tv, NULL);
		millisec = lrint(tv.tv_usec / 1000.0);


		if (millisec >= 1000) { // Allow for rounding up to nearest second
			millisec -= 1000;
			tv.tv_sec++;
		}
		tm_ptr = localtime(&the_time);

		strftime(m_curTime, 128, "%m/%d/%Y %H:%M:%S:", tm_ptr);

		char logFullName[128];
		sprintf(logFullName, "%s/%s%02d_%d.log", m_logPath, m_logFile,
			tm_ptr->tm_mon + 1, tm_ptr->tm_year + 1900);
		FILE *stream = NULL;
		if ((stream = fopen(logFullName, "ab")) == NULL)
			return false;

		fwrite(m_curTime, sizeof(char), strlen(m_curTime), stream);

#ifdef debug_log
		sprintf(s, "%3ld   ", millisec);
		fwrite(s, sizeof(char), strlen(s), stream);
#endif


		fwrite(msg, sizeof(char), strlen(msg), stream);
		fwrite("=> ", sizeof(char), 3, stream);
		char cc[20];
		for (int i = 0; i < datalen; i++) {
			if (0x20 <= (unsigned char)(data[i]) && (unsigned char)(data[i]) <= 0x7F)
			{
				//printable ASCII code
				sprintf(cc, "%c", data[i]);
			}
			else
				sprintf(cc, "(%02X)", data[i]);
			fwrite(cc, sizeof(char), strlen(cc), stream);
		}
		fwrite("\n", sizeof(char), 1, stream);
		fclose(stream);
		return true;
	}

	static void RunMaintain() {
		pthread_t a_thread;
		int res = pthread_create(&a_thread, NULL, LogMaintainThread, NULL);
		if (res != 0) {
			printf("Log Maintain thread creation failed");
		}
		res = pthread_create(&a_thread, NULL, TimerThread, NULL);
		if (res != 0) {
			printf("Timer thread creation failed");
		}
	}
};
void *TimerThread(void *arg) {
	while (1) {
		if (update_timeout > 0)
			update_timeout--;
		sleep(1);//1 second
		//usleep(100000);//100ms
	}
	pthread_exit(NULL);
}
void *LogMaintainThread(void *arg) {
	const char *dir = "/usr/tdk/log";
	DIR *dp;
	struct dirent *entry;
	struct stat statbuf;
	time_t curtime;
	time_t filetime;
	double timedif;
	char rmcommand[50];
	//printf("Now Running Log Maintain Thread!\n");
	while (1) {
		//printf("Now Running Log Maintain Loop!\n");
		time(&curtime);
		if ((dp = opendir(dir)) == NULL) {
			printf("Can not open directory: %s\n", dir);
			sleep(10);
			continue;
		}
		chdir(dir);
		while ((entry = readdir(dp)) != NULL) {
			lstat(entry->d_name, &statbuf);
			if (!S_ISDIR(statbuf.st_mode)) {
				filetime = statbuf.st_mtime;
				timedif = difftime(curtime, filetime) / 86400;
				if (timedif > 32) {
					//printf("Removing log files\n");
					strcpy(rmcommand, "rm -f ");
					strcat(rmcommand, entry->d_name);
					system(rmcommand);
				}
			}
		}
		closedir(dp);
		sleep(86400);//sleep 24 hours
	}
	pthread_exit(NULL);
}
//End CLog
//////////////////////////////
//////////////////////////////
//Begin CSerial
typedef void(*CBFuncPtType)(void *pCallbackObj, char *pMsg, int len);
////////////////
//Device IDs
#define HOSTBK	1
#define TAS300	2
#define BL600	3
#define HERMOS	4
#define OMRON	5
//End Device IDs
////////////////
#define MAXSIZE 2048
void *RcvThread(void *arg);

class CSerial {
public:
	int m_lpID;//the loadport ID 1(left) or 2(right)
	int m_deviceID;//the device to comunicate to
	char m_port[20];//"/dev/ttyS0-3" or "/dev/ttyS32-35" 
	int m_fd;//the file descriptor of the serial port
	void *m_pCBObj;
	CBFuncPtType m_CallbackW;
	CLog *m_log;//alloc at constructor, dealloc at destructor
	bool m_bStopThread;
	CSerial(int lpID, const char *logfile);
	~CSerial();
	//////////////////////////////////////
	//1st.call Initialize; 2nd.call OpenConnection
	//3rd.call SendBlock;  4th.call CloseConnection
	int Initialize(int deviceID, void *pCallbackObj, CBFuncPtType CallbackW);
	int OpenConnection();
	int CloseConnection();
	int SendBlock(const char *msg, int len);
	void RcvThreadBody();
	void fakeCB();
};

CSerial::CSerial(int lpID, const char *logfile) {
	char s[100];
	sprintf(s, "%d%s", lpID, logfile);
	m_lpID = lpID;
	m_log = new CLog(s);
	m_log->WriteLog("\n----------Program Starting----------");
	sprintf(s, "CSerial Constructor called with logfile = %d%s", lpID, logfile);
	m_log->WriteLog(s);
}
CSerial::~CSerial() {
	m_log->WriteLog("CSerial destructor called");
	delete m_log;
}

int CSerial::Initialize(int deviceID, void *pCallbackObj, CBFuncPtType CallbackW)
{
	char s[100];
	char s1[30];
	this->m_deviceID = deviceID;
	this->m_pCBObj = pCallbackObj;
	this->m_CallbackW = CallbackW;
	switch (m_deviceID) {
	case HOSTBK:
		sprintf(s1, "Host");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS0");
		else
			sprintf(m_port, "/dev/ttyCTI0");
		break;
	case TAS300:
		sprintf(s1, "TDK Tas300");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS1");
		else
			sprintf(m_port, "/dev/ttyCTI1");
		break;
	case BL600:
		sprintf(s1, "Keyence BL600");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS2");
		else
			sprintf(m_port, "/dev/ttyCTI2");
		break;
	case HERMOS:
		sprintf(s1, "Hermos Transponder Reader");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS2");
		else
			sprintf(m_port, "/dev/ttyCTI2");
		break;
	case OMRON:
		sprintf(s1, "Omron V700-HMD13A");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS2");
		else
			sprintf(m_port, "/dev/ttyCTI2");
		break;
	default://keep this branch for testing purpose
		sprintf(s1, "Unknown Device");
		if (m_lpID == 1)
			sprintf(m_port, "/dev/ttyS0");
		else
			sprintf(m_port, "/dev/ttyCTI0");
	}
	sprintf(s, "Initialize for lp%d device%d (%s) at %s",
		m_lpID, m_deviceID, s1, m_port);
	m_log->WriteLog(s);
	return 0;
}
//////////////////////////////
//Error code: 
//1-open port failure
//2-create thread failure
int CSerial::OpenConnection() {
	////////////////////////
	//RS232 Setting
	m_fd = open(m_port, O_RDWR | O_NOCTTY | O_NDELAY);
	char s[100];
	if (m_fd == -1) {//event port physically doesn't exist, still succeed in open
		sprintf(s, "Unable to open %s - %s", m_port, strerror(errno));
		printf("Unable to open %s - %s\n", m_port, strerror(errno));
		m_log->WriteLog(s);
		/*
		if(m_port[7] == 'y'){//orignally "/dev/ttyS*"
		  m_port[7] = 's'; m_port[8] = '/';
		}
		else{//orignally "/dev/tts/*"
		  m_port[7] = 'y'; m_port[8] = 'S';
		}
		m_fd = open(m_port, O_RDWR|O_NOCTTY|O_NDELAY);
		if(m_fd == -1)
		  return 1;//error code 1: open port failure
		else{
		  sprintf(s,"Succeed in opening %s",m_port);
		  m_log->WriteLog(s);
		}
		*/
	}
	else {
		sprintf(s, "Succeed in opening %s", m_port);
		printf("Succeed in opening %s\n", m_port);
		m_log->WriteLog(s);
	}

	struct termios options;
	// fcntl(m_fd, F_SETFL, FNDELAY);
	fcntl(m_fd, F_SETFL, 0);

	tcgetattr(m_fd, &options);// ttyS0,S1,S2 -> 4,6,5 ; ttyCTI0,CTI1,CTI2 -> 7 8 9
	switch (m_deviceID) {
	case HOSTBK:
		cfsetispeed(&options, B9600);
		cfsetospeed(&options, B9600);
		options.c_cflag &= ~PARENB;//disable parity bit
		options.c_cflag &= ~CSTOPB;//1 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS8;//select 8 data bits
		m_log->WriteLog("Set Baudrate 9600; 8 bits Data; 1 Stop bit; No Parity");
		break;
	case TAS300:
		cfsetispeed(&options, B9600);
		cfsetospeed(&options, B9600);
		options.c_cflag &= ~PARENB;//disable parity bit
		options.c_cflag &= ~CSTOPB;//1 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS8;//select 8 data bits
		m_log->WriteLog("Set Baudrate 9600; 8 bits Data; 1 Stop bit; No Parity");
		break;
	case BL600:
		////////////////////////////////////////////////////////
		//if expansion RS232 adapter selected, use these setting
		//cfsetispeed(&options, B1200);//correspond to ttyS4~7, 1200*8 = 9600
		//cfsetospeed(&options, B1200);
		cfsetispeed(&options, B9600);
		cfsetospeed(&options, B9600);
		options.c_cflag |= PARENB;//enable parity bit
		options.c_cflag &= ~PARODD;//even parity
		options.c_cflag &= ~CSTOPB;//1 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS7;//select 7 data bits
		m_log->WriteLog("Set Baudrate 9600; 7 bits Data; 1 Stop bit; Even Parity");
		break;
	case HERMOS:
		cfsetispeed(&options, B19200);
		cfsetospeed(&options, B19200);
		options.c_cflag |= PARENB;//enable parity bit
		options.c_cflag &= ~PARODD;//even parity
		options.c_cflag &= ~CSTOPB;//1 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS8;//select 8 data bits
		m_log->WriteLog("Set Baudrate 19200; 8 bits Data;1 Stop bit; Even Parity");
		break;
	case OMRON:
		cfsetispeed(&options, B9600);
		cfsetospeed(&options, B9600);
		options.c_cflag |= PARENB;//enable parity bit
		options.c_cflag &= ~PARODD;//even parity
		options.c_cflag &= ~CSTOPB;//1 stop bit//options.c_cflag |= CSTOPB;//2 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS8;//select 8 data bits
		m_log->WriteLog("Set Baudrate 9600; 7 bits Data;1 Stop bit; Even Parity");
		break;
	default:
		cfsetispeed(&options, B9600);
		cfsetospeed(&options, B9600);
		options.c_cflag &= ~PARENB;//disable parity bit
		options.c_cflag &= ~CSTOPB;//1 stop bit
		options.c_cflag &= ~CSIZE;//mask the character size bits
		options.c_cflag |= CS8;//select 8 data bits
		m_log->WriteLog("Set Baudrate 9600; 8 bits Data; 1 Stop bit; No Parity");
	}
	options.c_cflag |= (CLOCAL | CREAD);//ensure this program is not owner and 
									  //read incoming bytes
	options.c_cflag &= ~CRTSCTS;//disable hardware flow control
	options.c_lflag &= ~(ICANON | ECHO | ECHOE | ISIG); //raw input
	options.c_iflag &= ~(INLCR | IGNCR | ICRNL | IUCLC);//no character translation
	options.c_oflag &= ~OPOST;//raw output
	options.c_cc[VMIN] = 8;// it means get 8 bits every read, original setting 0 
	options.c_cc[VTIME] = 1;//1 tenth second read timeout,apply to 1st char only


	tcsetattr(m_fd, TCSANOW, &options);//make changes now
	//End RS232 Setting
	////////////////////////////

	//creating the receiving thread
	m_bStopThread = false;
	pthread_t a_thread;
	int res = pthread_create(&a_thread, NULL, RcvThread, this);
	if (res != 0) {
		m_log->WriteLog("Thread creation failed");
		return 2;//error code 2: thread creation failure
	}
	return 0;
}

int CSerial::CloseConnection() {
	close(m_fd);
	m_bStopThread = true;
	m_log->WriteLog("Connection closed");
	return 0;
}

int CSerial::SendBlock(const char *msg, int len) {
	int n = write(m_fd, msg, len);
	if (n < 0) {
		m_log->WriteLog("Send failed", msg, len);
		return 1;
	}
	else {
		m_log->WriteLog("Sent", msg, len);
	}
	return 0;
}

void CSerial::RcvThreadBody() {
	char buffer[MAXSIZE];//read() receiving buffer
	int bytesRd;
	char msg[MAXSIZE];//store assembled protocol message
	int msgTail = 0;//alway point to the first byte after the message
	int premsgTail = 0;
	int msgCurRetries = 0;
	//max calls to read(..) to assemble a protocol msg
	//const int msgMaxRetries = 35;
	m_log->WriteLog("Receiving thread started");
	while (!m_bStopThread) {
		char rt[256];




		switch (m_deviceID) {
		case HOSTBK:
			if (msgCurRetries < msgMaxRetries)
			{
				if (msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
				int msgRemainLen = MAXSIZE - msgTail;
				bytesRd = read(m_fd, buffer, msgRemainLen);
				if (bytesRd < 0) {//if port not exist, read will return -1
					bytesRd = 0;
					//  this->m_log->WriteLog(
					//	"In Receiving thread: read error! Port does not exist! ");
				}
#ifdef debug_log
				if (bytesRd > 0)
				{
					char s1[256];
					sprintf(s1, "msgTail:%d , bytesRd:%d", msgTail, bytesRd);
					this->m_log->WriteLog("After read ", buffer, strlen(buffer));
					this->m_log->WriteLog("b4 msgTail += bytesRd; ", s1, strlen(s1));

				}
#endif
				msgTail += bytesRd;

				for (int i = premsgTail; i < msgTail; i++)
				{
					msg[i] = buffer[i - premsgTail];
					//if (buffer[0] != NULL)
					//{
					//	this->m_log->WriteLog("Buffer", buffer, 20);
					//	this->m_log->WriteLog(" Msg", msg, 25);
					//}
				}
#ifdef debug_log
				if (bytesRd > 0)
				{
					this->m_log->WriteLog("After msg[i] = buffer[i - premsgTail]; ", msg, strlen(msg));
					char s1[256];
					sprintf(s1, "premsgTail:%d , msgTail:%d", premsgTail, msgTail);
					this->m_log->WriteLog("b4 premsgTail = msgTail; ", s1, strlen(s1));
				}
#endif
				premsgTail = msgTail;// premsgTail=8, msgTail=8 //r2 premsgTail = 3, msgTail = 3 no exceute

				//judge if message complete
				if (msgTail >= 2) {
					for (int i = 0; i < msgTail - 1; i++) {
#ifdef tdk_config
						if (msg[i] == 0x0D && msg[i + 1] == 0x0A && msg[i + 2] == 0x0A) {//TDKLpConfig commands ending characters is 0X0D 0X0A 0X0A
#else
						if (msg[i] == 0x0D && msg[i + 1] == 0x0A) {//msg assemble complete, Host computer commands ending characters is 0X0D 0X0A
#endif

						//if(msg[i]=='k' && msg[i+1]=='l'){//msg assemble complete
#ifdef tdk_config
							int len = i + 3;//its max is msgTail    
#else
							int len = i + 2;
#endif


							this->m_log->WriteLog("Received ", msg, len);
							this->m_CallbackW(m_pCBObj, msg, len);
#ifdef debug_log
							char s1[512];
							if (msgTail > 0) {
								sprintf(s1, "len:%d , msgTail:%d", len, msgTail);
								this->m_log->WriteLog("Rec ", s1, strlen(s1));
								this->m_log->WriteLog("B4 move msg:", msg, strlen(msg));
							}
#endif

							if (len < msgTail) {//move the remaining msg part to its head len=4 < msgTail=8 //r2  len =4  msgTail=3
								for (int i = len; i < msgTail; i++) // (i=5; i<8;++)
									msg[i - len] = msg[i];//msg[0]=msg[4]    msg[1,2,3]=msg[5,6,7]
							}
#ifdef debug_log
							this->m_log->WriteLog("After move msg:", msg, strlen(msg));
#endif
							msgCurRetries = 0;
#ifdef debug_log
							char s2[256];
							sprintf(s2, "msgTail:%d , len:%d", msgTail, len);
							this->m_log->WriteLog("b4 msgTail -= len; ", s2, strlen(s2));
#endif
							msgTail -= (len);//most probably will be 0   //len  = 4  ,msgTail = 3
#ifdef debug_log
							char s3[256];
							sprintf(s3, "premsgTail:%d , msgTail:%d", premsgTail, msgTail);
							this->m_log->WriteLog("b4 premsgTail = msgTail;", s3, strlen(s3));
#endif
							premsgTail = msgTail; //premsgTail = 3, msgTail = 3

							break;//exit the for loop containing this statement
						}
						}
					if (msgTail == MAXSIZE) {//search for 0D0A nonsuccessful,force end
						this->m_log->WriteLog("Received", msg, MAXSIZE);
						this->m_CallbackW(m_pCBObj, msg, MAXSIZE);
						msgCurRetries = 0;
						msgTail = 0;
						premsgTail = 0;
					}
					}
#ifdef debug_log
				char s[2048];
				if (msgTail > 0) {
					this->m_log->WriteLog("msg", msg, strlen(msg));
					sprintf(s, "msgCurRetries:%d", msgCurRetries);
					this->m_log->WriteLog("retry ", s, strlen(s));
				}
#endif
				}
			else {//reached maximum retry time, message still incomplete, end anyway
				this->m_log->WriteLog("Received else", msg, msgTail);
				this->m_CallbackW(m_pCBObj, msg, msgTail);
				msgCurRetries = 0;
				msgTail = 0;
				premsgTail = 0;
			}
			break;
		case TAS300://Standard Specification Format
			if (msgCurRetries < msgMaxRetries) {
				if (msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
				int msgRemainLen = MAXSIZE - msgTail;
				usleep(10000);
				bytesRd = read(m_fd, buffer, msgRemainLen);
				if (bytesRd < 0) {
					bytesRd = 0;
					//  this->m_log->WriteLog(
					//	"In Receiving thread: read error! Port does not exist! ");
				}
				msgTail += bytesRd;
				for (int i = premsgTail; i < msgTail; i++)
					msg[i] = buffer[i - premsgTail];
				premsgTail = msgTail;
				////////////////////////////
				//judge if message complete
				if (msgTail == 0)
					break;
				if (msg[0] != 0x01) {
					int iS = 0;
					while (iS < msgTail) {
						if (msg[iS] == 0x01) break;
						iS++;
					}
					int len = iS;
					this->m_log->WriteLog("Received", msg, len);//send the garbage
					this->m_CallbackW(m_pCBObj, msg, len);
					if (len < msgTail) {//move the remaining msg part to its head
						for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
					}
					msgCurRetries = 0;
					msgTail -= len;//most probably will be 0
					premsgTail = msgTail;
				}
				else {
					if (msgTail < 3)break;
					unsigned int framesize = (unsigned char)msg[1];
					//framesize << 8;
					framesize += (unsigned char)msg[2];
					framesize += 4;//SOH,LEN & DEL(maybe 03H,0DH, or 0DH0AH)
					if (msgTail < framesize)break;
					if (msg[framesize - 1] == 0x03 || msg[framesize - 1] == 0x0D) {
						int len;
						if ((framesize + 1) <= msgTail && msg[framesize] == 0x0A)
							len = framesize + 1;
						else
							len = framesize;
						this->m_log->WriteLog("Received", msg, len);//send the data
						this->m_CallbackW(m_pCBObj, msg, len);
						if (len < msgTail) {//move the remaining msg part to its head
							for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
						}
						msgCurRetries = 0;
						msgTail -= len;//most probably will be 0
						premsgTail = msgTail;
					}
					else {//the msg[0] maybe changed to 0x01 by interference
						int iS = 1;//search for 0x01 from msg[1]
						while (iS < msgTail) {
							if (msg[iS] == 0x01) break;
							iS++;
						}
						int len = iS;
						this->m_log->WriteLog("Received", msg, len);//send the garbage
						this->m_CallbackW(m_pCBObj, msg, len);
						if (len < msgTail) {//move the remaining msg part to its head
							for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
						}
						msgCurRetries = 0;
						msgTail -= len;//most probably will be 0
						premsgTail = msgTail;
					}
				}
			}
			else {//reached maximum retry time, message still incomplete, end anyway
				this->m_log->WriteLog("Received", msg, msgTail);
				this->m_CallbackW(m_pCBObj, msg, msgTail);
				msgCurRetries = 0;
				msgTail = 0;
				premsgTail = 0;
			}
			break;
		case BL600:
			if (msgCurRetries < msgMaxRetries) {
				if (msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
				int msgRemainLen = MAXSIZE - msgTail;
				bytesRd = read(m_fd, buffer, msgRemainLen);
				if (bytesRd < 0) {//if port not exist, read will return -1
					bytesRd = 0;
					//  this->m_log->WriteLog(
					//	"In Receiving thread: read error! Port does not exist! ");
				}
				msgTail += bytesRd;
				for (int i = premsgTail; i < msgTail; i++)
					msg[i] = buffer[i - premsgTail];
				premsgTail = msgTail;
				///////////////////////////
				//judge if message complete
				if (msgTail > 0) {
					for (int i = 0; i < msgTail - 1; i++) {
						if (msg[i] == 0x0D || msg[i] == 0x03) {//msg assemble complete
							int len;
							if (msgTail > 1 && msg[i + 1] == 0x0A)
								len = i + 2;//its max is msgTail
							else len = i + 1;
							this->m_log->WriteLog("Received", msg, len);
							this->m_CallbackW(m_pCBObj, msg, len);
							if (len < msgTail) {//move the remaining msg part to its head
								for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
							}
							msgCurRetries = 0;
							msgTail -= len;//most probably will be 0
							premsgTail = msgTail;
							break;//exit the for loop containing this statement
						}
					}
					if (msgTail == MAXSIZE) {//search for 0D0A nonsuccessful,force end
						this->m_log->WriteLog("Received", msg, MAXSIZE);
						this->m_CallbackW(m_pCBObj, msg, MAXSIZE);
						msgCurRetries = 0;
						msgTail = 0;
						premsgTail = 0;
					}
				}
			}
			else {//reached maximum retry time, message still incomplete, end anyway
				this->m_log->WriteLog("Received", msg, msgTail);
				this->m_CallbackW(m_pCBObj, msg, msgTail);
				msgCurRetries = 0;
				msgTail = 0;
				premsgTail = 0;
			}
			break;
		case HERMOS:
			if (msgCurRetries < msgMaxRetries) {
				if (msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
				int msgRemainLen = MAXSIZE - msgTail;
				bytesRd = read(m_fd, buffer, msgRemainLen);
				if (bytesRd < 0) {
					bytesRd = 0;
					//  this->m_log->WriteLog(
					//	"In Receiving thread: read error! Port does not exist! ");
				}
				msgTail += bytesRd;
				for (int i = premsgTail; i < msgTail; i++)
					msg[i] = buffer[i - premsgTail];
				premsgTail = msgTail;
				////////////////////////////
				//judge if message complete
				if (msgTail < 3)
					break;
				if (msg[0] != 'S' //start sign
					|| msg[1] < '0' || ('9' < msg[1] && msg[1] < 'A') || msg[1] > 'F'	//highbyte length '0'~'F'
					|| msg[2] < '0' || ('9' < msg[2] && msg[2] < 'A') || msg[2] > 'F') {//lowbyte length '0'~'F'
					int iS = 1;
					while (iS < msgTail) {
						if (msg[iS] == 'S') break;
						iS++;
					}
					int len = iS;
					this->m_log->WriteLog("Received", msg, len);//send the garbage
					this->m_CallbackW(m_pCBObj, msg, len);
					if (len < msgTail) {//move the remaining msg part to its head
						for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
					}
					msgCurRetries = 0;
					msgTail -= len;//most probably will be 0
					premsgTail = msgTail;
				}
				else {
					unsigned int framesize;
					if (msg[1] <= '9')
						framesize = msg[1] - '0';
					else
						framesize = msg[1] - 'A' + 10;
					framesize <<= 4;
					if (msg[2] <= '9')
						framesize += msg[2] - '0';
					else
						framesize += msg[2] - 'A' + 10;
					framesize += 8;//message size + 8
					//char sss[50];
					//sprintf(sss, "fram = %d", framesize);
					//this->m_log->WriteLog("note", sss, strlen(sss));//send the data
					if (msgTail < framesize)break;
					if (msg[framesize - 5] == 0x0D) {
						int len = framesize;
						this->m_log->WriteLog("Received", msg, len);//send the data
						this->m_CallbackW(m_pCBObj, msg, len);
						if (len < msgTail) {//move the remaining msg part to its head
							for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
						}
						msgCurRetries = 0;
						msgTail -= len;//most probably will be 0
						premsgTail = msgTail;
					}
					else {//data maybe changed by interference
						int iS = 1;//search for 'S' from msg[1]
						while (iS < msgTail) {
							if (msg[iS] == 'S') break;
							iS++;
						}
						int len = iS;
						this->m_log->WriteLog("Received", msg, len);//send the garbage
						this->m_CallbackW(m_pCBObj, msg, len);
						if (len < msgTail) {//move the remaining msg part to its head
							for (int i = len; i < msgTail; i++)msg[i - len] = msg[i];
						}
						msgCurRetries = 0;
						msgTail -= len;//most probably will be 0
						premsgTail = msgTail;
					}
				}
			}
			else {//reached maximum retry time, message still incomplete, end anyway
				this->m_log->WriteLog("Received", msg, msgTail);
				this->m_CallbackW(m_pCBObj, msg, msgTail);
				msgCurRetries = 0;
				msgTail = 0;
				premsgTail = 0;
			}
			break;
		case OMRON:

			char s[100];
			char s1[100];
			if (msgCurRetries < msgMaxRetries) {
				if (msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
				int msgRemainLen = MAXSIZE - msgTail;
				bytesRd = read(m_fd, buffer, msgRemainLen);
#ifdef debug_log				
				if (bytesRd > 0 || msgTail > 0) this->m_log->WriteLog("read buffer", buffer, strlen(buffer));
#endif			
				if (bytesRd < 0) {//if port not exist, read will return -1
					bytesRd = 0;
					//  this->m_log->WriteLog(
					//	"In Receiving thread: read error! Port does not exist! ");
				}
#ifdef debug_log
				if (bytesRd > 0)
				{
					char s1[256];
					sprintf(s1, "msgTail:%d , bytesRd:%d", msgTail, bytesRd);
					this->m_log->WriteLog("After read ", buffer, strlen(buffer));
					this->m_log->WriteLog("b4 msgTail += bytesRd; ", s1, strlen(s1));
				}
#endif


				msgTail += bytesRd;
				for (int i = premsgTail; i < msgTail; i++)
					msg[i] = buffer[i - premsgTail];
#ifdef debug_log				
				if (bytesRd > 0)
				{
					this->m_log->WriteLog("After msg[i] = buffer[i - premsgTail]; ", msg, strlen(msg));
					char s1[256];
					sprintf(s1, "premsgTail:%d , msgTail:%d", premsgTail, msgTail);
					this->m_log->WriteLog("b4 premsgTail = msgTail; ", s1, strlen(s1));
				}
#endif
				premsgTail = msgTail;
#ifdef debug_log				
				if (msgTail > 0)
				{
					char s[100];
					this->m_log->WriteLog("msg", msg, strlen(msg));
					sprintf(s, "msgCurRetries:%d", msgCurRetries);
					this->m_log->WriteLog("retry ", s, strlen(s));
				}
#endif
				///////////////////////////
				//judge if message complete
				char s3[100];
				if (msgTail >= 1) {
					for (int i = 0; i < msgTail; i++) {//previous for(int i=0; i<msgTail; i++)
#ifdef debug_log
						if (msgTail > 0) {
							this->m_log->WriteLog("Loop");
							sprintf(s3, "char for msg[%d], %c", i, msg[i]);
							this->m_log->WriteLog("msg[i] ", s3, strlen(s3));
						}
#endif
						if (msg[i] == 0x0D) {//msg assemble complete
							int len = i + 1;
							this->m_log->WriteLog("Received 0X0D", msg, len);
							this->m_CallbackW(m_pCBObj, msg, len);
#ifdef debug_log
							if (msgTail > 0) {
								sprintf(s1, "len:%d , msgTail:%d", len, msgTail);
								this->m_log->WriteLog("Rec ", s1, strlen(s1));
								this->m_log->WriteLog("B4 move msg:", msg, strlen(msg));
							}
#endif
							if (len < msgTail) {//move the remaining msg part to its head
								for (int i = len; i < msgTail; i++)
									msg[i - len] = msg[i];
							}

#ifdef debug_log
							this->m_log->WriteLog("After move msg:", msg, strlen(msg));
#endif
							msgCurRetries = 0;
#ifdef debug_log
							char s2[256];
							sprintf(s2, "msgTail:%d , len:%d", msgTail, len);
							this->m_log->WriteLog("b4 msgTail -= len; ", s2, strlen(s2));
#endif
							msgTail -= len;//most probably will be 0
#ifdef debug_log
							char s3[256];
							sprintf(s3, "premsgTail:%d , msgTail:%d", premsgTail, msgTail);
							this->m_log->WriteLog("b4 premsgTail = msgTail;", s3, strlen(s3));
#endif
							premsgTail = msgTail;
							break;//exit the for loop containing this statement

						}
					}
					if (msgTail == MAXSIZE) {//search for 0D nonsuccessful,force end
						this->m_log->WriteLog("Received MAXSIZE", msg, MAXSIZE);
						this->m_CallbackW(m_pCBObj, msg, MAXSIZE);
						msgCurRetries = 0;
						msgTail = 0;
						premsgTail = 0;
					}

				}
			}
			else {//reached maximum retry time, message still incomplete, end anyway
				this->m_log->WriteLog("Received maximum retry time", msg, msgTail);
				this->m_CallbackW(m_pCBObj, msg, msgTail);
				msgCurRetries = 0;
				msgTail = 0;
				premsgTail = 0;
			}
			break;
		default:
			if (bytesRd > 0)
				this->m_CallbackW(m_pCBObj, buffer, bytesRd);

			}
		}
	m_log->WriteLog("Receiving thread stopped");
	}

void CSerial::fakeCB() {
	char s[30];
	sprintf(s, "Hello, from fakeCB()");
	this->m_CallbackW(m_pCBObj, s, strlen(s));
}
//////////////////////////////////
//Assistant Thread for CSerial
void *RcvThread(void *arg) {
	CSerial *pSerial = (CSerial *)arg;
	pSerial->RcvThreadBody();
}
//End Assistant Thread for CSerial
//////////////////////////////////
//End CSerial
///////////////////////////////

///////////////////////////////////////////////////////////////////////////////

//////////////////////////////
//CLP program state machine states
#define prg_NOTINIT     0
#define prg_READY       1
#define prg_BUSY        2
//End CLP program state machine states
//////////////////////////////
//////////////////////////////////////////////////
//CLP Fixload with AMHS state machine states
#define fxl_NOTINIT     0
#define fxl_READY       1
#define fxl_BUSY        2
#define fxl_AMHS        3
//End CLP Fixload with AMHS state machine states
//////////////////////////////////////////////////

////////////////////////////////
//HostBK Class Declaration

class CHostBK {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	CHostBK(int lpID, void *parent);
	~CHostBK();
	int OpenPort();
	int ClosePort();
	void DoTest();
private:
	int m_lpID;
	void *m_parent;
	void Callback(char *pMsg, int len);
	int ToInteger(char *item, int itemlen, int *num);
	int HEX2Integer(char *item, int itemlen, int *num);
	int ReadItem(char *newmsg, int newmsgMaxlen, int *newmsglen, char *msg, int msglen, int *msglenstPos);
	int ReadLotID(char *a, int alen, int *aclen, char *b, int blen, int *bstPos);
	int CmpStr(const char *cmpstr, int cmpstrlen, char *newmsg, int newmsglen);
	int SearchStr(const char *searstr, int searstrlen, char *msg, int msglen, int *msgstPos);
	int ReadItemIO(char *newmsg, int newmsgMaxlen, int *newmsglen, char *msg, int msglen, int *msglenstPos);
};
//End HostBK Class Declaration
////////////////////////////////

//////////////////////////////////
//Tas300 Class Declaration
//////////////////////////////////
//Command Communication states
#define IDLE        0
#define WAITACK     1
#define WAITINF     2
//End Command Communication states
//////////////////////////////////
//////////////////////////////////
//Command Parameter Length
#define PARAMLEN   50
//End Command Parameter Length
//////////////////////////////////

//////////////////////////////////
//Command execution results
#define RES_NON       -1
#define RES_ACK        0
#define RES_NAK        1
#define RES_INF        2
#define RES_ABS        3
#define RES_RIF        4
#define RES_RAS        5
//End Command execution results
//////////////////////////////////

//////////////////////////////////
//Commands: turn out to no use
#define CMD_NONE        -1

#define GET_STATE       0//use
#define GET_VERSION     1
#define GET_LEDST       2
#define GET_MAPDT       3
#define GET_MAPRD       4//use
#define GET_WFCNT       5

#define EVT_EVTON       10//use
#define EVT_EVTOF       11
#define EVT_FPEON       12//use
#define EVT_FPEOF       13

#define MOV_ORGSH       20//initialize = brooks io init (not exactly)
#define MOV_ABORG       21//initialize = brooks io initx 2
#define MOV_PODCL       22//clamp = brooks io load 2
#define MOV_CLDYD       23//ld to dock = brooks io load 1
#define MOV_CLOAD       24//load = brooks io load
#define MOV_CULYD       25//uld to dock = brooks io unload 1
#define MOV_CULFC       26//uld to clamp = brooks io unload 2
#define MOV_CULOD       27//unload = brooks io unload
#define MOV_MAPDO       28//map = brooks io map
#define MOV_ABORT       29

#define SET_RESET       30//error reset
#define SET_INITL       31//program initialization
#define SET_LON01       32//load led: on
#define SET_LBL01       33//load led: flashing
#define SET_LOF01       34//load led: off
#define SET_LON02       35//unload led: on
#define SET_LBL02       36//unload led: flashing
#define SET_LOF02       37//unload led: off
#define SET_LON03       38//OP.Access led: on
#define SET_LBL03       39//OP.Access led: flashing
#define SET_LOF03       40//OP.Access led: off
#define SET_LON04       41//presence led: on
#define SET_LBL04       42//presence led: flashing
#define SET_LOF04       43//presence led: off
#define SET_LON05       44//placement led: on
#define SET_LBL05       45//placement led: flashing
#define SET_LOF05       46//placement led: off
#define SET_LON06       47//alarm led: on
#define SET_LBL06       48//alarm led: flashing
#define SET_LOF06       49//alarm led: off
#define SET_LON07       50//status1 led: on
#define SET_LBL07       51//status1 led: flashing
#define SET_LOF07       52//status1 led: off
#define SET_LON08       53//status2 led: on
#define SET_LBL08       54//status2 led: flashing
#define SET_LOF08       55//status2 led: off
//End Commands
//////////////////////////////////
//////////////////////////////////
//Foup Status
//#define FPS_ERROR         -2
#define FPS_UNKNOWN       -1
#define FPS_NOFOUP         0
#define FPS_PLACED         1
#define FPS_CLAMPED        2
#define FPS_DOCKED         3
#define FPS_OPENED         4
//End Foup Status
//////////////////////////////////
//////////////////////////////////
//Foup Event:
#define FPEVT_NONE			0xFF
#define FPEVT_PODOF			0
#define FPEVT_SMTON			1
#define FPEVT_ABNST			2
#define FPEVT_PODON			3
//End Foup Event
//////////////////////////////////
//////////////////////////////////
//Lamp Operation Code
#define LMP_OF      0
#define LMP_ON      1
#define LMP_BL      2
//End Lamp Operation Code
//////////////////////////////////


/*
struct ThreadArgType
{
  void *pObj;
  int prm1;
};
*/
//void *PodEvtThreadTas300(void *Arg);

class CTas300 {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	CTas300(int lpID, void *parent);
	~CTas300();
	int m_fpStatus;//FPS_NOFOUP, FPS_PLACED, FPS_CLAMPED, FPS_DOCKED, FPS_OPENED
	unsigned char m_fpEvent;//FPEVT_PODOF, FPEVT_SMTON, FPEVT_ABNST, FPEVT_PODON
	char m_statfxl[5];//for example: "0x69"
	char m_mapRes[26];//'0':No wafer;'1':Wafer;'2':Crossed;'W':Overlap;Other:undf
					  //TDK format, not SEMI format
	struct STATUS {
		char eqpStatus;//'0'= normal, 'A'= recoverable error, 'E'= fatal error
		char mode;//'0'= online, '1'= maintain;
		char inited;//'0'= not inited, '1'= inited
		char opStatus;//'0'= stopped, '1'= operating
		unsigned char ecode;//binary code 0= no error
		char fpPlace;//'0'= not present, '1'= placed, '2'= not placed properly
		char fpClamp;//'0'= released, '1'= clamped, '?'= not defined
		char ltchKey;//'0'= open, '1'= close, '?'= not defined
		char vacuum;//'0'= off, '1'= on
		char fpDoor;//'0'= open, '1'= close, '?'= not defined
		char wfFlyOutSensor;//'0'= blocked, '1'= unblocked
		char zPos;//'0'= up, '1'= down, '?'= not defined
		char yPos;//'0'= undock, '1'= docked, '?'= not defined
		char mpArmPos;//'0'= open, '1'= close, '?'= not defined
		char mpzPos;//'0'= retract, '1'= mapping, '?'= not defined
		char mpStoper;//'0'= on, '1'= off, '?'= not  defined
		char mapingStatus;//'0'= unmapped, '1'= mapped, '?'= map failed
		char intKey;//'0'= enable, '1'-'3'= disable
		char infoPad;//'0'= no input, '1'= A-pin on, '2'= B-pin on, '3'= both on
	}m_Status;

	struct N2PSTATUS {
		char gasPressure;//'0'=Normal, 'E'=Abnormal
		char nozzlePos;//'0'=All Down, '1'=All Up, '2'=Abnormal
	}m_n2pStatus;

	int OpenPort();
	int ClosePort();
	void DoTest();
	int statfxl();//GET
	int statn2purge();//GET
	int mapResult();//GET
	int evtON();//EVT
	int fpeON();//EVT
	int rstErr();//SET
	int prgInit();//SET
	int lampOP(int lmpNo, int opCode);//SET
	int movOP(const char *name);//MOV
	int movABORT();//MOV:if you don't use resend cmd, abort before send the same 
private:
	int m_lpID;
	void *m_parent;
	//ThreadArgType m_arg;
	//int m_curOpCmd;//Current Operation Cmd, including MOV, SET
	char m_curOpCmdName[6];//eg. "ORGSH"
	int m_OpCmdState;//IDLE, WAITACK, WAITINF
	int m_OpCmdRes;//Operation Cmd execution result: ACK, NAK, ABS, INF
	char m_OpCmdResParam[PARAMLEN];
	char m_EventParam[PARAMLEN];
	sem_t m_semOpCmdACK;
	sem_t m_semOpCmdINF;

	//int m_curInfCmd;//Current Info Cmd, including MOD, GET, EVT
	char m_curInfCmdName[6];//eg. "STATE"
	int m_InfCmdState;//IDLE, WAITACK
	int m_InfCmdRes;//Info Cmd execution result: ACK, NAK
	char m_InfCmdResParam[PARAMLEN];
	sem_t m_semInfCmdACK;

	int sem_reset(sem_t *sem);
	int prepCmd(const char *cmdstr, char *frame, int fmaxlen, int *factulen);
	void Callback(char *pMsg, int len);
};
//End Tas300 Class Declaration
//////////////////////////////////

//////////////////////////////////
//BL600 Class Declaration
class CBL600 {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	CBL600(int lpID, void *parent);
	~CBL600();
	int OpenPort();
	int ClosePort();
	int MotorON();
	int ReadBarCode(char *barcode, int len, int *aclen);
	int MotorOFF();//called during start up at OpenPort
	int Lock();
	int Unlock();
	void DoTest();
private:
	int m_lpID;
	void *m_parent;
	bool m_waitingRes;
	char m_bcode[100];
	int m_bcodelen;
	sem_t m_semCmdACK;
	sem_t m_semEvtRead;
	int sem_reset(sem_t *sem);
	int ReadResStr(char *a, int alen, int *aclen, char *b, int blen);
	void Callback(char *pMsg, int len);
};
//End BL600 Class Declaration
//////////////////////////////////

//////////////////////////////////
//Hermos Class Declaration
//////////////////////////////////
//Current Command
#define CMD_NULL	 0
#define RD_1PAGE	 1
#define RD_MPAGE     2
#define WR_TAG	     3
#define VS_ASK	     4
//End Current Command
//Command execution result
#define ACKED_OK	 0
#define ACKED_ERR	 1
//End command execution result
#define MAXINFOLEN   40

class CHermos {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	CHermos(int lpID, void *parent);
	~CHermos();
	int OpenPort();
	int ClosePort();
	int ReadRFID(int page, char *rfid, int len, int *aclen);
	int ReadMULTIPAGE(int page, char *content, int len, int *aclen);
	int WriteRFID(int page, const char *rfid, int len);
	int AskVersion(char *version, int len, int *aclen);
	void DoTest();
private:
	int m_lpID;
	void *m_parent;
	int m_CmdState;//IDLE, WAITACK
	int m_CurCmd;//CMD_NULL, RD_1PAGE, RD_MPAGE, WR_TAG, VS_INT; 
				 //This is meaningful only if m_CmdState is WAITACK
	int m_CmdRes;//ACKED_OK, ACKED_ERR
	char m_CmdResInfo[MAXINFOLEN];
	int m_CmdResInfoLen;
public:
	char m_failCode[2];
private:
	sem_t m_semCmdACK;
	sem_t m_semNextPage;
	int sem_reset(sem_t *sem);
	int prepCmd(const char *cmdstr, char *frame, int fmaxlen, int *factulen);
	void Callback(char *pMsg, int len);
};
//End Hermos Class Declaration
//////////////////////////////////

//////////////////////////////////
//Omron Class Declaration
//////////////////////////////////
//Current Command
#define OM_NUL		0
#define OM_RD		1
#define OM_WT		2
#define OM_WP		3
#define OM_TS		4
//End Current Command
//Command State
#define OM_IDLE		0
#define OM_WAITRES	1
//End Command State
//Command execution result
#define OM_RESOK	0
#define OM_RESERR	1
//End command execution result
//ID content format
#define OM_ASCII	0
#define OM_HEX		1
#define OM_MAXLEN	480
//End ID content format

class COmron {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	COmron(int lpID, int fmt, void *parent);
	~COmron();
	int OpenPort();
	int ClosePort();
	int ReadRFID(int page, char *rfid, int len, int *aclen);
	int WriteRFID(int page, char *rfid, int len);
	int LoopBackTS(char *tststr, int len, int *aclen);
	void DoTest();
private:
	int m_lpID;
	void *m_parent;
	int m_cntntFMT;

	int m_CmdState;//OM_IDLE, OM_WAITRES
	int m_CurCmd;//OM_NUL, OM_RD, OM_WT, OM_WP, OM_TS; 
				 //This is meaningful only if m_CmdState is OM_WAITRES
	int m_CmdRes;//OM_RESOK, OM_RESERR
	char m_CmdResParam[OM_MAXLEN];
	int m_CmdResParamLen;
public:
	char m_CompleteCode[3];//need 2, the last one stores '\0'
private:
	sem_t m_semCmdRes;
	int sem_reset(sem_t *sem);
	void Callback(char *pMsg, int len);
};
//End Omron Class Declaration
//////////////////////////////////


//////////////////////////////////
//E84 Class Declaration
//////////////////////////////////
//AMHS States
#define E84_DISABLD   -1
#define E84_ENABLED    0
#define E84_CS_ON      1
#define E84_VALID_ON   2
#define E84_LUREQ_ON   3
#define E84_TRREQ_ON   4
#define E84_READY_ON   5
#define E84_BUSY_ON    6
#define E84_LUREQ_OF   7
#define E84_COMPT_ON  10
#define E84_READY_OF  11
#define E84_VALID_OF  12
//End AMHS States
//////////////////////////////////

void *AmhsThreadE84(void *Arg);

class CE84 {
public:
	CE84(int lpID, void *parent);
	~CE84();
	static int m_hDev;//initially null; not null if opened;
	int m_tp1, m_tp2, m_tp3, m_tp4, m_tp5, m_tp6, m_td1;
	int m_ta1, m_ta2, m_ta3, m_td0;
	bool m_constChanged;
	unsigned char m_Output;//Data written to Output port
	unsigned char m_Input;//Data read from Input port
	int m_mchStatus;
	bool m_amhsRunning;
	bool bSetLTC_LED_ON; //Avoid repeat command send
	int OpenDevice();
	int CloseDevice();
	int CreateE84Thread();
	int EnableAMHS();//start AMHS
	int DisableAMHS();//Stop AMHS
	int SetLTC(int endis);
	int SetLTC_LED(int onoff);
	void DoTest();
	void AmhsThreadBody();
	int W_HO_AVBL(int onoff);
	int W_ES(int onoff);
	int W_E84OUTPUT(int src, unsigned char output);
	int W_NONE84OUT(unsigned char output);
	int R_INPUT(int *VALID, int *CS_0, int *CS_1, int *LTCIN,
		int *TR_REQ, int *BUSY, int *COMPT, int *CONT);
	int R_OUTPUT(int *L_REQ, int *U_REQ, int *READY, int *HO_AVBL,
		int *ES);


private:
	int m_lpID;
	void *m_parent;
	int m_e84State;
	int m_e84LDULD;//0:Load, 1:Unload; decided when starting AMHS
	unsigned char m_wPort;//Writing port dio2
	unsigned char m_rPort;//Reading port dio0
	int W_L_REQ(int onoff);//1:on; 0:off 
	int W_U_REQ(int onoff);
	int W_READY(int onoff);

	sem_t m_semOutput;
	int sem_reset(sem_t *sem);

	void ErrSignalProcess(char *status);//null termed string
	void ErrFpStatus_Busy(char *status);//null terminated string
	void ErrFpStatus_Ready(char *status);//null terminated string
	void TimeOutProcess(char *evtcode, char *status);//null termed string
	void AbortSequence(char *status);//null terminated string
	void LTCViolated(char *status);//null terminated string
	void MchDoorOpened(char *status);//null terminated string
};
//End E84 Class Declaration
//////////////////////////////////

//////////////////////////////////
//Load Port Class Declaration
class CLP {
public:
	CLP(int lpID);
	~CLP();
	CLog *m_log;//alloc at constructor, dealloc at destructor
	CHostBK *m_host;//alloc at constructor, dealloc at destructor
	CTas300 *m_tas300;//alloc at constructor, dealloc at destructor
	CBL600 *m_bl600;//alloc at constructor, dealloc at destructor
	CHermos *m_hmos;//alloc at constructor, dealloc at destructor
	COmron *m_omron;//alloc at constructor, dealloc at destructor
	CE84 *m_e84;//alloc at constructor, dealloc at destructor
	CLP *m_brother;//pointer to its brother load port
	CConfig *m_pCfg;
	int m_prgState;//prg_NOTINIT,prg_READY,prg_BUSY
	int m_fxlamhsState;//fxl_NOTINIT, fxl_READY, fxl_BUSY, fxl_AMHS
	char m_mapRes[26];//'0':Undef;'1':empty;'2':notempty;'3':correctly Ocupied
					  //'4':Double Slotted '5':Cross Slotted=>Semi Format
	int EnableOperation();
	int DisableOperation();
	void Setbrother(CLP *brother);
	void Setconfig(CConfig *pCfg);
	void Brkinit();
	void Brkinitx();
	void Brkload(int ldtype);
	void Brkunload(int uldtype);
	void Brkevon(char *evtID);
	void Brkevoff(char *evtID);
	void Brkid();
	void Brkstatfxl();
	void Brkstatnzl();
	void Brkstat_m();
	void Brkstat_pdo();
	void Brkstat_lp();
	void Brklamp(int lampID, int lampACT);
	void Brkmap();
	void Brkrmap();
	void Brkrdid(int page);
	void Brkwrid(int page, char* lotID, int lotIDlen);
	void Brkresid();
	void Brkesmode(int mode);
	void Brkmchstatus(int status);
	void Brke84t(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, int td1);
	void Brksmcr();
	void Brkenltc(int onoff);
	void Brkene84nz(int onoff);
	void Brkgetconf();
	void Brksetconf(int p1, int p2, int p3, int p4);
	void Brkshutdown(int cmd);
	void Brkupdate(char *lengthStr, int len);
	void Brkassemblefile(char *received, int len);
	void Brkene84(int onoff, int addr);
	void Brkrde84(int addr);
	void Brkho_avbl(int ho_avbl, int addr);
	void Brkes(int es, int addr);
	void Brkout_e84(char *out_e84, int addr);
	void BrkPurge(int purgeType);//1-Activate; 2 or other - Deactivate
	void Brkdate();
	void E84_st_chg(const char *evtcode, char *status);//evtcode status:null termed str
	void E84_st_chg2(const char *evtcode, char *status);//evtcode status:null termed str
	void E84_8030_Event(bool foup_errcond);
	void E84_8031_Event(int errorcode);
	void TasPodEvt(int off_2_on);//0: not present(PODOF), 1: present only(SMTON) 
		 //2: partial placement(ABNST), 3: normal placement(PODON)
	void TasManSwEvt();
	void TasPGEvent(char *evtparm);
	int GetFxlAmhsStatus();
	void CheckHWstatus();
	void DoTest();
	void SendToHostMaxReceiveTimes();
private:
	int m_lpID;
	void TasErrorHandle();//Used in:Brkload;Brkunload;Brkmap;Brkrmap;Brkrdid
	long m_updatefilelen;
	long m_receivedlen;
	int offonStatus;
public:
	bool m_bacceptingfile;
};
//End Load Port Class Declaration
///////////////////////////////////

//////////////////////////////////
//E84 Class Implementation
CE84::CE84(int lpID, void *parent) {
	m_lpID = lpID;
	m_parent = parent;
	// m_hDev = 0;
	m_amhsRunning = false;
	bSetLTC_LED_ON = false;
	m_mchStatus = 0;
	m_e84LDULD = -1;
	m_Output = 0;
	m_Input = 0;
	if (m_lpID == 1) {
		m_wPort = 2;//writing port
		m_rPort = 0;//reading port
	}
	else {//if(m_lpID == 2)
		m_wPort = 3;//writing port
		m_rPort = 1;//reading port
	}
	m_tp1 = 2; m_tp2 = 2; m_tp3 = 60;
	m_tp4 = 60; m_tp5 = 2; m_tp6 = 2; m_td1 = 1;
	m_ta1 = 2; m_ta2 = 2; m_ta3 = 2; m_td0 = 1;//m_td0 TYP 0.1
	m_constChanged = false;
	int res = sem_init(&m_semOutput, 1, 1);
	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("m_semOutput initialization failed");

	((CLP*)m_parent)->m_log->WriteLog("CE84 Constructor called");
}
CE84::~CE84() {
	((CLP*)m_parent)->m_log->WriteLog("CE84 Destructor called");
}

int CE84::m_hDev = 0;
int CE84::OpenDevice() {//only used in CLP::EnableOperation()
	if (m_hDev) {
		((CLP*)m_parent)->m_log->WriteLog("Dio device already opened");
		return 0;
	}
	//always load the driver here!
	int ret = system("/usr/local/sbin/seaioload");
	char s[50];
	sprintf(s, "seaioload return code = %d", ret);
	((CLP*)m_parent)->m_log->WriteLog(s);

	unsigned long status = 0;
	status = SeaIo_OpenDevice(0, &m_hDev);//must open port 0(/dev/dio0)to operate
										  //all 4 ports(dio0~3)
	if (status != 0) {
		m_hDev = 0;
		((CLP*)m_parent)->m_log->WriteLog("Could not ");
		printf("Could not open dio device\n");
		return 1;
	}
	else
		printf("Succeed in opening Sea Digital I/O device\n");
	return 0;
}
int CE84::CloseDevice() {//only used in CLP::DisableOperation()
	if (m_hDev) {
		SeaIo_CloseDevice(m_hDev);
		//always close the driver here!
		int ret = system("/usr/local/sbin/seaiounload");
		char s[50];
		sprintf(s, "seaio-unload return code = %d", ret);
		//((CLP*)m_parent)->m_log->WriteLog(s);
	}
	m_hDev = 0;
	return 0;
}
int CE84::CreateE84Thread() {//only used in CLP::EnableOperation()
	pthread_t a_thread;
	int res = pthread_create(&a_thread, NULL, AmhsThreadE84, (void *)this);
	if (res != 0) {
		((CLP*)m_parent)->m_log->WriteLog("E84 Thread creation failed");
		return 1;
	}
	return 0;
}
int CE84::EnableAMHS() {//start AMHS
	if (m_amhsRunning) {
		((CLP*)m_parent)->m_log->WriteLog("E84 Already Enabled: Wrong Enable");
		return 1;
	}
	m_amhsRunning = true;
	/*
	pthread_t a_thread;
	int res = pthread_create(&a_thread,NULL,AmhsThreadE84,(void *)this);
	if(res != 0){
	  ((CLP*)m_parent)->m_log->WriteLog("Thread creation failed");
	  m_amhsRunning = false;
	  return 1;
	}
	*/
	return 0;
}
int CE84::DisableAMHS() {//Stop AmhsThreadE84 and AMHS
	if (!m_amhsRunning) {
		((CLP*)m_parent)->m_log->WriteLog("E84 Already Disabled: No Harm Disable");
	}
	m_amhsRunning = false;
	usleep(200000);//200ms
	W_E84OUTPUT(SRC_HOST_ene84_DisableAMHS, 0);
	return 0;
}
int CE84::SetLTC(int endis) {
	if (((CLP*)m_parent)->m_brother == NULL)
		return 1;
	CE84 *pE84Brother = ((CLP*)m_parent)->m_brother->m_e84;
	if (pE84Brother == NULL)
		return 2;
	int res = 0;
	//control C5=0,Resetin=24V; C4=1,Testin=0V; C2=1,Interlockin=0V
	unsigned char ctrl;//xxC5C4_xC2xx
	if (endis == 0)
		ctrl = 0x00;
	else
		ctrl = 0x14;//xx01_x1xx
	if (m_lpID == 1) {
		res = this->W_NONE84OUT(ctrl);
	}
	else if (m_lpID == 2) {
		res = pE84Brother->W_NONE84OUT(ctrl);
	}
	return res;
}
int CE84::SetLTC_LED(int onoff) {
	if (((CLP*)m_parent)->m_brother == NULL)
		return 1;
	CE84 *pE84Brother = ((CLP*)m_parent)->m_brother->m_e84;
	if (pE84Brother == NULL)
		return 2;
	int res = 0;
	//control D5=0; D4=0; D2=onoff,LED
	unsigned char ctrl;//xxD5D4_xD2xx
	if (onoff == 0)
		ctrl = 0x00;//xx00_x0xx
	else
		ctrl = 0x04;//xx00_x1xx
	if (m_lpID == 1) {
		res = pE84Brother->W_NONE84OUT(ctrl);
	}
	else if (m_lpID == 2) {
		res = this->W_NONE84OUT(ctrl);
	}

	pE84Brother->bSetLTC_LED_ON = (onoff == 0) ? false : true;

	return res;
}
void CE84::DoTest() {
	EnableAMHS();
L:  char ch;
	read(0, &ch, 1);
	if (ch != 'a' || ch == '\n')goto L;
	DisableAMHS();
	return;
	W_L_REQ(1);
	sleep(1);
	W_U_REQ(1);
	sleep(1);
	W_READY(1);
	sleep(1);
	W_HO_AVBL(1);
	sleep(1);
	W_ES(1);
	sleep(1);
	W_L_REQ(0);
	sleep(1);
	W_U_REQ(0);
	sleep(1);
	W_READY(0);
	sleep(1);
	W_HO_AVBL(0);
	sleep(1);
	W_ES(0);

	int VALID, CS_0, CS_1, LTCIN, TR_REQ, BUSY, COMPT, CONT;
	int pVALID, pCS_0, pCS_1, pLTCIN, pTR_REQ, pBUSY, pCOMPT, pCONT;
	for (int i = 0; i < 1000; i++) {
		R_INPUT(&VALID, &CS_0, &CS_1, &LTCIN, &TR_REQ, &BUSY, &COMPT, &CONT);
		if (VALID != pVALID && VALID == 1) {
			printf("VALID on\n");
			pVALID = VALID;
		}
		if (VALID != pVALID && VALID == 0) {
			printf("VALID off\n");
			pVALID = VALID;
		}

		if (CS_0 != pCS_0 && CS_0 == 1) {
			printf("CS_0 on\n");
			pCS_0 = CS_0;
		}
		if (CS_0 != pCS_0 && CS_0 == 0) {
			printf("CS_0 off\n");
			pCS_0 = CS_0;
		}

		if (CS_1 != pCS_1 && CS_1 == 1) {
			printf("CS_1 on\n");
			pCS_1 = CS_1;
		}
		if (CS_1 != pCS_1 && CS_1 == 0) {
			printf("CS_1 off\n");
			pCS_1 = CS_1;
		}

		if (TR_REQ != pTR_REQ && TR_REQ == 1) {
			printf("TR_REQ on\n");
			pTR_REQ = TR_REQ;
		}
		if (TR_REQ != pTR_REQ && TR_REQ == 0) {
			printf("TR_REQ off\n");
			pTR_REQ = TR_REQ;
		}

		if (BUSY != pBUSY && BUSY == 1) {
			printf("BUSY on\n");
			pBUSY = BUSY;
		}
		if (BUSY != pBUSY && BUSY == 0) {
			printf("BUSY off\n");
			pBUSY = BUSY;
		}

		if (COMPT != pCOMPT && COMPT == 1) {
			printf("COMPT on\n");
			pCOMPT = COMPT;
		}
		if (COMPT != pCOMPT && COMPT == 0) {
			printf("COMPT off\n");
			pCOMPT = COMPT;
		}

		if (CONT != pCONT && CONT == 1) {
			printf("CONT on\n");
			pCONT = CONT;
		}
		if (CONT != pCONT && CONT == 0) {
			printf("CONT off\n");
			pCONT = CONT;
		}
		usleep(100000);
	}
}
void CE84::AmhsThreadBody() {
	CTas300 *pTas = ((CLP*)m_parent)->m_tas300;
	CE84 *pE84Brother = ((CLP*)m_parent)->m_brother->m_e84;
	int ltEnabled;
	int res;
	char status[10];//keep e84 status
	int VALID, CS_0, CS_1, LTCIN, TR_REQ, BUSY, COMPT, CONT;
	int L_REQ, U_REQ, READY, HO_AVBL, ES;
	int CS;
	int CS_OTHER;
	int counter, carrier_stable_counter, syn_counter;
	int counter_BUSY_COMPT_overlap = 0;//for Samsung
	int counter_retries = 0;
	int scale = 10;//1 second = 10 * 100ms
	int tp1 = m_tp1 * scale;//in 100ms unit
	int tp2 = m_tp2 * scale;
	int tp3 = m_tp3 * scale;
	int tp4 = m_tp4 * scale;
	int tp5 = m_tp5 * scale;
	//int tp6 = m_tp6 * scale;
	//int ta1 = m_ta1 * scale;
	//int ta2 = m_ta2 * scale;
	//int ta3 = m_ta3 * scale;
	int td0 = m_td0 * scale;
	//int td1 = m_td1 * scale;
	((CLP*)m_parent)->m_log->WriteLog("AMHS thread started!");

START:
	//_LTCEnDis config change only takes effect at
	//the beginning of each big loop
	ltEnabled = _LTCEnDis;//0: disable; 1: enable
	///////////////////////////////////////
	//E84 initialize
	if (m_amhsRunning) {
		counter = 0;
		carrier_stable_counter = 0;
		syn_counter = 0;
		sprintf(status, "0x1");
		m_e84State = E84_ENABLED;//!!
		res = 0;
		//if(ltEnabled == 1)
		//	res = SetLTC_LED(1);
		///////////////////////////////////////////
		//Change according to Robert
		res = R_INPUT(&VALID, &CS_0, &CS_1, &LTCIN, &TR_REQ, &BUSY, &COMPT, &CONT);
		if (res != 0) {//should not happen
			AbortSequence(status);
			m_amhsRunning = false;
			goto START;
		}
		if (VALID != 0 || CS_0 != 0 || CS_1 != 0 || TR_REQ != 0 || BUSY != 0 || COMPT != 0 || CONT != 0) {
			ErrSignalProcess(status);
			m_amhsRunning = false;
			goto START;
		}

		//End change according to Robert
		///////////////////////////////////////////
		res += W_ES(1);
		res += W_HO_AVBL(1);
		if (res != 0) {//should not happen
			AbortSequence(status);
			m_amhsRunning = false;
		}
		else {
			((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_ENABLED");
			((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
		}
	}
	////////////////////////////////////////
	//E84 Loop
	while (m_amhsRunning) {
		////////////////////////////////////////////////////////////
		//constantly update possible constant change from Up machine.
		if (m_constChanged) {
			tp1 = m_tp1 * scale;//in 100ms unit
			tp2 = m_tp2 * scale;
			tp3 = m_tp3 * scale;
			tp4 = m_tp4 * scale;
			tp5 = m_tp5 * scale;
			//tp6 = m_tp6 * scale;
			//ta1 = m_ta1 * scale;
			//ta2 = m_ta2 * scale;
			//ta3 = m_ta3 * scale;
			td0 = m_td0 * scale;
			//td1 = m_td1 * scale;
			m_constChanged = false;
		}
		//end update possible constant change from Up machine.
		//////////////////////////////////////////////////////////////
		counter++;
		res = R_INPUT(&VALID, &CS_0, &CS_1, &LTCIN, &TR_REQ, &BUSY, &COMPT, &CONT);
		if (res != 0) {//should not happen
			AbortSequence(status);
			m_amhsRunning = false;
			goto START;
		}
		//===============================================================================
		//[V.127]Leo Added:Read Output signal and judge "HO_AVBL" for LightCurtai be triggered.
		//===============================================================================	
		res = R_OUTPUT(&L_REQ, &U_REQ, &READY, &HO_AVBL, &ES);
		if (res != 0) {//should not happen
			AbortSequence(status);
			m_amhsRunning = false;
			goto START;
		}

		if ((ltEnabled == 1 && VALID == 1) || (ltEnabled == 2 && HO_AVBL == 1)) {
			if (LTCIN != _LTCOnLevel) {
				//light curtain beams are interrupted
				//res = W_ES(0);//changed to do inside LTCViolated
				LTCViolated(status);
				m_amhsRunning = false;
				goto START;
			}
		}

		if (m_mchStatus & BIT0_MCH_DOOR) {//Door opened
			res = W_ES(0);
			MchDoorOpened(status);
			m_amhsRunning = false;
			goto START;
		}

		//if(CS_0==1 || CS_1==1)
		//  CS = 1;
		//else
		//  CS = 0;	
		CS = CS_0;
		CS_OTHER = CS_1;

		if (m_e84State != E84_BUSY_ON) {
			if (pTas->m_fpEvent == FPEVT_SMTON || pTas->m_fpEvent == FPEVT_ABNST) {
				ErrFpStatus_Ready(status);
				m_amhsRunning = false;
				goto START;
			}
		}

		switch (m_e84State) {
		case E84_ENABLED:
			if (pTas->m_fpStatus == FPS_NOFOUP) {
				if (m_e84LDULD != 0) {
					m_e84LDULD = 0;//LOAD CYCLE
					((CLP*)m_parent)->m_log->WriteLog("AMHS Loading cycle");
				}
			}
			else if (pTas->m_fpStatus == FPS_PLACED ||
				pTas->m_fpStatus == FPS_CLAMPED) {
				if (m_e84LDULD != 1) {
					m_e84LDULD = 1;//UNLOAD CYCLE
					if (pTas->m_fpStatus == FPS_PLACED)
						((CLP*)m_parent)->m_log->WriteLog("AMHS Unloading cycle: Placed");
					else
						((CLP*)m_parent)->m_log->WriteLog("AMHS Unloading cycle: Clamped");
				}
			}
			else {//should not happen
				ErrFpStatus_Busy(status);
				m_amhsRunning = false;
				goto START;
			}

			if (ltEnabled == 2 && HO_AVBL == 1 && bSetLTC_LED_ON == false)
			{
				res = SetLTC_LED(1);
				if (res != 0) {//should not happen
					AbortSequence(status);
					m_amhsRunning = false;
				}
			}

			if (CS == 1) {
				if (TR_REQ == 0 && BUSY == 0 && COMPT == 0 && CS_OTHER == 0) {
					m_e84State = E84_CS_ON;//!!
					counter = 0;//reset counter for next state
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_CS_ON");
					sprintf(status, "0x2");
					((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
				}
				else {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
			}
			else {//if(CS==0)
				if (VALID != 0 || TR_REQ != 0 || BUSY != 0 || COMPT != 0 || CS_OTHER != 0) {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				/*//this comment out is for Samsung's non E84 conforming practice
				if(CONT==1){
				  if(counter > tp6+td1){//VALID_OF to VALID_ON timed out
					TimeOutProcess("0x3c50", status);
					m_amhsRunning = false;
					goto START;
				  }//else stay in E84_ENABLED state and let the counter ++...
				}//else stay in in E84_ENABLED state, no timeout
				*/
			}
			break;
		case E84_CS_ON:
			if (VALID == 1) {
				if (CS == 1 && TR_REQ == 0 && BUSY == 0 && COMPT == 0 && CS_OTHER == 0) {
					m_e84State = E84_VALID_ON;//!!
					counter = 0;//reset counter for next state
					if (ltEnabled == 1 && bSetLTC_LED_ON == false)
					{
						res = SetLTC_LED(1);
						if (res != 0) {//should not happen
							AbortSequence(status);
							m_amhsRunning = false;
						}
					}
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_VALID_ON");
					//According to Brooks spec, CS0_ON to VALID_ON no report
				}
				else {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
			}
			else {//if(VALID==0)
				if (CS != 1 || TR_REQ != 0 || BUSY != 0 || COMPT != 0 || CS_OTHER != 0) {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				else {
					if (counter > td0) {//CS0_ON to VALID_ON timed out
						TimeOutProcess("0x3c50", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_CS_ON state and let the counter ++...
				}
			}
			break;
		case E84_VALID_ON:
			if (m_e84LDULD == 0) {//LOAD CYCLE
				res = W_L_REQ(1);
				if (res != 0) {//should not happen
					AbortSequence(status);
					m_amhsRunning = false;
					goto START;
				}
				m_e84State = E84_LUREQ_ON;//!!
				counter = 0;//reset counter for next state
				((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_LUREQ_ON");
				sprintf(status, "0x10");
				((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
			}
			else {//UNLOAD CYCLE
				if (_N2PurgeNozzleDown_InE84 != 0)
				{
					if (counter < 15)//Wait for N2 purge close
						break;
				}
				res = W_U_REQ(1);
				if (res != 0) {//should not happen
					AbortSequence(status);
					m_amhsRunning = false;
					goto START;
				}
				m_e84State = E84_LUREQ_ON;//!!
				counter = 0;//reset counter for next state
				((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_LUREQ_ON");
				sprintf(status, "0x20");
				((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
			}
			break;
		case E84_LUREQ_ON:
			if (TR_REQ == 1) {
				if (CS == 1 && VALID == 1 && BUSY == 0 && COMPT == 0 && CS_OTHER == 0) {
					m_e84State = E84_TRREQ_ON;//!!
					counter = 0;//reset counter for next state
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_TRREQ_ON");
					if (m_e84LDULD == 0)//LOAD CYCLE
						sprintf(status, "0x11");
					else//UNLOAD CYCLE
						sprintf(status, "0x21");
					((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
				}
				else {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
			}
			else {//if(TR_REQ==0)
				if (CS != 1 || VALID != 1 || BUSY != 0 || COMPT != 0 || CS_OTHER != 0) {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				else {
					if (counter > tp1) {//E84_LUREQ_ON to E84_TRREQ_ON timed out
						TimeOutProcess("0x3c51", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_LUREQ_ON state and let the counter ++...
				}
			}
			break;
		case E84_TRREQ_ON:
			if (pTas->m_fpStatus == FPS_CLAMPED) {
				if (_N2PurgeNozzleDown_InE84 != 0)
				{
					if (counter < 5)//Continue to wait for N2 purge close
						break;
					res = pTas->movOP("BPNDW");
					if (res != 0) {//command can not be executed
						AbortSequence(status);
						((CLP*)m_parent)->E84_8031_Event(res);
						m_amhsRunning = false;
						goto START;
					}
					else
						((CLP*)m_parent)->E84_8031_Event(0);
				}

				counter_retries = 0;
				do {
					res = pTas->movOP("PODOP");//("CULOD");//use this to speed up
					usleep(100000);//100ms
					counter_retries++;
				} while (res != 0 && counter_retries < 10);
				//res = pTas->movOP("PODOP");//("CULOD");//use this to speed up
				if (res != 0) {//command can not be executed
					AbortSequence(status);
					((CLP*)m_parent)->E84_8031_Event(100 + res);
					m_amhsRunning = false;
					goto START;
				}
				else
					((CLP*)m_parent)->E84_8031_Event(200 + counter_retries);

				res = ((CLP*)m_parent)->GetFxlAmhsStatus();
				if (res != 0 || pTas->m_fpStatus > FPS_PLACED) {
					((CLP*)m_parent)->E84_st_chg("0x3c57", status);//report e84 error
					((CLP*)m_parent)->m_log->WriteLog("AMHS stoped!");
					if (_PreservE84FieldSignal == 0)
						W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
					else
						W_HO_AVBL(0);
					m_amhsRunning = false;
					goto START;
				}
			}

			res = W_READY(1);
			if (res != 0) {//should not happen
				AbortSequence(status);
				m_amhsRunning = false;
				goto START;
			}
			m_e84State = E84_READY_ON;//!!
			counter = 0;//reset counter for next state
			((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_READY_ON");
			if (m_e84LDULD == 0)//LOAD CYCLE
				sprintf(status, "0x12");
			else
				sprintf(status, "0x22");
			((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
			break;
		case E84_READY_ON:
			if (BUSY == 1) {
				if (CS == 1 && VALID == 1 && TR_REQ == 1 && COMPT == 0 && CS_OTHER == 0) {
					m_e84State = E84_BUSY_ON;//!!
					counter = 0;//reset counter for next state
					carrier_stable_counter = 0;
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_BUSY_ON");
					if (m_e84LDULD == 0)//LOAD CYCLE
						sprintf(status, "0x13");
					else//UNLOAD CYCLE
						sprintf(status, "0x23");
					((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
				}
				else {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
			}
			else {//if(BUSY==0)
				if (CS != 1 || VALID != 1 || TR_REQ != 1 || COMPT != 0 || CS_OTHER != 0) {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				else {
					if (counter > tp2) {//E84_READY_ON to E84_BUSY_ON timed out
						TimeOutProcess("0x3c52", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_READY_ON state and let the counter ++...
				}
			}
			break;
		case E84_BUSY_ON:
			if (CS != 1 || VALID != 1 || TR_REQ != 1 || BUSY == !1 || COMPT != 0 || CS_OTHER != 0) {
				ErrSignalProcess(status);
				m_amhsRunning = false;
				goto START;
			}

			if (m_e84LDULD == 0) {//LOAD CYCLE
#ifdef _OfflineTest
				if (counter > tp3 / 3)
					pTas->m_fpStatus = FPS_PLACED;//for test only	
#endif
				if (pTas->m_fpStatus == FPS_PLACED && pTas->m_fpEvent == FPEVT_PODON) {
					carrier_stable_counter++;
					//if(carrier_stable_counter > 45){//stable for 4~5 seconds
					if (carrier_stable_counter > _FoupStable_debouce) {
						res = W_L_REQ(0);
						if (res != 0) {//should not happen
							AbortSequence(status);
							m_amhsRunning = false;
							goto START;
						}
						m_e84State = E84_LUREQ_OF;//!!
						counter = 0;//reset counter for next state
						((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_LUREQ_OF");
						sprintf(status, "0x14");
						((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
					}
				}
				else {//if(pTas->m_fpStatus!=FPS_PLACED)
					carrier_stable_counter = 0;
					if (counter > tp3) {//E84_BUSY_ON to E84_LUREQ_OF timed out
						TimeOutProcess("0x3c53", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_BUSY_ON state and let the counter ++...
				}
			}
			else {//if(m_e84LDULD == 1)//UNLOAD CYCLE
#ifdef _OfflineTest
				if (counter > tp3 / 3)
					pTas->m_fpStatus = FPS_NOFOUP;//for test only	
#endif
				if (pTas->m_fpStatus == FPS_NOFOUP && pTas->m_fpEvent == FPEVT_PODOF) {
					carrier_stable_counter++;
					//if(carrier_stable_counter > 45){//orginal
					if (carrier_stable_counter > _FoupStable_debouce) {
						res = W_U_REQ(0);
						if (res != 0) {//should not happen
							AbortSequence(status);
							m_amhsRunning = false;
							goto START;
						}
						m_e84State = E84_LUREQ_OF;//!!
						counter = 0;//reset counter for next state
						counter_BUSY_COMPT_overlap = 0;
						((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_LUREQ_OF");
						sprintf(status, "0x24");
						((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
					}
				}
				else {//if(pTas->m_fpStatus!=FPS_NOFOUP)
					carrier_stable_counter = 0;
					if (counter > tp3) {//E84_BUSY_ON to E84_LUREQ_OF timed out
						TimeOutProcess("0x3c53", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_BUSY_ON state and let the counter ++...
				}
			}
			break;
		case E84_LUREQ_OF:
			if (COMPT == 1) {
				//if(CS==1 && VALID==1 && TR_REQ==0 && BUSY==0 && CS_OTHER==0){//Robert 06/23
				if (CS == 1 && VALID == 1 && BUSY == 0 && CS_OTHER == 0) {//don't check TR_REQ, as caused by BUSY off not COMPT on
					m_e84State = E84_COMPT_ON;//!!
					counter = 0;//reset counter for next state
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_COMPT_ON");
					if (m_e84LDULD == 0)//LOAD CYCLE
						sprintf(status, "0x15");
					else//UNLOAD CYCLE
						sprintf(status, "0x25");
					((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
				}
				else {
					counter_BUSY_COMPT_overlap++;
					if (counter_BUSY_COMPT_overlap > 2)//added (BUSY==1 and COMPT==1)overlap (200ms) tolerance for Samsung 
					{
						ErrSignalProcess(status);
						m_amhsRunning = false;
						goto START;
					}
				}
			}
			else {//if(COMPT==0)
				if (CS != 1 || VALID != 1 || CS_OTHER != 0) {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				else {
					if (counter > tp4) {//E84_LUREQ_OF to E84_COMPT_ON timeout
						TimeOutProcess("0x3c54", status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_LUREQ_OF state and let the counter ++...
				}
			}
			break;
		case E84_COMPT_ON:
			res = W_READY(0);
			if (res != 0) {//should not happen
				AbortSequence(status);
				m_amhsRunning = false;
				goto START;
			}
			m_e84State = E84_READY_OF;//!!
			counter = 0;//reset counter for next state
			((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_READY_OF");
			if (m_e84LDULD == 0)//LOAD CYCLE
				sprintf(status, "0x16");
			else
				sprintf(status, "0x26");
			((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
			break;
		case E84_READY_OF:
			if (VALID == 0 && CS == 0 && COMPT == 0) {
				//if(TR_REQ==0 && BUSY==0 && CS_OTHER==0){//Robert 06/23
				if (BUSY == 0 && CS_OTHER == 0) {//check TR_REQ==0 may cause problem
					m_e84State = E84_VALID_OF;//!!
					counter = 0;//reset counter for next state
					((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_VALID_OF");
					if (m_e84LDULD == 0)//LOAD CYCLE
						sprintf(status, "0x17");
					else//UNLOAD CYCLE
						sprintf(status, "0x27");
					((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
				}
				else {
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
			}
			else {//if(VALID!=0 || CS!=0 || COMPT!=0)
			  //if(TR_REQ!=0 || BUSY!=0 || CS_OTHER!=0){//Robert 06/23
				if (BUSY != 0 || CS_OTHER != 0) {//check TR_REQ==0 may cause problem
					ErrSignalProcess(status);
					m_amhsRunning = false;
					goto START;
				}
				else {
					if (VALID == 0 && COMPT != 0) {
						syn_counter++;
						if (syn_counter >= 2) {
							ErrSignalProcess(status);
							m_amhsRunning = false;
							goto START;
						}
					}

					if (counter > tp5) {//E84_READY_OF to E84_VALID_OF timeout
						if (VALID != 0)
							TimeOutProcess("0x3c55", status);
						else//According to Kevin
							ErrSignalProcess(status);
						m_amhsRunning = false;
						goto START;
					}//else stay in E84_READY_OF state and let the counter ++...
				}
			}
			break;
		case E84_VALID_OF:
			if (ltEnabled == 1)
			{
				if (bSetLTC_LED_ON == true) {
					res = SetLTC_LED(0);
					if (res != 0) {//should not happen
						AbortSequence(status);
						m_amhsRunning = false;
					}
				}
			}
			if (_ESMode == 1 && m_e84LDULD == 0)
			{
				res = W_HO_AVBL(0);//A strange requirement by one customer: RexChip
				if (res != 0) {//should not happen
					AbortSequence(status);
					m_amhsRunning = false;
				}
			}
			m_e84State = E84_ENABLED;//!!
			counter = 0;//reset counter for next state
			((CLP*)m_parent)->m_log->WriteLog("E84 State => E84_ENABLED");

			//////////////////////////////////////////
			//Special Requirement According to Kevin
			((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
			if (_PreservE84FieldSignal == 0)
				W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
			else
				W_HO_AVBL(0);
			m_amhsRunning = false;

			sprintf(status, "0x0");//disabled
			((CLP*)m_parent)->E84_st_chg("0x0", status);//report e84 status
			//End According to Kevin
			//////////////////////////////////////////

			break;
		}
		//usleep(100000);//100ms: actually more than 100, considering code execute
		usleep(99000);
	}
	//End E84 Loop
	///////////////////////////////////////////
	//Now e84 in this lp has stopped running
	if (ltEnabled == 1) {
		if (pE84Brother->m_amhsRunning == false) {//e84 in other lp has stopped running too
			res = SetLTC_LED(0);
			if (res != 0)
				((CLP*)m_parent)->m_log->WriteLog("In E84 idle loop: turn off LTC led failed");
		}
		else if (pE84Brother->m_e84State == E84_ENABLED) {//e84 in other lp is running, but waiting for E84_CS_ON
			res = SetLTC_LED(0);//between E84_CS_ON to E84_VALID_ON, there are no SetLTC_LED(0) action!
			if (res != 0)
				((CLP*)m_parent)->m_log->WriteLog("In E84 idle loop: turn off LTC led failed");
		}
	}
	else if (ltEnabled == 2) {
		if (pE84Brother->m_amhsRunning == false) {//e84 in other lp has stopped running too
			res = SetLTC_LED(0);
			if (res != 0)
				((CLP*)m_parent)->m_log->WriteLog("In E84 idle loop: turn off LTC led failed");

		}

	}

	usleep(100000);//100ms
	goto START;
	//((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	//((CLP*)m_parent)->m_log->WriteLog("AMHS thread stoped!");
}

int CE84::W_E84OUTPUT(int src, unsigned char output) {
	//only used for set e84 part of m_Output to output, 0 for example 
	//preserve C5C4&C2 in C7C6C5C4_C3C2C1C0 (if m_wPort==2 in lp1) or
	//preserve D5D4&D2 in D7D6D5D4_D3D2D1D0 (if m_wPort==3 in lp2)
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0x34;//mask1 = 0011 0100
	out |= (output & 0xCB);//mask2 = 1100 1011

	if (_ESMode == 1) {//ES Always ON Mode
		if (src != SRC_HOST_out_e84_Brkout_e84) {//don't operate on es signal(bit 7)
			out = m_Output;
			out &= 0xB4;//mask1 = 1011 0100
			out |= (output & 0x4B);//mask2 = 0100 1011
		}
	}

	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_NONE84OUT(unsigned char output) {
	//only used for set non-e84 part of m_Output to output 
	//preserve C7C6_C3C1C0 in C7C6C5C4_C3C2C1C0 (if m_wPort==2 in lp1) or
	//preserve D7D6_D3D1D0 in D7D6D5D4_D3D2D1D0 (if m_wPort==3 in lp2)
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0xCB;//mask2 = 1100 1011
	out |= (output & 0x34);//mask1 = 0011 0100
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_L_REQ(int onoff) {//1:on; 0:off
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0xFE;
	out |= onoff;
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_U_REQ(int onoff) {
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0xFD;
	out |= (onoff << 1);
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_READY(int onoff) {
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0xF7;
	out |= (onoff << 3);
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_HO_AVBL(int onoff) {
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0xBF;
	out |= (onoff << 6);
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::W_ES(int onoff) {
	unsigned char out;
	sem_wait(&m_semOutput);
	out = m_Output;
	out &= 0x7F;
	out |= (onoff << 7);
	m_Output = out;
	//int status = SeaIo_WriteReg(m_hDev, m_wPort, m_Output);
	int status = SeaIo_WriteByte(m_hDev, m_wPort, m_Output, Absolute);
	sem_post(&m_semOutput);
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not write to dio device");
		return 1;
	}
	return 0;
}
int CE84::R_INPUT(int *VALID, int *CS_0, int *CS_1, int *LTCIN,
	int *TR_REQ, int *BUSY, int *COMPT, int *CONT) {
	//unsigned char buffer[4];
	unsigned char dataByte;
	static bool reading = false;
	if (reading)
		usleep(10);
	reading = true;
	//int status = SeaIo_ReadReg(m_hDev, m_rPort, buffer, length, &length);
	int status = SeaIo_ReadByte(m_hDev, m_rPort, &dataByte, Absolute);
	reading = false;
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not read from e84 input port");
		return 1;

	}
	//m_Input = buffer[0];
	m_Input = dataByte;
	if ((m_Input & 0x01))
		*VALID = 1;
	else
		*VALID = 0;
	if ((m_Input & 0x02))
		*CS_0 = 1;
	else
		*CS_0 = 0;
	if ((m_Input & 0x04))
		*CS_1 = 1;
	else
		*CS_1 = 0;
	if ((m_Input & 0x08))
		*LTCIN = 1;
	else
		*LTCIN = 0;
	if ((m_Input & 0x10))
		*TR_REQ = 1;
	else
		*TR_REQ = 0;
	if ((m_Input & 0x20))
		*BUSY = 1;
	else
		*BUSY = 0;
	if ((m_Input & 0x40))
		*COMPT = 1;
	else
		*COMPT = 0;
	if ((m_Input & 0x80))
		*CONT = 1;
	else
		*CONT = 0;

	return 0;
}
int CE84::R_OUTPUT(int *L_REQ, int *U_REQ, int *READY, int *HO_AVBL,
	int *ES) {
	unsigned char temp_Output;//Data read from Output port 
	//unsigned char buffer[4];
	unsigned char dataByte;
	static bool reading = false;
	if (reading)
		usleep(10);
	reading = true;
	//int status = SeaIo_ReadReg(m_hDev, m_wPort, buffer, length, &length);
	int status = SeaIo_ReadByte(m_hDev, m_wPort, &dataByte, Absolute);
	reading = false;
	if (status != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Could not read from e84 output port");
		return 1;
	}
	//temp_Output = buffer[0];
	temp_Output = dataByte;
	if ((temp_Output & 0x01))
		*L_REQ = 1;
	else
		*L_REQ = 0;
	if ((temp_Output & 0x02))
		*U_REQ = 1;
	else
		*U_REQ = 0;
	if ((temp_Output & 0x08))
		*READY = 1;
	else
		*READY = 0;
	if ((temp_Output & 0x40))
		*HO_AVBL = 1;
	else
		*HO_AVBL = 0;
	if ((temp_Output & 0x80))
		*ES = 1;
	else
		*ES = 0;

	return 0;
}
int CE84::sem_reset(sem_t *sem)
{
	int res;
	while ((res = sem_trywait(sem)) == 0);
	//while((res = sem_trywait(&m_semInfCmdACK)) == 0)
	//  ((CLP*)m_parent)->m_log->WriteLog("sem_trywait succeeded!");
	if (res != -1)
		return 1;//internal error
	if (errno == EAGAIN) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EAGAIN");
		return 0;
	}
	if (errno == EDEADLK) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EDEADLK");
		return 2;//deadlock detected
	}
	if (errno == EINVAL) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINVAL");
		return 3;//invalid semaphore descriptor sem
	}
	if (errno == EINTR) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINTR");
		return 4;//A signal interrupted sem_trywait function
	}
	return 5;//unknown errno
}

void CE84::ErrSignalProcess(char *status) {
	((CLP*)m_parent)->E84_st_chg("0x3c56", status);//err: Unexpected signal
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Unexpected signal: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(false);
}

void CE84::ErrFpStatus_Ready(char *status) {
	((CLP*)m_parent)->E84_st_chg2("0x3c56", status);//err: treated as Protocol error
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Unexpected foup status: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(true);
}
void CE84::ErrFpStatus_Busy(char *status) {//null terminated string
	((CLP*)m_parent)->E84_st_chg2("0x3c57", status);//err: Incorrect foup status
	((CLP*)m_parent)->m_fxlamhsState = fxl_BUSY;
	((CLP*)m_parent)->m_log->WriteLog("fpStatus Not Ready: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(true);
}
void CE84::TimeOutProcess(char *evtcode, char *status) {//null termed string
	((CLP*)m_parent)->E84_st_chg(evtcode, status);
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Timeout: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(false);
}
void CE84::AbortSequence(char *status) {//null terminated string
	((CLP*)m_parent)->E84_st_chg("0x3c58", status);//err: Sequence aborted
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Read/Write dio err: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(false);
}
void CE84::LTCViolated(char *status) {//null terminated string
	((CLP*)m_parent)->E84_st_chg("0x3c60", status);//err: LTC violated
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Lightcurtain Violated: AMHS stoped!");
	W_ES(0);
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(false);
}
void CE84::MchDoorOpened(char *status) {//null terminated string
	((CLP*)m_parent)->E84_st_chg("0x3c61", status);//err: Machine Door Opened
	((CLP*)m_parent)->m_fxlamhsState = fxl_READY;
	((CLP*)m_parent)->m_log->WriteLog("Machine Door Opened: AMHS stoped!");
	if (_PreservE84FieldSignal == 0)
		W_E84OUTPUT(SRC_LOCAL_AmhsThreadBody, 0);
	else
		W_HO_AVBL(0);
	((CLP*)m_parent)->E84_8030_Event(false);
}
void *AmhsThreadE84(void *pObj) {
	CE84 *pE84 = (CE84 *)pObj;
	pE84->AmhsThreadBody();
	return pE84;  //add return Leon20210219
}
//End E84 Class Implementation
//////////////////////////////////

//////////////////////////////////
//Load Port Class Implementation
CLP::CLP(int lpID) {
	char s[100];
	m_lpID = lpID;
	sprintf(s, "%dLP", m_lpID);
	m_log = new CLog(s);
	sprintf(s, "CLP Constructor called, lpID= %d", m_lpID);
	m_log->WriteLog("\n----------Program Starting----------");
	m_log->WriteLog(s);
	offonStatus = -1;
	m_host = new CHostBK(m_lpID, (void *)this);
	m_tas300 = new CTas300(m_lpID, (void *)this);
	m_bl600 = NULL;
	m_hmos = NULL;
	m_omron = NULL;
	int IdReaderSelector;
	if (m_lpID == 1)
		IdReaderSelector = _LP1IdReader;
	else
		IdReaderSelector = _LP2IdReader;
	if (IdReaderSelector == 1) {//1: use barcode
		m_bl600 = new CBL600(m_lpID, (void *)this);
	}
	else if (IdReaderSelector == 2) {//2: use Hermos RFID
		m_hmos = new CHermos(m_lpID, (void *)this);
	}
	else if (IdReaderSelector == 3) {//3: use Omron RFID(ASCII)
		m_omron = new COmron(m_lpID, OM_ASCII, (void *)this);
	}
	else if (IdReaderSelector == 4) {//4: use Omron RFID(HEX)
		m_omron = new COmron(m_lpID, OM_HEX, (void *)this);
	}
	m_e84 = new CE84(m_lpID, (void *)this);
	m_brother = NULL;
	m_pCfg = NULL;
	m_prgState = prg_NOTINIT;
	m_fxlamhsState = fxl_NOTINIT;
	for (int i = 0; i < 25; i++)
		m_mapRes[i] = '0';
	m_mapRes[25] = '\0';
	m_updatefilelen = 0;
	m_receivedlen = 0;
	m_bacceptingfile = false;
	m_log->WriteLog("PRG state => prg_NOTINIT");
}
CLP::~CLP() {
	delete m_host;
	delete m_tas300;
	if (m_bl600)
		delete m_bl600;
	if (m_hmos)
		delete m_hmos;
	if (m_omron)
		delete m_omron;
	delete m_e84;
	m_log->WriteLog("CLP Destructor called");
	delete m_log;//delete this one last, as object m_host etc use it
}
int CLP::GetFxlAmhsStatus() {
	int res = m_tas300->statfxl();
	if (res == 0) {
		m_log->WriteLog("Request ACKed", m_tas300->m_statfxl, strlen(m_tas300->m_statfxl));
		if (m_tas300->m_fpStatus == 0)
			m_log->WriteLog("FPS_NOFOUP");
		else if (m_tas300->m_fpStatus == 1)
			m_log->WriteLog("FPS_PLACED");
		else if (m_tas300->m_fpStatus == 2)
			m_log->WriteLog("FPS_CLAMPED");
		else if (m_tas300->m_fpStatus == 3)
			m_log->WriteLog("FPS_DOCKED");
		else if (m_tas300->m_fpStatus == 4)
			m_log->WriteLog("FPS_OPENED");
		else if (m_tas300->m_fpStatus == -1)
			m_log->WriteLog("FPS_UNKNOWN");

		if (m_fxlamhsState != fxl_AMHS) {//if AMHS enabled, E84 Class only cares for m_fpStatus
			if (m_tas300->m_Status.inited == '0') {
				m_fxlamhsState = fxl_NOTINIT;
				m_log->WriteLog("FXL_AMHS state => fxl_NOTINIT");
			}
			else {
				if (m_tas300->m_fpStatus <= FPS_PLACED) {//FPS_NOFOUP or FPS_PLACED, NOTE: if res = m_tas300->
													   //statfxl() succeed, m_fpStatus >= FPS_NOFOUP
					m_fxlamhsState = fxl_READY;
					m_log->WriteLog("FXL_AMHS state => fxl_READY");
				}
				else {
					m_fxlamhsState = fxl_BUSY;
					m_log->WriteLog("FXL_AMHS state => fxl_BUSY");
				}
			}
		}
	}
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in Statfxl()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in Statfxl()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in Statfxl()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Incorrect Param Length");
	else if (res == 8)
		m_log->WriteLog("Incorrect Parameters");
	else if (res == 9)
		m_log->WriteLog("m_fpStatus and m_statfxl not updated");

	return res;
}
int CLP::EnableOperation() {
	m_log->WriteLog("Enable Load Port Operation");
	if (m_prgState != prg_NOTINIT)
		return 1;

	int res = m_e84->OpenDevice();
	if (res != 0) {
		m_log->WriteLog("Open Device for E84 failed");
		return 2;
	}

	res = m_e84->CreateE84Thread();
	if (res != 0) {
		m_log->WriteLog("Create Thread for E84 failed");
		return 3;
	}

	res = m_host->OpenPort();
	if (res != 0) {
		m_log->WriteLog("Open Port for HostBK failed");
		return 4;
	}

	res = m_tas300->OpenPort();
	if (res != 0) {
		m_log->WriteLog("Open Port for Tas300 failed");
		return 5;
	}

	if (m_bl600) {
		res = m_bl600->OpenPort();
		if (res != 0) {
			m_log->WriteLog("Open Port for Bl600 failed");
			return 6;
		}
	}
	else if (m_hmos) {
		res = m_hmos->OpenPort();
		if (res != 0) {
			m_log->WriteLog("Open Port for Hermos failed");
			return 6;
		}
	}
	else if (m_omron) {
		res = m_omron->OpenPort();
		if (res != 0) {
			m_log->WriteLog("Open Port for Omron failed");
			return 6;
		}
	}

	res = m_e84->SetLTC(1);//set light curtain to autoreset mode
	if (res != 0) {
		m_log->WriteLog("Set light curtain failed");
		return 7;
	}

	res = m_e84->SetLTC_LED(0);//set light curtain led off
	if (res != 0) {
		m_log->WriteLog("Set light curtain led off failed");
		return 8;
	}

#ifdef _OfflineTest
	//m_tas300->m_fpStatus = FPS_NOFOUP;//for test only: load
	m_tas300->m_fpStatus = FPS_PLACED;//for test only: unload
#else
	//Decide m_fxlamhsState
	res = GetFxlAmhsStatus();
	if (res != 0) {
		m_log->WriteLog("GetFxlAmhsStatus failed");
	}
	//if(m_tas300->m_Status.ecode != 0)//commented off, send a controller start event anyway
	{
		char s[100];
		sprintf(s, "io event 0x8000 0x%02X\r\n",
			m_tas300->m_Status.ecode);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	if (m_tas300->m_Status.eqpStatus != '0'
		|| m_tas300->m_Status.mode != '0'
		|| m_tas300->m_Status.inited != '1'
		|| m_tas300->m_Status.opStatus != '0')
	{
		char s[100];
		sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
			m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
			m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
#endif
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
	return 0;
}
int CLP::DisableOperation() {
	m_log->WriteLog("Disable Load Port Operation");

	while (m_prgState != prg_READY)usleep(100000);//100ms
	m_prgState = prg_NOTINIT;
	m_log->WriteLog("PRG state => prg_NOTINIT");

	int res = m_host->ClosePort();
	if (res != 0) {
		m_log->WriteLog("Close Port for HostBK failed");
		return 1;
	}

	res = m_tas300->ClosePort();
	if (res != 0) {
		m_log->WriteLog("Close Port for Tas300 failed");
		return 2;
	}

	if (m_bl600) {
		res = m_bl600->ClosePort();
		if (res != 0) {
			m_log->WriteLog("Close Port for Bl600 failed");
			return 3;
		}
	}
	else if (m_hmos) {
		res = m_hmos->ClosePort();
		if (res != 0) {
			m_log->WriteLog("Close Port for Hermos failed");
			return 3;
		}
	}
	else if (m_omron) {
		res = m_omron->ClosePort();
		if (res != 0) {
			m_log->WriteLog("Close Port for Omron failed");
			return 3;
		}
	}

	res = m_e84->CloseDevice();
	if (res != 0) {
		m_log->WriteLog("Close Device for E84 failed");
		return 4;
	}

	return 0;
}

void CLP::TasErrorHandle() {//Used in:Brkload;Brkunload;Brkmap;
	m_log->WriteLog("Error Handling");
	int res = m_tas300->statfxl();
	if (res == 0) {
		m_log->WriteLog("Request ACKed");
		if (m_tas300->m_fpStatus == 0)
			m_log->WriteLog("FPS_NOFOUP");
		else if (m_tas300->m_fpStatus == 1)
			m_log->WriteLog("FPS_PLACED");
		else if (m_tas300->m_fpStatus == 2)
			m_log->WriteLog("FPS_CLAMPED");
		else if (m_tas300->m_fpStatus == 3)
			m_log->WriteLog("FPS_DOCKED");
		else if (m_tas300->m_fpStatus == 4)
			m_log->WriteLog("FPS_OPENED");
		else if (m_tas300->m_fpStatus == -1)
			m_log->WriteLog("FPS_UNKNOWN");
	}
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in Statfxl()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in Statfxl()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in Statfxl()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Incorrect Param Length");
	else if (res == 8)
		m_log->WriteLog("Incorrect Parameters");
	else if (res == 9)
		m_log->WriteLog("m_fpStatus and m_statfxl not updated");

	if (res != 0) {
		m_log->WriteLog("Query Tas300 status failed");
		return;
	}
	/*
	struct STATUS{
	  char eqpStatus;//'0'= normal, 'A'= recoverable error, 'E'= fatal error
	  char mode;//'0'= online, '1'= maintain;
	  char inited;//'0'= not inited, '1'= inited
	  char ecode;//binary code 0= no error
	}m_Status;
	*/
	if (m_tas300->m_Status.mode == '1') {
		m_log->WriteLog("Tas300 now in maintain mode!");
		return;
	}

	if (m_tas300->m_Status.inited == '0') {
		m_log->WriteLog("Tas300 not initialized!");
		return;
		//do ABORG here
		/*
		if(m_prgState != prg_READY){
		  m_log->WriteLog("Not prg_READY");
		  return;
		}
		m_prgState = prg_BUSY;
		m_log->WriteLog("PRG state => prg_BUSY");
		*/
		//executing command
		int res = m_tas300->movOP("ABORG");
		if (res == 0)
			m_log->WriteLog("Request INFed");
		else if (res == 1)
			m_log->WriteLog("Busy!");
		else if (res == 2)
			m_log->WriteLog("Internal error");
		else if (res == 3)
			m_log->WriteLog("Reset semaphore error");
		else if (res == 4)
			m_log->WriteLog("Wait timeout");
		else if (res == 5)
			m_log->WriteLog("Request NAKed");
		else if (res == 6)
			m_log->WriteLog("Request neither ACKed or NAKed");
		else if (res == 7)
			m_log->WriteLog("Reset semaphore 2 error");
		else if (res == 8)
			m_log->WriteLog("Wait INF timeout");
		else if (res == 9)
			m_log->WriteLog("Non ABS and non INF result");
		else if (res == 10)
			m_log->WriteLog("Request ABSed");
		/*
		m_prgState = prg_READY;
		m_log->WriteLog("PRG state => prg_READY");
		*/
	}
	else if (m_tas300->m_Status.eqpStatus == 'A') {
		char s[100];
		sprintf(s, "Tas300 in recoverable error: %2X",
			m_tas300->m_Status.ecode);
		m_log->WriteLog(s);
		int res = m_tas300->rstErr();
		if (res == 0)
			m_log->WriteLog("Request INFed");
		else if (res == 1)
			m_log->WriteLog("Busy!");
		else if (res == 2)
			m_log->WriteLog("Internal error in rstErr()");
		else if (res == 3)
			m_log->WriteLog("Reset semaphore error in rstErr()");
		else if (res == 4)
			m_log->WriteLog("Wait timeout in rstErr()");
		else if (res == 5)
			m_log->WriteLog("Request NAKed");
		else if (res == 6)
			m_log->WriteLog("Request neither ACKed or NAKed");
		else if (res == 7)
			m_log->WriteLog("Reset semaphore 2 error in rstErr()");
		else if (res == 8)
			m_log->WriteLog("Wait INF timeout in rstErr");
		else if (res == 9)
			m_log->WriteLog("Non ABS and non INF result");
		else if (res == 10)
			m_log->WriteLog("Request ABSed");
	}
	else if (m_tas300->m_Status.eqpStatus == 'E') {
		char s[100];
		sprintf(s, "Tas300 in fatal error:%2X Need Service!",
			m_tas300->m_Status.ecode);
		m_log->WriteLog(s);
	}
}

void CLP::Setbrother(CLP *brother) {
	m_brother = brother;
}
void CLP::Setconfig(CConfig *pCfg) {
	m_pCfg = pCfg;
}
void CLP::Brkinit() {
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io init 0xc021\r\n", 16);
		m_host->m_pSerial->SendBlock("io init 0xc021\r\n", 16);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io init 0x2\r\n", 13);
	m_host->m_pSerial->SendBlock("io init 0x2\r\n", 13);
	//executing command
	int res = m_tas300->movOP("ORGSH");
	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");

	GetFxlAmhsStatus();
	if (m_tas300->m_Status.ecode != 0)
	{
		char s[100];
		sprintf(s, "io event 0x8000 0x%02X\r\n",
			m_tas300->m_Status.ecode);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	if (m_tas300->m_Status.eqpStatus != '0'
		|| m_tas300->m_Status.mode != '0'
		|| m_tas300->m_Status.inited != '1'
		|| m_tas300->m_Status.opStatus != '0')
	{
		char s[100];
		sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
			m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
			m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}

	//executing finished successfully
	if (res == 0) {
		m_log->WriteLog("To Host", "io event 0x1000 0x1\r\n", 21);
		m_host->m_pSerial->SendBlock("io event 0x1000 0x1\r\n", 21);
	}
	//executing finished failed
	else {
		m_log->WriteLog("To Host", "io event 0x1000 0xc01c\r\n", 24);
		m_host->m_pSerial->SendBlock("io event 0x1000 0xc01c\r\n", 24);
	}
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkinitx() {
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io initx 0xc021\r\n", 17);
		m_host->m_pSerial->SendBlock("io initx 0xc021\r\n", 17);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io initx 0x2\r\n", 14);
	m_host->m_pSerial->SendBlock("io initx 0x2\r\n", 14);
	//executing command
	int res = m_tas300->movOP("ABORG");
	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");

	GetFxlAmhsStatus();
	if (m_tas300->m_Status.ecode != 0)
	{
		char s[100];
		sprintf(s, "io event 0x8000 0x%02X\r\n",
			m_tas300->m_Status.ecode);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	if (m_tas300->m_Status.eqpStatus != '0'
		|| m_tas300->m_Status.mode != '0'
		|| m_tas300->m_Status.inited != '1'
		|| m_tas300->m_Status.opStatus != '0')
	{
		char s[100];
		sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
			m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
			m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}

	//executing finished successfully
	if (res == 0) {
		m_log->WriteLog("To Host", "io event 0x1001 0x1\r\n", 21);
		m_host->m_pSerial->SendBlock("io event 0x1001 0x1\r\n", 21);
	}
	//executing finished failed
	else {
		m_log->WriteLog("To Host", "io event 0x1001 0xc01c\r\n", 24);
		m_host->m_pSerial->SendBlock("io event 0x1001 0xc01c\r\n", 24);
	}
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkload(int ldtype) {
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io load 0xc021\r\n", 16);
		m_host->m_pSerial->SendBlock("io load  0xc021\r\n", 16);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io load 0x2\r\n", 13);
	m_host->m_pSerial->SendBlock("io load 0x2\r\n", 13);
	//executing command
	int res;
	if (ldtype == 0)
		res = m_tas300->movOP("CLOAD");
	else if (ldtype == 1)
		res = m_tas300->movOP("CLDYD");
	else
		res = m_tas300->movOP("PODCL");

	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");

	GetFxlAmhsStatus();
	//executing finished successfully
	if (res == 0) {
		m_log->WriteLog("To Host", "io event 0x1002 0x1\r\n", 21);
		m_host->m_pSerial->SendBlock("io event 0x1002 0x1\r\n", 21);
	}
	//executing finished failed
	else {
		char opcode[100];
		sprintf(opcode, "io event 0x8002 %d\r\n", res);
		m_log->WriteLog("To Host", opcode, strlen(opcode));
		m_host->m_pSerial->SendBlock(opcode, strlen(opcode));

		//TasErrorHandle();
		if (m_tas300->m_Status.ecode != 0)
		{
			char s[100];
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			char s[100];
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus == 'A')
			m_tas300->rstErr();
		if (m_tas300->m_Status.inited != '1')
		{
			m_log->WriteLog("To Host", "io event 0x1002 0xc01c\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1002 0xc01c\r\n", 24);
		}
		else
		{
			m_log->WriteLog("To Host", "io event 0x1002 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1002 0xc017\r\n", 24);
		}
	}
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkunload(int uldtype) {
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io unload 0xc021\r\n", 18);
		m_host->m_pSerial->SendBlock("io unload 0xc021\r\n", 18);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io unload 0x2\r\n", 15);
	m_host->m_pSerial->SendBlock("io unload 0x2\r\n", 15);
	//executing command
	int res;
	if (uldtype == 0) {
		if (m_tas300->m_fpStatus == FPS_CLAMPED)
			res = m_tas300->movOP("PODOP");//("CULOD");//use this to speed up
		else
			//res = m_tas300->movOP("CULOD");
			res = m_tas300->movOP("ABORG");//use this to address the two consective CULOD caused issue
	}
	else if (uldtype == 1)
		res = m_tas300->movOP("CULYD");
	else
		res = m_tas300->movOP("CULFC");

	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");

	GetFxlAmhsStatus();
	//executing finished successfully
	if (res == 0) {
		m_log->WriteLog("To Host", "io event 0x1003 0x1\r\n", 21);
		m_host->m_pSerial->SendBlock("io event 0x1003 0x1\r\n", 21);
	}
	//executing finished failed
	else {
		char opcode[100];
		sprintf(opcode, "io event 0x8002 %d\r\n", res);
		m_log->WriteLog("To Host", opcode, strlen(opcode));
		m_host->m_pSerial->SendBlock(opcode, strlen(opcode));

		//TasErrorHandle();
		if (m_tas300->m_Status.ecode != 0)
		{
			char s[100];
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			char s[100];
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus == 'A')
			m_tas300->rstErr();
		if (m_tas300->m_Status.inited != '1')
		{
			m_log->WriteLog("To Host", "io event 0x1003 0xc01c\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1003 0xc01c\r\n", 24);
		}
		else
		{
			m_log->WriteLog("To Host", "io event 0x1003 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1003 0xc017\r\n", 24);
		}
	}
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkevon(char *evtID) {
	if (strcmp(evtID, "0x2001") != 0) {
		m_log->WriteLog("To Host", "io evon 0x1\r\n", 13);
		m_host->m_pSerial->SendBlock("io evon 0x1\r\n", 13);
		return;
	}

	int res = m_tas300->evtON();
	if (res == 0)
		m_log->WriteLog("Request ACKed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in evtON()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in evtON()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in evtON()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");

	res = m_tas300->fpeON();
	if (res == 0)
		m_log->WriteLog("Request ACKed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in fpeON()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in fpeON()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in fpeON()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");

	m_log->WriteLog("To Host", "io evon 0x1\r\n", 13);
	m_host->m_pSerial->SendBlock("io evon 0x1\r\n", 13);
}

void CLP::Brkevoff(char *evtID) {
	m_log->WriteLog("To Host", "io evoff 0x1\r\n", 14);
	m_host->m_pSerial->SendBlock("io evoff 0x1\r\n", 14);
}

void CLP::Brkid() {
	//id0(code number)=2005; id1,id2(Reserved)=0; id3=61; id4=100
	//id3=61(0011'1101:Maper installed,E84(2)(OHT)installed, IDsys installed
	/////////////////////////////////////////////////////////////////
	//Version control History:
	//1. Initial New(2007)Version: Rev = 100, maintenance tool support 
	//m_log->WriteLog("To Host","io id 0x1 2007 0 0 61 100\r\n", 27);
	//m_host->m_pSerial->SendBlock("io id 0x1 2007 0 0 61 100\r\n", 27);
	//2. Version: Rev = 101, speeded up unclamp time in E84
	//3. Version: Rev = 102, fixed bug of TasPodEvt:
	//static int offonStatus = -1;//This is a very hidden bug
	//4. Version: Rev = 103, support Omron IFID and more error report
	//5. Version: Rev = 104, not sending FIN to Tas300
	//6. Version: Rev = 105, speed up unload when FPS_CLAMPED
	//7. Version: Rev = 106, support es always on mode:discard this version
	//8. Version: Rev = 107, added e84 input/output recording:discard this version
	//9. Version: Rev = 108, es always on mode final
	//10. Version: Rev = 109, CS0 CS1 seperation; signal detection when BUSY_ON: discard this version
	//11. Version: Rev = 110, use CS0 only; unload using ABORG instead CULOD
	//12. Version: Rev = 111, added CS1 check, must be 0 all the time.
	//13. Version: Rev = 112, added all signal check before make HO_AVBL on.
	//14. Version: Rev = 113, turn es off when LT violated.
	//15. Version: Rev = 114, added door open interlock,modified signal check, turn off E84 when FIN.
	//16. Version: Rev = 115, 8030 format change, sending before status change.
	//17. Version: Rev = 116, preserve E84 field output signals when error happens.
	//18. Version: Rev = 117, added check Valid Off to CMPT Off timing check and fp status check in e84.
	//19. Version: Rev = 118, added 8030 event after action taken.
	//20. Version: Rev = 119, adjusted foup debouce time in E84 from 100ms to 1s.
	//21. Version: Rev = 120, added (BUSY==1 and COMPT==1)overlap (200ms) tolerance for Samsung.
	//22. Version: Rev = 121, added NG and ERROR barcode error filtering.
	//23. Version: Rev = 122, changed MotorOff timeout from 10 to 3; open control of lampID 4, 5 and 6.
	//24. Version: Rev = 123, add diagnostic event 8002 for load port operation return code 
	//25. Version: Rev = 124, add any page(1~32) reading support for Omron RFID; previous version only page 1. 
	//26. Version: Rev = 125, add N2 purge support. 
	//27. Version: Rev = 126, add "date" command and support any page(1~32) writing for Omron RFID; previous version only page 1.
	//28. Version: Rev = 127, add "LTC Enable-InTransfer(1)" and "LTC Enable-Always(2)" in _LTCEnDis flag,for Samsung   /////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//29. Version: Rev = 128, add doing N2 nozzle down in e84, for Intel
	//30. Version: Rev = 129, add "shutdown -1" to perform shutdown function.(Original is "Reboot(shutdown -0)" command.)
	//
	//[For New SBC]
	//01.Current Version: Rev = 200,
	//02.Current Version: Rev = 201,Fix cannot find end character(0X0D) from Omron. Modify vtime = 8. Modfiy msgMaxRetries =10 and change type to public . and io mrt xx for change msgMaxRetries value.
	//03.Current Version: Rev = 202, suitable for both new(rs232,rs485) and old(rs232) of CTI Xtreme/104-Plus.
	//04.Current Version: Rev = 203, fix barcode read cannot read issue.
	//05.Current Version: Rev = 204, Fix E84 TP3/TP4 timeout.
	char s[100];
	sprintf(s, "io id 0x1 2023 10 20 %d 204\r\n", _SBCVersion);//For SBC_Version_Query_For_TDK_LP_Config
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::Brkstatfxl() {
	char st[100];
	int res = m_tas300->statfxl();
	if (res == 0) {
		m_log->WriteLog("Request ACKed", m_tas300->m_statfxl, strlen(m_tas300->m_statfxl));
		if (m_tas300->m_fpStatus == 0)
			m_log->WriteLog("FPS_NOFOUP");
		else if (m_tas300->m_fpStatus == 1)
			m_log->WriteLog("FPS_PLACED");
		else if (m_tas300->m_fpStatus == 2)
			m_log->WriteLog("FPS_CLAMPED");
		else if (m_tas300->m_fpStatus == 3)
			m_log->WriteLog("FPS_DOCKED");
		else if (m_tas300->m_fpStatus == 4)
			m_log->WriteLog("FPS_OPENED");
		else if (m_tas300->m_fpStatus == -1)
			m_log->WriteLog("FPS_UNKNOWN");
	}
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in Statfxl()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in Statfxl()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in Statfxl()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Incorrect Param Length");
	else if (res == 8)
		m_log->WriteLog("Incorrect Parameters");
	else if (res == 9)
		m_log->WriteLog("m_fpStatus and m_statfxl not updated");

	if (res != 0)
	{
		sprintf(st, "io statfxl 0xc017 %d\r\n", res);
		m_log->WriteLog("To Host", st, strlen(st));
		m_host->m_pSerial->SendBlock(st, strlen(st));
		return;
	}

	if (m_tas300->m_Status.ecode != 0)
	{
		char s[100];
		sprintf(s, "io event 0x8000 0x%02X\r\n",
			m_tas300->m_Status.ecode);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	if (m_tas300->m_Status.eqpStatus != '0'
		|| m_tas300->m_Status.mode != '0'
		|| m_tas300->m_Status.inited != '1'
		|| m_tas300->m_Status.opStatus != '0')
	{
		char s[100];
		sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
			m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
			m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}

	sprintf(st, "io statfxl 0x1 %s\r\n", m_tas300->m_statfxl);
	m_log->WriteLog("To Host", st, strlen(st));
	m_host->m_pSerial->SendBlock(st, strlen(st));
}

void CLP::Brkstatnzl() {
	char s[100];
	int res = m_tas300->statn2purge();
	if (res == 0)
	{
		sprintf(s, "io statnzl 0x1 0x%c%c\r\n",
			m_tas300->m_n2pStatus.gasPressure, m_tas300->m_n2pStatus.nozzlePos);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	else
	{
		sprintf(s, "io statnzl 0xc017 %d\r\n", res);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
}

void CLP::Brkstat_m() {
	m_host->m_pSerial->SendBlock("io stat_m 0x1 0x03\r\n", 20);
}

void CLP::Brkstat_pdo() {
	m_host->m_pSerial->SendBlock("io stat_pdo 0x1 0x00\r\n", 22);
}

void CLP::Brkstat_lp() {
	m_host->m_pSerial->SendBlock("io stat_lp 0x1 0x00\r\n", 21);
}

void CLP::Brklamp(int lampID, int lampACT) {
	//if(lampID==4 || lampID==5 || lampID==6){
	  //presense, placement, alarm controled by TDKlp
	//  m_log->WriteLog("To Host","io lamp 0x1\r\n", 13);
	//  m_host->m_pSerial->SendBlock("io lamp 0x1\r\n", 13);
	  //return;
	//}
	char s[50];
	sprintf(s, "lamp ID = %d, lamp ACT = %d", lampID, lampACT);
	m_log->WriteLog(s);
	int res = m_tas300->lampOP(lampID, lampACT);
	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");
	if (res == 0) {
		m_log->WriteLog("To Host", "io lamp 0x1\r\n", 13);
		m_host->m_pSerial->SendBlock("io lamp 0x1\r\n", 13);
	}
}

void CLP::Brkmap() {
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io map 0xc021\r\n", 15);
		m_host->m_pSerial->SendBlock("io map 0xc021\r\n", 15);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io map 0x2\r\n", 12);
	m_host->m_pSerial->SendBlock("io map 0x2\r\n", 12);
	//executing command
	int res = m_tas300->movOP("MAPDO");
	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");
	//executing finished failed
	if (res != 0) {
		//TasErrorHandle();
		if (m_tas300->m_Status.ecode != 0)
		{
			char s[100];
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			char s[100];
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus == 'A')
			m_tas300->rstErr();
		if (m_tas300->m_Status.inited != '1')
		{
			m_log->WriteLog("To Host", "io event 0x1323 0xc01c\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1323 0xc01c\r\n", 24);
		}
		else
		{
			m_log->WriteLog("To Host", "io event 0x1323 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1323 0xc017\r\n", 24);
		}
		goto ex;
	}
	//executing finished successfully, read map data here!
	char st[100];
	res = m_tas300->mapResult();
	if (res == 0) {
		m_log->WriteLog("Request ACKed",
			m_tas300->m_mapRes, strlen(m_tas300->m_mapRes));
	}
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error in mapResult()");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error in mapResult()");
	else if (res == 4)
		m_log->WriteLog("Wait timeout in mapResult()");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Incorrect Param Length");
	else if (res == 8)
		m_log->WriteLog("Incorrect Parameters");
	//executing finished failed
	if (res != 0) {
		m_log->WriteLog("To Host", "io event 0x1323 0xc017\r\n", 24);
		m_host->m_pSerial->SendBlock("io event 0x1323 0xc017\r\n", 24);
		goto ex;
	}
	strncpy(m_mapRes, m_tas300->m_mapRes, 26);
	m_mapRes[25] = '\0';
	for (int i = 0; i < 25; i++) {//change to SEMI format
		if (m_mapRes[i] == '0')//no wafer
			m_mapRes[i] = '1';
		else if (m_mapRes[i] == '1')//wafer normal
			m_mapRes[i] = '3';
		else if (m_mapRes[i] == '2')//crossed
			m_mapRes[i] = '5';
		else if (m_mapRes[i] == 'W')//double slotted
			m_mapRes[i] = '4';
		else//undefined
			m_mapRes[i] = '0';
	}
	sprintf(st, "io event 0x1323 0x1 '%s'\r\n", m_mapRes);
	m_log->WriteLog("To Host", st, strlen(st));
	m_host->m_pSerial->SendBlock(st, strlen(st));
ex:  m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkrmap() {
	char st[100];
	sprintf(st, "io rmap 0x1 '%s'\r\n", m_mapRes);
	m_log->WriteLog("To Host", st, strlen(st));
	m_host->m_pSerial->SendBlock(st, strlen(st));
}

void CLP::Brkrdid(int page) {
	char s[500];
	int res;
	sprintf(s, "Page Number = %d", page);
	m_log->WriteLog(s);
	if (page < 1 || (page > 17 && page != 98 && page != 99)) {
		m_log->WriteLog("Unsupported page number");
		return;
	}
	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io rdid 0xc021\r\n", 16);
		m_host->m_pSerial->SendBlock("io rdid 0xc021\r\n", 16);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io rdid 0x2\r\n", 13);
	m_host->m_pSerial->SendBlock("io rdid 0x2\r\n", 13);
	//////////////////////////////////////////////////
	//m_hmos-> operation should be added here!!
	//////////////////////////////////////////////////
	//executing command
	if (m_bl600) {
		res = m_bl600->MotorON();
		if (res == 1)
			m_log->WriteLog("Error happened during sem reset");
		else if (res == 2)
			m_log->WriteLog("Wait for OK time out");
		//executing finished failed
		if (res != 0) {//MOTORON FAIL result
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);
			sprintf(s, "io event 0x8021 0x01\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
			goto ex;
		}
		//sleep(6);
		/*
		res = m_bl600->Unlock();
		if(res == 1)
		  m_log->WriteLog("Error happened during sem reset");
		else if(res == 2)
		  m_log->WriteLog("Wait for OK time out");
		*/
		int count;
		char bcode[50];
		char bcodepad[20];
		int len;
		count = 0;
		len = 0;
		m_readBcodeFinish = false;
		do {
			sleep(2);//2 seconds is enough for BL601, but BL601HAC1 need 12 seconds
			res = m_bl600->ReadBarCode(bcode, 50, &len);
			if (res == 1)
				m_log->WriteLog("Error happened during sem reset");
			else if (res == 2)
				m_log->WriteLog("Wait for result time out");
			else if (res == 3)
				m_log->WriteLog("Input array too short");
			count++;
		} while (count < 8 && res == 0 && m_readBcodeFinish == false);
		//executing finished
		if (res != 0 || m_readBcodeFinish == false) {//TIMEOUT result
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);

			if (m_readBcodeFinish == false)
			{
				sprintf(s, "io event 0x8021 0x05\r\n");
			}
			else
			{
				sprintf(s, "io event 0x8021 0x02\r\n");
			}
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
			goto ex;
		}
		bcode[len] = '\0';
		int i;
		for (i = 0; i < 16 - len; i++)
			bcodepad[i] = 0x1b;//ESC
		bcodepad[i] = '\0';
		if (len == 2 && bcode[0] == 'N' && bcode[1] == 'G') {//NG result
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);

			sprintf(s, "io event 0x8021 0x03\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else if (len == 5 && bcode[0] == 'E' && bcode[1] == 'R' && bcode[2] == 'R' && bcode[3] == 'O' && bcode[4] == 'R') {//ERROR result
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);

			sprintf(s, "io event 0x8021 0x04\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else {//SUCCESSFUL result
			sprintf(s, "io event 0x1092 0x1 %d '%s%s'\r\n", page, bcode, bcodepad);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}

	ex: res = m_bl600->MotorOFF();
		if (res == 1)
			m_log->WriteLog("Error happened during sem reset");
		else if (res == 2)
			m_log->WriteLog("Wait for OK time out");
	}
	else if (m_hmos) {
		char content[510];
		int len;

		if (page != 98 && page != 99) {
			res = m_hmos->ReadRFID(page, content, 300, &len);
			if (res == 1)
				m_log->WriteLog("Illegal single page number");
			else if (res == 2)
				m_log->WriteLog("Busy");
			else if (res == 3)
				m_log->WriteLog("prepCmd error");
			else if (res == 4)
				m_log->WriteLog("Semaphore reset error");
			else if (res == 5)
				m_log->WriteLog("Wait time out");
			else if (res == 6)
				m_log->WriteLog("Error result");
			else if (res == 7)
				m_log->WriteLog("Msg too long");
		}
		else {//if(page ==98 || page == 99)
			res = m_hmos->ReadMULTIPAGE(page, content, 500, &len);
			if (res == 1)
				m_log->WriteLog("Illegal multi-page number");
			else if (res == 2)
				m_log->WriteLog("Busy");
			else if (res == 3)
				m_log->WriteLog("prepCmd error");
			else if (res == 4 || res == 6)
				m_log->WriteLog("Semaphore reset error");
			else if (res == 5)
				m_log->WriteLog("Wait time out");
			else if (res == 6)
				m_log->WriteLog("Error result");
			else if (res == 7)
				m_log->WriteLog("Msg too long");
		}
		//executing finished failed
		if (res != 0) {
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);
			if (res == 6)//Reading from Hmos returned fail code
				sprintf(s, "io event 0x8024 0x0%s\r\n", m_hmos->m_failCode);
			else//usually time out
				sprintf(s, "io event 0x8024 0x10\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else {//executing finished successfully
			sprintf(s, "io event 0x1092 0x1 %s\r\n", content);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
	}
	else if (m_omron) {
		char content[510];
		int len;
		res = m_omron->ReadRFID(page, content, 300, &len);
		sprintf(s, "debug page:%d content:'%s'\r\n", page, content);
		m_log->WriteLog("To Host", s, strlen(s));
		//res = m_omron->LoopBackTS(content, 300, &len);
		if (res == 1)
			m_log->WriteLog("Illegal single page number");
		else if (res == 2)
			m_log->WriteLog("Busy");
		else if (res == 3)
			m_log->WriteLog("prepCmd error");
		else if (res == 4)
			m_log->WriteLog("Semaphore reset error");
		else if (res == 5)
			m_log->WriteLog("Wait time out");
		else if (res == 6)
			m_log->WriteLog("Error result");
		else if (res == 7)
			m_log->WriteLog("Msg too long");
		//executing finished failed
		if (res != 0) {
			m_log->WriteLog("To Host", "io event 0x1092 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1092 0xc017\r\n", 24);
			if (res == 6) {//Error result
				sprintf(s, "io event 0x8027 0x%s\r\n", m_omron->m_CompleteCode);
			}
			else//usually time out
				sprintf(s, "io event 0x8027 0xF0\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else {//executing finished successfully
			sprintf(s, "io event 0x1092 0x1 %d '%s'\r\n", page, content);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
	}

	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkwrid(int page, char* lotID, int lotIDlen) {
	char s[100];
	sprintf(s, "Page Number = %d", page);
	m_log->WriteLog(s);
	//if(page<2 || page>17){
	if (page < 1 || page>17) {
		m_log->WriteLog("Unsupported page number");
		return;
	}
	m_log->WriteLog("LotID", lotID, lotIDlen);
	if (lotIDlen < 16) {
		m_log->WriteLog("Lot ID must be 16 digits");
		return;
	}

	if (m_prgState != prg_READY) {
		m_log->WriteLog("To Host", "io wrid 0xc021\r\n", 16);
		m_host->m_pSerial->SendBlock("io wrid 0xc021\r\n", 16);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}

	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_log->WriteLog("To Host", "io wrid 0x2\r\n", 13);
	m_host->m_pSerial->SendBlock("io wrid 0x2\r\n", 13);
	//executing command
	int res;
	if (m_bl600) {
		sprintf(s, "io event 0x1093 0x1\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	else if (m_hmos) {
		if (lotIDlen == 16)
			for (int i = 0; i < 16; i++) s[i] = lotID[i];
		else
			for (int i = 0; i < 16; i++) s[i] = lotID[i + 1];
		res = m_hmos->WriteRFID(page, s, 16);
		if (res == 1)
			m_log->WriteLog("Illegal single page number");
		else if (res == 2)
			m_log->WriteLog("Busy");
		else if (res == 3)
			m_log->WriteLog("Invalid length");
		else if (res == 4)
			m_log->WriteLog("prepCmd error");
		else if (res == 5)
			m_log->WriteLog("Semaphore reset error");
		else if (res == 6)
			m_log->WriteLog("Wait time out");
		else if (res == 7)
			m_log->WriteLog("Error result");
		//executing finished successfully
		if (res == 0) {
			sprintf(s, "io event 0x1093 0x1\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else {//executing finished failed
			m_log->WriteLog("To Host", "io event 0x1093 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1093 0xc017\r\n", 24);
			if (res == 7)//Writing Hmos returned fail code
				sprintf(s, "io event 0x8024 0x0%s\r\n", m_hmos->m_failCode);
			else//usually time out
				sprintf(s, "io event 0x8024 0x10\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
	}
	else if (m_omron) {
		if (lotIDlen == 16)
			for (int i = 0; i < 16; i++) s[i] = lotID[i];
		else
			for (int i = 0; i < 16; i++) s[i] = lotID[i + 1];
		res = m_omron->WriteRFID(page, s, 16);
		if (res == 1)
			m_log->WriteLog("Illegal single page number");
		else if (res == 2)
			m_log->WriteLog("Busy");
		else if (res == 3)
			m_log->WriteLog("Invalid length");
		else if (res == 4)
			m_log->WriteLog("prepCmd error");
		else if (res == 5)
			m_log->WriteLog("Semaphore reset error");
		else if (res == 6)
			m_log->WriteLog("Wait time out");
		else if (res == 7)
			m_log->WriteLog("Error result");
		//executing finished successfully
		if (res == 0) {
			sprintf(s, "io event 0x1093 0x1\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else {//executing finished failed
			m_log->WriteLog("To Host", "io event 0x1093 0xc017\r\n", 24);
			m_host->m_pSerial->SendBlock("io event 0x1093 0xc017\r\n", 24);
			if (res == 7) {//Error result
				sprintf(s, "io event 0x8027 0x%s\r\n", m_omron->m_CompleteCode);
			}
			else//usually time out
				sprintf(s, "io event 0x8027 0xF0\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
	}

	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkresid() {
	char s[100];
	if (m_prgState != prg_READY) {
		m_host->m_pSerial->SendBlock("io resid 0xc021\r\n", 17);
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}

	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	m_host->m_pSerial->SendBlock("io resid 0x2\r\n", 14);
	//executing command
	sleep(2);
	//executing finished successfully
	sprintf(s, "io event 0x10BC 0x1\r\n");
	m_host->m_pSerial->SendBlock(s, strlen(s));
	//executing finished failed
	//m_host->m_pSerial->SendBlock("io event 0x10BC 0xc017\r\n", 24);
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brke84t(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, int td1) {
	char s[100];
	sprintf(s, "E84 tp1=%d, tp2=%d, tp3=%d, tp4=%d, tp5=%d, tp6=%d, td1=%d,",
		tp1, tp2, tp3, tp4, tp5, tp6, td1);
	m_log->WriteLog(s);
	m_e84->m_tp1 = tp1;
	m_e84->m_tp2 = tp2;
	m_e84->m_tp3 = tp3;
	m_e84->m_tp4 = tp4;
	m_e84->m_tp5 = tp5;
	m_e84->m_tp6 = tp6;
	m_e84->m_td1 = td1;
	m_e84->m_constChanged = true;
	m_log->WriteLog("To Host", "io e84t 0x1\r\n", 13);
	m_host->m_pSerial->SendBlock("io e84t 0x1\r\n", 13);
}

void CLP::Brksmcr() {
	//save to flash
	m_log->WriteLog("To Host", "io smcr 0x1\r\n", 13);
	m_host->m_pSerial->SendBlock("io smcr 0x1\r\n", 13);
}

void CLP::Brkenltc(int onoff) {
	char s[100];
	int lpcfchged;
	sprintf(s, "LTC onoff=%d", onoff);
	m_log->WriteLog(s);

	if (_LTCEnDis == 2)//[20131226]LeoModify_Avoid Loadport.exe send command to overwrite new value(2) during HOSTGUI initializing.
	{
		sprintf(s, "_LTCEnDis set to 2(LTC Detect Always) , Deny the command sent from HOST.");
		m_log->WriteLog(s);
	}
	else
	{
		_LTCEnDis = onoff;
		m_pCfg->cfgfileStr[7] = '0' + onoff;
		if (m_pCfg->cfgfileStr[1] != m_pCfg->orgcfgfStr[1] ||
			m_pCfg->cfgfileStr[3] != m_pCfg->orgcfgfStr[3])
			lpcfchged = 1;
		else
			lpcfchged = 0;
		m_pCfg->wrCfgnSet(lpcfchged, m_pCfg->mainmenuStr, m_pCfg->cfgfileStr);
	}

	m_log->WriteLog("To Host", "io enltc 0x1\r\n", 14);
	m_host->m_pSerial->SendBlock("io enltc 0x1\r\n", 14);

}

void CLP::Brkene84nz(int onoff)
{
	//Enable/Disable N2Purge Nozzle Up/Down In E84
	char s[100];
	sprintf(s, "N2Purge Nozzle Up/Dn in E84 onoff=%d", onoff);
	m_log->WriteLog(s);

	if (_N2PurgeNozzleDown_InE84 != onoff)
	{
		_N2PurgeNozzleDown_InE84 = onoff;
		int ret = m_pCfg->wrSingleCfgFile(1, _N2PurgeNozzleDown_InE84);
		if (ret != 0)
		{
			sprintf(s, "io ene84nz 0xc017\r\n");
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
			return;
		}
	}

	sprintf(s, "io ene84nz 0x1\r\n");
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::Brkgetconf() {
	char s[100];
	//char rd1, rd2, en, onlvl;
	char rd1, rd2;
	if (_LP1IdReader == 1)
		rd1 = 'b';
	else if (_LP1IdReader == 2)
		rd1 = 'h';
	else if (_LP1IdReader == 3)
		rd1 = 'o';
	else if (_LP1IdReader == 4)
		rd1 = 'm';

	if (_LP2IdReader == 1)
		rd2 = 'b';
	else if (_LP2IdReader == 2)
		rd2 = 'h';
	else if (_LP2IdReader == 3)
		rd2 = 'o';
	else if (_LP2IdReader == 4)
		rd2 = 'm';
	sprintf(s, "io getconf 0x1 '1%c2%c lten=%c onlv=%c'\r\n",
		rd1, rd2, '0' + _LTCEnDis, '0' + _LTCOnLevel);

	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::Brksetconf(int p1, int p2, int p3, int p4) {
	//_LP1IdReader = p1;//1: use barcode; 2: use Hermos RFID
	//_LP2IdReader = p2;//1: use barcode; 2: use Hermos RFID
	//_LTCEnDis    = p3;//0: Disable; 1: Enable-InTransfer;  2:Enable-Always
	//_LTCOnLevel  = p4;//0: 0V for ON; 1: 24V for ON
	char s[100];
	int lpcfchged;
	char cidreader;
	if (p1 == 1)
		cidreader = 'b';
	else if (p1 == 2)
		cidreader = 'h';
	else if (p1 == 3)
		cidreader = 'o';
	else if (p1 == 4)
		cidreader = 'm';
	else
		cidreader = 'h';
	m_pCfg->cfgfileStr[1] = cidreader;//"1h2h:En0Lv1"

	if (p2 == 1)
		cidreader = 'b';
	else if (p2 == 2)
		cidreader = 'h';
	else if (p2 == 3)
		cidreader = 'o';
	else if (p2 == 4)
		cidreader = 'm';
	else
		cidreader = 'h';
	m_pCfg->cfgfileStr[3] = cidreader;//"1h2h:En0Lv1"

	m_pCfg->cfgfileStr[7] = '0' + p3;//"1h2h:En0Lv1"
	m_pCfg->cfgfileStr[10] = '0' + p4;//"1h2h:En0Lv1"

	if (m_pCfg->cfgfileStr[1] != m_pCfg->orgcfgfStr[1] ||
		m_pCfg->cfgfileStr[3] != m_pCfg->orgcfgfStr[3])
		lpcfchged = 1;
	else
		lpcfchged = 0;
	m_pCfg->wrCfgnSet(lpcfchged, m_pCfg->mainmenuStr, m_pCfg->cfgfileStr);

	sprintf(s, "io setconf 0x1\r\n");
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::Brkshutdown(int cmd) {
	char s[100];
	sprintf(s, "io shutdown %d 0x1\r\n", cmd);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
	if (cmd == 1)
		system("shutdown -h now");
	else
		system("shutdown -r now");
}
void CLP::Brkupdate(char *lengthStr, int len) {
	if (len > 20)
		return;
	char s[100];
	int i;
	for (i = 0; i < len; i++)
		s[i] = lengthStr[i];
	lengthStr[i] = '\0';

	m_updatefilelen = atol(lengthStr);
	m_receivedlen = 0;
	m_bacceptingfile = true;//from now on, receiving source file lp.cc
	update_timeout = 600;//a total of 600 seconds or 10 minutes

	sprintf(s, "io update 0x2\r\n");
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::Brkassemblefile(char *received, int len) {
	static int called_count = 0;
	static FILE *stream = NULL;
	m_receivedlen += len;
	called_count++;

	if (called_count == 1)
	{
		char curTime[128];
		struct tm *tm_ptr;
		time_t the_time;
		time(&the_time);
		tm_ptr = localtime(&the_time);
		strftime(curTime, 128, "%m_%d_%Y-%H_%M_%S", tm_ptr);

		char backupfile[128];
		sprintf(backupfile, "/usr/tdk/lp/lp-%s-bk.cc", curTime);
		char mvcommand[128];
		strcpy(mvcommand, "mv /usr/tdk/lp/lp.cc ");
		strcat(mvcommand, backupfile);
		system(mvcommand);

		if ((stream = fopen("/usr/tdk/lp/lp.cc", "wb")) == NULL)
			return;
	}

	fwrite(received, sizeof(char), len, stream);

	if (m_receivedlen >= m_updatefilelen || update_timeout == 0)
	{
		m_updatefilelen = 0;
		m_receivedlen = 0;
		m_bacceptingfile = false;
		update_timeout = 0;
		called_count = 0;
		if (stream)
		{
			fflush(stream);
			fclose(stream);
			stream = NULL;
		}
		char s[1000];
		if (m_receivedlen == m_updatefilelen)
		{
			system("make -C /usr/tdk/lp 2>/usr/tdk/lp/cmpile_err.txt");
			sleep(8);
			if ((stream = fopen("/usr/tdk/lp/cmpile_err.txt", "rb")) == NULL)
			{
				sprintf(s, "io event 0x8010 0xff 'compile_err file not created'\r\n");
			}
			else
			{
				char subs[900];
				size_t bytesRD = fread(subs, sizeof(char), 800, stream);
				subs[bytesRD] = '\0';
				strcat(subs, "\r\n");

				if (bytesRD == 0)
					sprintf(s, "io event 0x8010 0x1 'compile succeed'\r\n");
				else
				{
					sprintf(s, "io event 0x8010 0xff '%s'\r\n", subs);
				}
			}

			if (stream)
			{
				fclose(stream);
				stream = NULL;
			}
		}
		else
			sprintf(s, "io event 0x8010 0xff 'wrong length'\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
}
void CLP::Brkene84(int onoff, int addr) {
	/*
	if(onoff == 1)
	  m_e84->EnableAMHS();
	else
	  m_e84->DisableAMHS();
	m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
	m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
	return;
	*/
	char s[100];
	sprintf(s, "E84 onoff=%d, addr=%d: %s %s", onoff, addr,
		onoff == 1 ? "Enable" : "Disable", addr == 1 ? "OHT" : "AGV");
	m_log->WriteLog(s);
	if (addr == 0) {//illegal address: AGV not supported
	  //the following 2 line will cause upersoftware repeatedly send same thing
	  //m_log->WriteLog("To Host","io ene84 0xc015\r\n", 17);
	  //m_host->m_pSerial->SendBlock("io ene84 0xc015\r\n", 17);
		m_log->WriteLog("AGV Not Supported, Do nothing");
		m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
		m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
		return;
	}
	if (onoff == 1) {
		if (m_fxlamhsState == fxl_AMHS) {
			m_log->WriteLog("E84 Already Enabled, Do nothing");
			m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
			m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
			return;
		}
	Lget:
		int res = GetFxlAmhsStatus();
		if (res != 0) {//command can not be executed
			m_log->WriteLog("To Host", "io ene84 0xc017\r\n", 17);
			m_host->m_pSerial->SendBlock("io ene84 0xc017\r\n", 17);
			return;
		}
		if (m_tas300->m_Status.ecode != 0)
		{
			char s[100];
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			char s[100];
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_fxlamhsState != fxl_READY) {
			//command will be rejected if m_fpStatus > FPS_CLAMPED;
			//if m_fpStatus == FPS_PLACED: Unload Scenario A
			//if m_fpStatus == FPS_CLAMPED, unclamp first: Unload Scenario B
			if (m_tas300->m_fpStatus > FPS_CLAMPED) {
				m_log->WriteLog("To Host", "io ene84 0xc021\r\n", 17);
				m_host->m_pSerial->SendBlock("io ene84 0xc021\r\n", 17);
				return;
			}
#ifndef _UnclampInE84
			res = m_tas300->movOP("CULOD");
			if (res == 0)
				m_log->WriteLog("Request INFed");
			else if (res == 1)
				m_log->WriteLog("Busy!");
			else if (res == 2)
				m_log->WriteLog("Internal error");
			else if (res == 3)
				m_log->WriteLog("Reset semaphore error");
			else if (res == 4)
				m_log->WriteLog("Wait timeout");
			else if (res == 5)
				m_log->WriteLog("Request NAKed");
			else if (res == 6)
				m_log->WriteLog("Request neither ACKed or NAKed");
			else if (res == 7)
				m_log->WriteLog("Reset semaphore 2 error");
			else if (res == 8)
				m_log->WriteLog("Wait INF timeout");
			else if (res == 9)
				m_log->WriteLog("Non ABS and non INF result");
			else if (res == 10)
				m_log->WriteLog("Request ABSed");

			if (res != 0) {//command can not be executed
				m_log->WriteLog("To Host", "io ene84 0xc017\r\n", 17);
				m_host->m_pSerial->SendBlock("io ene84 0xc017\r\n", 17);
				return;
			}
			goto Lget;
#endif
		}
		res = m_e84->EnableAMHS();
		if (res != 0) {//command can not be executed
			m_log->WriteLog("To Host", "io ene84 0xc017\r\n", 17);
			m_host->m_pSerial->SendBlock("io ene84 0xc017\r\n", 17);
			return;
		}
		m_log->WriteLog("FXL_AMHS state => fxl_AMHS");
		m_fxlamhsState = fxl_AMHS;//finally state changed to fxl_AMHS
		m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
		m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
	}
	else {//if(onoff == 0)
	  /*
	  if(m_fxlamhsState != fxl_AMHS){
		m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
		m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
		return;
	  }
	  */
		sleep(1);
		m_e84->DisableAMHS();
		m_fxlamhsState = fxl_READY;//state changed to fxl_READY
		GetFxlAmhsStatus();//Don't care for the return result

		if (m_tas300->m_Status.ecode != 0)
		{
			char s[100];
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			char s[100];
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}

		m_log->WriteLog("To Host", "io ene84 0x1\r\n", 14);
		m_host->m_pSerial->SendBlock("io ene84 0x1\r\n", 14);
	}
}

void CLP::Brkrde84(int addr) {
	int VALID, CS_0, CS_1, LTCIN, TR_REQ, BUSY, COMPT, CONT;
	char s[100];
	sprintf(s, "Read E84 from addr=%d (%s)", addr, addr == 0 ? "AGV" : "OHT");
	m_log->WriteLog(s);
	if (addr == 0) {//illegal address: AGV not supported
		m_log->WriteLog("To Host", "io rde84 0xc015\r\n", 17);
		m_host->m_pSerial->SendBlock("io rde84 0xc015\r\n", 17);
		return;
	}
	int res = m_e84->R_INPUT(&VALID, &CS_0, &CS_1, &LTCIN, &TR_REQ, &BUSY, &COMPT, &CONT);
	if (res != 0) {//should not happen
		m_log->WriteLog("To Host", "io rde84 0xc017\r\n", 17);
		m_host->m_pSerial->SendBlock("io rde84 0xc017\r\n", 17);
		return;
	}
	unsigned char inport = m_e84->m_Input;
	unsigned char outport = m_e84->m_Output;
	char prm1[5], prm2[5];
	prm1[0] = '0'; prm1[1] = 'x'; prm1[4] = '\0';
	prm2[0] = '0'; prm2[1] = 'x'; prm2[4] = '\0';
	char tmp = (inport >> 4) & 0x0F;
	if (tmp < 10)tmp += '0';
	else tmp = tmp - 10 + 'a';
	prm1[2] = tmp;
	tmp = inport & 0x0F;
	if (tmp < 10)tmp += '0';
	else tmp = tmp - 10 + 'a';
	prm1[3] = tmp;

	tmp = (outport >> 4) & 0x0F;
	if (tmp < 10)tmp += '0';
	else tmp = tmp - 10 + 'a';
	prm2[2] = tmp;
	tmp = outport & 0x0F;
	if (tmp < 10)tmp += '0';
	else tmp = tmp - 10 + 'a';
	prm2[3] = tmp;

	sprintf(s, "io rde84 0x1 %s %s\r\n", prm1, prm2);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::Brkesmode(int mode)
{
	char s[100];
	sprintf(s, "ES mode set to: %d(%s)", mode, mode == 0 ? "Normal Mode" : "Always ON Mode");
	m_log->WriteLog(s);

	if (mode >= 0 && mode <= 1) {
		_ESMode = mode;
		sprintf(s, "io esmode 0x1\r\n");
		if (_ESMode == 1)
		{
			int res = m_e84->W_ES(1);
			//res = m_e84->W_HO_AVBL(1);
			if (res != 0) {//command can not be executed
				sprintf(s, "io esmode 0xc017\r\n");
				return;
			}
		}
	}
	else
		sprintf(s, "io esmode 0xc013\r\n");

	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::Brkmchstatus(int status) {
	char s[100];
	sprintf(s, "Machine status set to: 0x%x", status);
	m_log->WriteLog(s);

	if (status >= 0 && status <= 255) {
		m_e84->m_mchStatus = status;
		sprintf(s, "io mch 0x1\r\n");
	}
	else
		sprintf(s, "io mch 0xc013\r\n");

	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::Brkho_avbl(int ho_avbl, int addr) {
	char s[100];
	sprintf(s, "Set E84 ho_avbl=%d, addr=%d: %s Handoff %s", ho_avbl, addr,
		addr == 1 ? "OHT" : "AGV", ho_avbl == 1 ? "available" : "not available");
	m_log->WriteLog(s);
	if (addr == 0) {//illegal address: AGV not supported
		m_log->WriteLog("To Host", "io ho_avbl 0xc015\r\n", 19);
		m_host->m_pSerial->SendBlock("io ho_avbl 0xc015\r\n", 19);
		return;
	}
	int res = m_e84->W_HO_AVBL(ho_avbl);
	if (res != 0) {//command can not be executed
		m_log->WriteLog("To Host", "io ho_avbl 0xc017\r\n", 19);
		m_host->m_pSerial->SendBlock("io ho_avbl 0xc017\r\n", 19);
		return;
	}
	m_log->WriteLog("To Host", "io ho_avbl 0x1\r\n", 16);
	m_host->m_pSerial->SendBlock("io ho_avbl 0x1\r\n", 16);
}

void CLP::Brkes(int es, int addr) {
	char s[100];
	sprintf(s, "Set E84 es=%d, addr=%d: %s Emergency stop %s", es, addr,
		addr == 1 ? "OHT" : "AGV", es == 1 ? "no" : "yes");
	m_log->WriteLog(s);
	if (addr == 0) {//illegal address: AGV not supported
		m_log->WriteLog("To Host", "io es 0xc015\r\n", 14);
		m_host->m_pSerial->SendBlock("io es 0xc015\r\n", 14);
		return;
	}
	int res = m_e84->W_ES(es);
	if (res != 0) {//command can not be executed
		m_log->WriteLog("To Host", "io es 0xc017\r\n", 14);
		m_host->m_pSerial->SendBlock("io es 0xc017\r\n", 14);
		return;
	}
	m_log->WriteLog("To Host", "io es 0x1\r\n", 11);
	m_host->m_pSerial->SendBlock("io es 0x1\r\n", 11);
}

void CLP::Brkout_e84(char *out_e84, int addr) {
	char s[100];
	sprintf(s, "Set E84 OutPort = %s, addr=%d (%s)", out_e84, addr,
		addr == 0 ? "AGV" : "OHT");
	m_log->WriteLog(s);
	if (addr == 0) {//illegal address: AGV not supported
		m_log->WriteLog("To Host", "io out_e84 0xc015\r\n", 19);
		m_host->m_pSerial->SendBlock("io out_e84 0xc015\r\n", 19);
		return;
	}
	if (m_fxlamhsState == fxl_AMHS) {//command can not be executed
		m_log->WriteLog("To Host", "io out_e84 0xc017\r\n", 19);
		m_host->m_pSerial->SendBlock("io out_e84 0xc017\r\n", 19);
		return;
	}
	bool wrongParam = false;
	unsigned char oe84;
	unsigned char tmp = out_e84[2];
	if ('0' <= tmp && tmp <= '9')
		tmp -= '0';
	else if ('A' <= tmp && tmp <= 'F')
		tmp = tmp - 'A' + 10;
	else if ('a' <= tmp && tmp <= 'f')
		tmp = tmp - 'a' + 10;
	else wrongParam = true;
	oe84 = (tmp << 4) & 0xF0;
	tmp = out_e84[3];
	if ('0' <= tmp && tmp <= '9')
		tmp -= '0';
	else if ('A' <= tmp && tmp <= 'F')
		tmp = tmp - 'A' + 10;
	else if ('a' <= tmp && tmp <= 'f')
		tmp = tmp - 'a' + 10;
	else wrongParam = true;
	oe84 += tmp & 0x0F;
	if (wrongParam) {//illegal parameter value
		m_log->WriteLog("To Host", "io out_e84 0xc013\r\n", 19);
		m_host->m_pSerial->SendBlock("io out_e84 0xc013\r\n", 19);
		return;
	}
	int res = m_e84->W_E84OUTPUT(SRC_HOST_out_e84_Brkout_e84, oe84);
	if (res != 0) {//command can not be executed
		m_log->WriteLog("To Host", "io out_e84 0xc017\r\n", 19);
		m_host->m_pSerial->SendBlock("io out_e84 0xc017\r\n", 19);
		return;
	}
	m_log->WriteLog("To Host", "io out_e84 0x1\r\n", 16);
	m_host->m_pSerial->SendBlock("io out_e84 0x1\r\n", 16);
}

void CLP::BrkPurge(int purgeType)//1-Activate; 2 or other - Deactivate
{
	char s[100];
	if (m_prgState != prg_READY) {
		if (purgeType == 1)
			sprintf(s, "io act_purge 0xc021\r\n");
		else
			sprintf(s, "io deact_purge 0xc021\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
		m_log->WriteLog("Not prg_READY: Command rejected");
		return;
	}
	m_prgState = prg_BUSY;
	m_log->WriteLog("PRG state => prg_BUSY");
	if (purgeType == 1)
		sprintf(s, "io act_purge 0x2\r\n");
	else
		sprintf(s, "io deact_purge 0x2\r\n");
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
	//executing command
	int res;
	if (purgeType == 1)
		res = m_tas300->movOP("BPNUP");
	else
		res = m_tas300->movOP("BPNDW");

	if (res == 0)
		m_log->WriteLog("Request INFed");
	else if (res == 1)
		m_log->WriteLog("Busy!");
	else if (res == 2)
		m_log->WriteLog("Internal error");
	else if (res == 3)
		m_log->WriteLog("Reset semaphore error");
	else if (res == 4)
		m_log->WriteLog("Wait timeout");
	else if (res == 5)
		m_log->WriteLog("Request NAKed");
	else if (res == 6)
		m_log->WriteLog("Request neither ACKed or NAKed");
	else if (res == 7)
		m_log->WriteLog("Reset semaphore 2 error");
	else if (res == 8)
		m_log->WriteLog("Wait INF timeout");
	else if (res == 9)
		m_log->WriteLog("Non ABS and non INF result");
	else if (res == 10)
		m_log->WriteLog("Request ABSed");

	GetFxlAmhsStatus();
	//executing finished successfully
	if (res == 0) {
		if (purgeType == 1)
			sprintf(s, "io event 0x1130 0x1\r\n");
		else
			sprintf(s, "io event 0x1131 0x1\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	//executing finished failed
	else {//TAS300 Operation return code.
		sprintf(s, "io event 0x8002 %d\r\n", res);
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));

		//TasErrorHandle();
		if (m_tas300->m_Status.ecode != 0)//standard: 0x28-bottom purge nozzle position error
		{								 //expanded: 0x28-bottom purge nozzle up error;0x68-nozzle down error 
			sprintf(s, "io event 0x8000 0x%02X\r\n",
				m_tas300->m_Status.ecode);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus != '0'
			|| m_tas300->m_Status.mode != '0'
			|| m_tas300->m_Status.inited != '1'
			|| m_tas300->m_Status.opStatus != '0')
		{
			sprintf(s, "io event 0x8001 0x%c%c%c%c\r\n",
				m_tas300->m_Status.eqpStatus, m_tas300->m_Status.mode,
				m_tas300->m_Status.inited, m_tas300->m_Status.opStatus);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		if (m_tas300->m_Status.eqpStatus == 'A')
			m_tas300->rstErr();

		res = m_tas300->statn2purge();
		if (res == 0)
		{
			sprintf(s, "io event 0x8003 0x%c%c\r\n",
				m_tas300->m_n2pStatus.gasPressure, m_tas300->m_n2pStatus.nozzlePos);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}
		else
		{
			sprintf(s, "io event 0x8003 %d\r\n", res);
			m_log->WriteLog("To Host", s, strlen(s));
			m_host->m_pSerial->SendBlock(s, strlen(s));
		}

		if (purgeType == 1)
			sprintf(s, "io event 0x1130 0xc901\r\n");//nozzle up timeout error; all 3 up sensors will not turn on in time
		else
			sprintf(s, "io event 0x1131 0xc900\r\n");//nozzle down timeout error; all 3 down sensors will not turn on in time

		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));

	}
	m_prgState = prg_READY;
	m_log->WriteLog("PRG state => prg_READY");
}

void CLP::Brkdate() {
	char m_curTime[128];
	struct tm *tm_ptr;
	time_t the_time;
	time(&the_time);
	tm_ptr = localtime(&the_time);
	strftime(m_curTime, 128, "%Y %m %d %H %M", tm_ptr); //YYYY MM DD HH mm  

	char s[100];
	sprintf(s, "io date %s\r\n", m_curTime);

	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::E84_8031_Event(int errorcode) {
	char s[100];
	sprintf(s, "io event 0x8031 %d\r\n", errorcode);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}

void CLP::E84_8030_Event(bool foup_errcond) {
	char s[100];
	unsigned char fpEvent;
	if (foup_errcond)
		fpEvent = 0x10 | m_tas300->m_fpEvent;
	else
		fpEvent = m_tas300->m_fpEvent;
	sprintf(s, "io event 0x8030 0x%02x 0x%02x 0x%02x\r\n", m_e84->m_Input, m_e84->m_Output, fpEvent);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::E84_st_chg(const char *evtcode, char *status) {
	//evtcode and status:null terminated string
	E84_8030_Event(false);//not foup error condition

	char s[100];
	sprintf(s, "io event 0x2022 %s %s\r\n", evtcode, status);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::E84_st_chg2(const char *evtcode, char *status) {
	//evtcode and status:null terminated string
	E84_8030_Event(true);//foup error condition

	char s[100];
	sprintf(s, "io event 0x2022 %s %s\r\n", evtcode, status);
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
void CLP::TasPodEvt(int off_2_on) {
	//0: not present(PODOF), 1: present only(SMTON) 
	//2: partial placement(ABNST), 3: normal placement(PODON)
	//static int offonStatus = -1;//This is a very hidden bug

	switch (off_2_on) {
	case 0://PODOF=>brooks removed to not present
		m_tas300->m_fpStatus = FPS_NOFOUP;
		m_tas300->m_fpEvent = FPEVT_PODOF;
		m_log->WriteLog("FPS_NOFOUP");
		//GetFxlAmhsStatus();//don't call
		if (offonStatus == -1 || offonStatus == 3) {
			m_log->WriteLog("To Host", "io event 0x2002 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x2002 0x0\r\n", 21);//remove action
			m_log->WriteLog("To Host", "io event 0x200e 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x200e 0x0\r\n", 21);//not present
		}
		else if (offonStatus > 0) {
			m_log->WriteLog("To Host", "io event 0x200e 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x200e 0x0\r\n", 21);//not present
		}
		offonStatus = 0;
		break;
	case 1://SMTON
		m_tas300->m_fpEvent = FPEVT_SMTON;
		if (offonStatus == -1) {
			if (m_tas300->m_fpStatus == FPS_NOFOUP) {
				m_log->WriteLog("To Host", "io event 0x200d 0x0\r\n", 21);
				m_host->m_pSerial->SendBlock("io event 0x200d 0x0\r\n", 21);//present        
			}
		}
		else if (offonStatus == 0) {
			m_log->WriteLog("To Host", "io event 0x200d 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x200d 0x0\r\n", 21);//present        
		}
		else if (offonStatus == 3) {
			m_log->WriteLog("To Host", "io event 0x2002 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x2002 0x0\r\n", 21);//remove action
		}
		offonStatus = 1;
		break;
	case 2://ABNST
		m_tas300->m_fpEvent = FPEVT_ABNST;
		if (offonStatus == -1) {
			if (m_tas300->m_fpStatus == FPS_PLACED) {
				m_log->WriteLog("To Host", "io event 0x2002 0x0\r\n", 21);
				m_host->m_pSerial->SendBlock("io event 0x2002 0x0\r\n", 21);//remove action
			}
		}
		else if (offonStatus == 3) {
			m_log->WriteLog("To Host", "io event 0x2002 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x2002 0x0\r\n", 21);//remove action
		}
		offonStatus = 2;
		break;
	case 3://PODON=>brooks present to placed
		m_tas300->m_fpStatus = FPS_PLACED;
		m_tas300->m_fpEvent = FPEVT_PODON;
		m_log->WriteLog("FPS_PLACED");
		//GetFxlAmhsStatus();//don't call
		if (offonStatus == -1 || offonStatus == 0) {
			m_log->WriteLog("To Host", "io event 0x200d 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x200d 0x0\r\n", 21);//present
			m_log->WriteLog("To Host", "io event 0x2001 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x2001 0x0\r\n", 21);//placed
		}
		else {
			m_log->WriteLog("To Host", "io event 0x2001 0x0\r\n", 21);
			m_host->m_pSerial->SendBlock("io event 0x2001 0x0\r\n", 21);//placed      
		}
		offonStatus = 3;
		break;
	}
}
void CLP::TasManSwEvt() {
	m_log->WriteLog("Manual Switch Pushed!");
	m_log->WriteLog("To Host", "io event 0x2027 0x0\r\n", 21);
	m_host->m_pSerial->SendBlock("io event 0x2027 0x0\r\n", 21);
}
void CLP::TasPGEvent(char *evtparm) {
	char s[100];
	int len = strlen(evtparm);
	if (len < 3)
		return;
	m_log->WriteLog("PG Event!");
	if (evtparm[0] == '1' && evtparm[1] == 'O' && evtparm[2] == 'N')//Primary N2 pressure senosr output ON
		sprintf(s, "io event 0x8004 0x01\r\n");
	else if (evtparm[0] == '1' && evtparm[1] == 'O' && evtparm[2] == 'F')//Primary N2 pressure senosr output OFF
		sprintf(s, "io event 0x8004 0x02\r\n");
	else
		sprintf(s, "io event 0x8004 0x03\r\n");
	m_log->WriteLog("To Host", s, strlen(s));
	m_host->m_pSerial->SendBlock(s, strlen(s));
}
//the_ustsname.release = Linux Version Ex:4.2.4 
void CLP::CheckHWstatus()
{
	struct utsname the_ustsname;
	if (uname(&the_ustsname)) exit(-1);
	char MaxRetryTimesMsg[100];
	char LinuxMajorVer[1];
	LinuxMajorVer[0] = the_ustsname.release[0];
	if (atoi(LinuxMajorVer) >= 4)
	{
		char s[100];
		_SBCVersion = 2;
		sprintf(s, "io sbc_new\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));

	}
	else if (atoi(LinuxMajorVer) < 4)
	{
		char s[100];
		_SBCVersion = 1;
		sprintf(s, "io sbc_original\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}
	else
	{
		char s[100];
		_SBCVersion = 1;
		sprintf(s, "io cannot recognize sbc version\r\n");
		m_log->WriteLog("To Host", s, strlen(s));
		m_host->m_pSerial->SendBlock(s, strlen(s));
	}

	sprintf(MaxRetryTimesMsg, "max retry times:%d\r\n", msgMaxRetries);
	m_host->m_pSerial->SendBlock(MaxRetryTimesMsg, strlen(MaxRetryTimesMsg));

}

void CLP::SendToHostMaxReceiveTimes()
{
	char MaxRetryTimesMsg[35];

	sprintf(MaxRetryTimesMsg, "Parsing Msg Max Retry Times:%d\r\n", msgMaxRetries);
	m_host->m_pSerial->SendBlock(MaxRetryTimesMsg, strlen(MaxRetryTimesMsg));

}
//End Load Port Class Implementation
//////////////////////////////////

//////////////////////////////////
//CHostBK Class Implementation
CHostBK::CHostBK(int lpID, void *parent) {
	m_lpID = lpID;
	m_parent = parent;
	((CLP*)m_parent)->m_log->WriteLog("CHostBK Constructor called");
	m_pSerial = new CSerial(m_lpID, "Host"/*log file name base*/);
	m_pSerial->Initialize(HOSTBK, this, CallbackWrap);
}
CHostBK::~CHostBK() {
	((CLP*)m_parent)->m_log->WriteLog("CHostBK Destructor called");
	if (m_pSerial != NULL)
		delete m_pSerial;
}

int CHostBK::OpenPort() {
	return m_pSerial->OpenConnection();
}
int CHostBK::ClosePort() {
	return m_pSerial->CloseConnection();
}

void CHostBK::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	CHostBK *pSelf = (CHostBK*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

void CHostBK::Callback(char *pMsg, int len) {
	char *pM = new char[len];
	for (int l = 0; l < len; l++) {
		pM[l] = pMsg[l];
	}
	char item[100];
	int itemlen;
	int msgstPos = 0;
	int res;
	char *ptri;
	if (((CLP*)m_parent)->m_bacceptingfile)
	{
		((CLP*)m_parent)->Brkassemblefile(pM, len);
		goto ex;
	}
	//Do processing of message bellow 
	((CLP*)m_parent)->m_log->WriteLog("Fr Host", pM, len);
	//Read address
	res = ReadItemIO(item, 100, &itemlen, pM, len, &msgstPos);
	res = CmpStr("io", 2, item, itemlen);
	if (res != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong address");
		goto ex;
	}
	//Read command
	res = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
	//////////////////////////////////////////////////////////////////
	//command init
	res = CmpStr("init", 4, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkinit();
		goto ex;
	}
	//end command init
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command initx 2
	res = CmpStr("initx", 5, item, itemlen);
	if (res == 0) {
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No parameter 2");
			goto ex;
		}
		lres = CmpStr("2", 1, item, itemlen);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameter");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkinitx();
		goto ex;
	}
	//end command initx 2
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command load
	res = CmpStr("load", 4, item, itemlen);
	if (res == 0) {
		int ldtype = 0;//0:load;   1:load 1;    2:load 2
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		if (lres != 0)//no parameters after load
			ldtype = 0;
		else {//detected one parameter
			lres = CmpStr("1", 1, item, itemlen);
			lres2 = CmpStr("2", 1, item, itemlen);
			if (lres != 0 && lres2 != 0) {
				((CLP*)m_parent)->m_log->WriteLog("Wrong parameter");
				goto ex;
			}
			if (lres == 0) ldtype = 1;
			else ldtype = 2;
		}
		char s[50];
		sprintf(s, "Load type = %d", ldtype);
		((CLP*)m_parent)->m_log->WriteLog(s);

		//processing according to ldtype
		((CLP*)m_parent)->Brkload(ldtype);
		goto ex;
	}
	//end command load
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command unload
	res = CmpStr("unload", 6, item, itemlen);
	if (res == 0) {
		int uldtype = 0;//0:unload;   1:unload 1;    2:unload 2
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		if (lres != 0)//no parameters after unload
			uldtype = 0;
		else {//detected one parameter
			lres = CmpStr("1", 1, item, itemlen);
			lres2 = CmpStr("2", 1, item, itemlen);
			if (lres != 0 && lres2 != 0) {
				((CLP*)m_parent)->m_log->WriteLog("Wrong parameter");
				goto ex;
			}
			if (lres == 0) uldtype = 1;
			else uldtype = 2;
		}
		char s[50];
		sprintf(s, "Unload type = %d", uldtype);
		((CLP*)m_parent)->m_log->WriteLog(s);

		//processing according to uldtype
		((CLP*)m_parent)->Brkunload(uldtype);
		goto ex;
	}
	//end command unload
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command evon
	res = CmpStr("evon", 4, item, itemlen);
	if (res == 0) {
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after evon
			((CLP*)m_parent)->m_log->WriteLog("No event ID to turn on");
			goto ex;
		}
		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		int r1 = CmpStr("0x2001", 6, item, itemlen);
		int r2 = CmpStr("0x2002", 6, item, itemlen);
		int rd = CmpStr("0x200d", 6, item, itemlen);
		int re = CmpStr("0x200e", 6, item, itemlen);
		int r22 = CmpStr("0x2022", 6, item, itemlen);
		int r27 = CmpStr("0x2027", 6, item, itemlen);
		int r29 = CmpStr("0x2029", 6, item, itemlen);
		int r1092 = CmpStr("0x1092", 6, item, itemlen);//for escan300
		if (r1 != 0 && r2 != 0 && rd != 0 && re != 0 &&
			r22 != 0 && r27 != 0 && r29 != 0 && r1092 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Unknow event ID");
			goto ex;
		}

		char s[50];
		if (r1 == 0)
			sprintf(s, "0x2001");
		else if (r2 == 0)
			sprintf(s, "0x2002");
		else if (rd == 0)
			sprintf(s, "0x200d");
		else if (re == 0)
			sprintf(s, "0x200e");
		else if (r22 == 0)
			sprintf(s, "0x2022");
		else if (r27 == 0)
			sprintf(s, "0x2027");
		else if (r29 == 0)
			sprintf(s, "0x2029");
		else if (r1092 == 0)
			sprintf(s, "0x1092");
		else
			sprintf(s, "");

		//processing
		((CLP*)m_parent)->Brkevon(s);
		goto ex;
	}
	//end command evon
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command evoff
	res = CmpStr("evoff", 5, item, itemlen);
	if (res == 0) {
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after evon
			((CLP*)m_parent)->m_log->WriteLog("No event ID to turn off");
			goto ex;
		}
		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		int r1 = CmpStr("0x2001", 6, item, itemlen);
		int r2 = CmpStr("0x2002", 6, item, itemlen);
		int rd = CmpStr("0x200d", 6, item, itemlen);
		int re = CmpStr("0x200e", 6, item, itemlen);
		int r22 = CmpStr("0x2022", 6, item, itemlen);
		int r27 = CmpStr("0x2027", 6, item, itemlen);
		int r29 = CmpStr("0x2029", 6, item, itemlen);
		if (r1 != 0 && r2 != 0 && rd != 0 && re != 0 &&
			r22 != 0 && r27 != 0 && r29 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Unknow event ID");
			goto ex;
		}

		char s[50];
		if (r1 == 0)
			sprintf(s, "0x2001");
		else if (r2 == 0)
			sprintf(s, "0x2002");
		else if (rd == 0)
			sprintf(s, "0x200d");
		else if (re == 0)
			sprintf(s, "0x200e");
		else if (r22 == 0)
			sprintf(s, "0x2022");
		else if (r27 == 0)
			sprintf(s, "0x2027");
		else if (r29 == 0)
			sprintf(s, "0x2029");
		else
			sprintf(s, "");

		//processing
		((CLP*)m_parent)->Brkevoff(s);
		goto ex;
	}
	//end command evoff
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command id
	res = CmpStr("id", 2, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkid();
		goto ex;
	}
	//end command id
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command statfxl
	res = CmpStr("statfxl", 7, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkstatfxl();
		goto ex;
	}
	//end command statfxl
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command statnzl
	res = CmpStr("statnzl", 7, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkstatnzl();
		goto ex;
	}
	//end command statfxl
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command stat_m
	res = CmpStr("stat_m", 6, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkstat_m();
		goto ex;
	}
	//end command stat_m
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command stat_pdo
	res = CmpStr("stat_pdo", 8, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkstat_pdo();
		goto ex;
	}
	//end command stat_pdo
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command stat_lp
	res = CmpStr("stat_lp", 7, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkstat_lp();
		goto ex;
	}
	//end command stat_lp
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command lamp
	res = CmpStr("lamp", 4, item, itemlen);
	if (res == 0) {
		int lampID;
		int lampACT;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no lamp ID parameters after lamp
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters after lamp");
			goto ex;
		}
		if (itemlen == 1 && item[0] >= '1' && item[0] <= '9') {
			lampID = item[0] - '0';
		}
		else if (itemlen == 3 && (item[2] == 'a' || item[2] == 'A')) {
			lampID = 10;
		}
		else {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}
		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no lamp action parameters after lamp
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters after lamp");
			goto ex;
		}
		if (itemlen == 1 && item[0] >= '0' && item[0] <= '2') {
			lampACT = item[0] - '0';
		}
		else {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brklamp(lampID, lampACT);
		goto ex;
	}
	//end command lamp
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command map
	res = CmpStr("map", 3, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkmap();
		goto ex;
	}
	//end command map
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command rmap
	res = CmpStr("rmap", 4, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkrmap();
		goto ex;
	}
	//end command rmap
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command rdid
	res = CmpStr("rdid", 4, item, itemlen);
	if (res == 0) {
		int page = 0;//1...17(pages),98(tag),99(all pages)
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No page parameter");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//decide page number
		lres = ToInteger(item, itemlen, &page);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		//processing according to page number
		((CLP*)m_parent)->Brkrdid(page);
		goto ex;
	}
	//end command rdid
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command wrid
	res = CmpStr("wrid", 4, item, itemlen);
	if (res == 0) {
		int page = 0;//1...17(pages),98(tag),99(all pages)
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No page parameter");
			goto ex;
		}
		//decide page number
		lres = ToInteger(item, itemlen, &page);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadLotID(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No LotID parameter");
			goto ex;
		}
		char lotID[20];
		int lotIDlen;
		if (itemlen > 18) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong lotID");
			goto ex;
		}
		for (int i = 0; i < itemlen; i++)
			lotID[i] = item[i];
		lotIDlen = itemlen;

		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkwrid(page, lotID, lotIDlen);
		goto ex;
	}
	//end command wrid
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command resid
	res = CmpStr("resid", 5, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkresid();
		goto ex;
	}
	//end command resid
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command e84t
	res = CmpStr("e84t", 4, item, itemlen);
	if (res == 0) {
		int tp1, tp2, tp3, tp4, tp5, tp6, td1;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp1);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp2);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp3);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp4);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp5);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &tp6);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &td1);
		if (lres != 0) {//wrong parameters after e84t
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brke84t(tp1, tp2, tp3, tp4, tp5, tp6, td1);
		goto ex;
	}
	//end command e84t
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command smcr: save e84t setting to flash memory
	res = CmpStr("smcr", 4, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brksmcr();
		goto ex;
	}
	//end command smcr
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command ene84nz
	res = CmpStr("ene84nz", 7, item, itemlen);
	if (res == 0) {
		int onoff;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &onoff);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkene84nz(onoff);
		goto ex;
	}
	//end command ene84nz
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command enltc
	res = CmpStr("enltc", 5, item, itemlen);
	if (res == 0) {
		int onoff;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &onoff);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkenltc(onoff);
		goto ex;
	}
	//end command enltc
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command getconf
	res = CmpStr("getconf", 7, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkgetconf();
		goto ex;
	}
	//end command getconf
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command setconf
	res = CmpStr("setconf", 7, item, itemlen);
	if (res == 0) {
		int p1, p2, p3, p4;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &p1);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &p2);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &p3);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &p4);
		if (lres != 0) {//wrong parameters after enltc
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brksetconf(p1, p2, p3, p4);
		goto ex;
	}
	//end command setconf
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command shutdown
	res = CmpStr("shutdown", 8, item, itemlen);
	if (res == 0) {
		int cmd = 0;//0:Reboot , 1:Shutdown
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			//processing
			((CLP*)m_parent)->Brkshutdown(cmd);
			goto ex;
		}

		//Decide "0:Reboot" or "1:Shutdown"
		lres = ToInteger(item, itemlen, &cmd);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkshutdown(cmd);
		goto ex;
	}
	//end command shutdown
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command update
	res = CmpStr("update", 6, item, itemlen);
	if (res == 0) {
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after update
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkupdate(item, itemlen);
		goto ex;
	}
	//end command update
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command ene84
	res = CmpStr("ene84", 5, item, itemlen);
	if (res == 0) {
		int onoff, addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after ene84
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &onoff);
		if (lres != 0) {//wrong parameters after ene84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after ene84
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after ene84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkene84(onoff, addr);
		goto ex;
	}
	//end command ene84
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command rde84
	res = CmpStr("rde84", 5, item, itemlen);
	if (res == 0) {
		int addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after rde84
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after rde84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkrde84(addr);
		goto ex;
	}
	//end command rde84
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command esmode 
	res = CmpStr("esmode", 6, item, itemlen);
	if (res == 0) {
		int mode;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after esmode
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &mode);
		if (lres != 0) {//wrong parameters after esmode
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkesmode(mode);
		goto ex;
	}
	//end command esmode
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command mch 
	res = CmpStr("mch", 3, item, itemlen);
	if (res == 0) {
		int status;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after esmode
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = HEX2Integer(item, itemlen, &status);
		if (lres != 0) {//wrong parameters after esmode
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkmchstatus(status);
		goto ex;
	}
	//end command mch
	//////////////////////////////////////////////////////////////////




	//////////////////////////////////////////////////////////////////
	//command ho_avbl
	res = CmpStr("ho_avbl", 7, item, itemlen);
	if (res == 0) {
		int ho_avbl, addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after ho_avbl
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &ho_avbl);
		if (lres != 0) {//wrong parameters after ho_avbl
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after ho_avbl
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after ene84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkho_avbl(ho_avbl, addr);
		goto ex;
	}
	//end command ho_avbl
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command es
	res = CmpStr("es", 2, item, itemlen);
	if (res == 0) {
		int es, addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after es
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &es);
		if (lres != 0) {//wrong parameters after es
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after es
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after es
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkes(es, addr);
		goto ex;
	}
	//end command es
	//////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////
	//command out_e84
	res = CmpStr("out_e84", 7, item, itemlen);
	if (res == 0) {
		char out_e84[5];
		int addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after out_e84
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		if (itemlen != 4) {//wrong parameters after out_e84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}
		for (int i = 0; i < 4; i++)
			out_e84[i] = item[i];
		out_e84[4] = '\0';

		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after out_e84
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after out_e84
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->Brkout_e84(out_e84, addr);
		goto ex;
	}
	//end command out_e84
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	//command act_purge
	res = CmpStr("act_purge", 9, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->BrkPurge(1);//1-Activate;
		goto ex;
	}
	//end command load
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	//command deact_purge
	res = CmpStr("deact_purge", 11, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->BrkPurge(2);//2-Deactivate
		goto ex;
	}
	//end command load
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	//command date (date YYYY MM DD hh mm)
	res = CmpStr("date", 4, item, itemlen);
	if (res == 0) {
		int iYear, iMonth, iDay, iHour, iMinute;

		//Year(YYYY)
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) { //No parameter mean "Query Date".
		  //((CLP*)m_parent)->m_log->WriteLog("No enough parameters");             

		  //processing
			((CLP*)m_parent)->Brkdate();

			goto ex;
		}
		lres = ToInteger(item, itemlen, &iYear);
		if (lres != 0) {//wrong parameters
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		//Month(MM)
		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &iMonth);
		if (lres != 0) {//wrong parameters
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		//Day(DD)
		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &iDay);
		if (lres != 0) {//wrong parameters
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		//Hour(hh)
		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &iHour);
		if (lres != 0) {//wrong parameters
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		//Minute(mm)
		lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &iMinute);
		if (lres != 0) {//wrong parameters
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}

		//Validate the input parameter    
		if (iYear < 1000 || iYear > 9999)
			goto ex;
		else if (iMonth < 1 || iMonth > 12)
			goto ex;
		else if (iDay < 1 || iDay > 31)
			goto ex;
		else if (iHour < 0 || iHour > 23)
			goto ex;
		else if (iMinute < 0 || iMinute > 59)
			goto ex;

		//processing
		char datecommand[128];
		//Convert to "date MMDDhhmmYYYY" for Linux to send "date" format.
		sprintf(datecommand, "date %02d%02d%02d%02d%04d", iMonth, iDay, iHour, iMinute, iYear);
		((CLP*)m_parent)->m_log->WriteLog(datecommand);
		system(datecommand);

		sleep(2);//2 second

		((CLP*)m_parent)->m_log->WriteLog("hwclock -w");
		system("hwclock -w");

		goto ex;
	}
	//end command date
	//////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////
	//command check SBC type. 0 => old version PPM-LX800. 1 => new version PPM-C412. 
	res = CmpStr("ver_sbc", 7, item, itemlen);
	if (res == 0) {
		int lres = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		((CLP*)m_parent)->CheckHWstatus(); // return 0 => old SBC.  return 1 => new SBC.
		goto ex;
	}
	//end command load
	//////////////////////////////////////////////////////////////////

	/////////////////////////////////////////////////////////////////////
	// Modify RS232 Receive Max retry times, default = 10
	res = CmpStr("mrt", 3, item, itemlen);
	if (res == 0) {
		int addr;
		int lres = ReadItem(item, 100, &itemlen, pM, len, &msgstPos);
		if (lres != 0) {//no parameters after mrt
			((CLP*)m_parent)->m_log->WriteLog("No enough parameters");
			goto ex;
		}
		lres = ToInteger(item, itemlen, &addr);
		if (lres != 0) {//wrong parameters after mrt
			((CLP*)m_parent)->m_log->WriteLog("Wrong parameters");
			goto ex;
		}

		int lres2 = SearchStr("\r\n", 2, pM, len, &msgstPos);
		if (lres2 != 0) {
			((CLP*)m_parent)->m_log->WriteLog("Wrong ending or excess parameters");
			goto ex;
		}
		//processing
		msgMaxRetries = addr;
		goto ex;
	}
	//end command load
	//////////////////////////////////////////////////////////////////



	//command ...
	//unknown command
	((CLP*)m_parent)->m_log->WriteLog("Unknow command");


ex:  delete[] pM;
}

int CHostBK::ToInteger(char *item, int itemlen, int *num) {
	*num = 0;
	for (int i = 0; i < itemlen; i++) {
		if (item[i]<'0' || item[i]>'9')
			return 1;//failed
		*num = (*num) * 10 + item[i] - '0';
	}
	return 0;
}

int CHostBK::HEX2Integer(char *item, int itemlen, int *num) {
	*num = 0;
	if (itemlen < 2)
		return 1;//not enough length
	if (item[0] != '0' || (item[1] != 'x' && item[1] != 'X'))
		return 2;//wrong format

	for (int i = 2; i < itemlen; i++) {
		if (item[i] >= '0' && item[i] <= '9')
			*num = (*num) * 16 + item[i] - '0';
		else if (item[i] >= 'a' && item[i] <= 'f')
			*num = (*num) * 16 + item[i] - 'a' + 10;
		else if (item[i] >= 'A' && item[i] <= 'F')
			*num = (*num) * 16 + item[i] - 'A' + 10;
		else
			return 3;
	}
	return 0;
}

int CHostBK::SearchStr(const char *searstr, int searstrlen, char *msg, int msglen, int *msgstPos) {
	//search for cmpstr in msg starting from bstPos. if succeed, forward msgstPos by cmpstrlen
	int stPos = *msgstPos;
	for (int i = 0; i < searstrlen; i++) {
		if (i + stPos >= msglen)
			return 1;
		if (searstr[i] != msg[i + stPos])
			return 2;
	}
	*msgstPos += searstrlen;
	return 0;
}

int CHostBK::CmpStr(const char *cmpstr, int cmpstrlen, char *newmsg, int newmsglen) {
	if (cmpstrlen != newmsglen)
		return 1;
	for (int i = 0; i < cmpstrlen; i++) {
		if (cmpstr[i] != newmsg[i])
			return 2;
	}
	return 0;
}

int CHostBK::ReadItem(char *newmsg, int newmsgMaxlen, int *newmsglen,
	char *msg, int msglen, int *msglenstPos) {
	//read an item in msg starting from bstPos, forward bstPos by item length 
	//plus length of space starting from bstPos and before the item.
	//store the item in newmsg and acutual length of item in aclen (>0)
	//if item length > newmsglen, return nonzero meaning failed
	//if msglenstPos >= msglen, return nonzero meaning failed
	//if aclen == 0, return nonzero meaning failed
	*newmsglen = 0;
	while (*msglenstPos < msglen) {
		if (msg[*msglenstPos] == ' ') (*msglenstPos)++;
		else break;
	}
	if (*msglenstPos >= msglen)
		return 1;
	while (*msglenstPos < msglen) {
		if (msg[*msglenstPos] != ' ' && msg[*msglenstPos] != '\r' && msg[*msglenstPos] != '\n') {
			if (*newmsglen >= newmsgMaxlen) return 2;
			newmsg[*newmsglen] = msg[*msglenstPos];
			(*newmsglen)++;
			(*msglenstPos)++;
		}
		else break;
	}
	if (*newmsglen == 0)
		return 3;
	return 0;
}

int CHostBK::ReadLotID(char *a, int alen, int *aclen, char *b, int blen, int *bstPos) {
	//read an LotID in b starting from bstPos, forward bstPos by LotID length(18)
	//plus space length starting from bstPos before it.
	//store the item in a and acutual length of item in aclen (>0)
	//if item length > alen, return nonzero meaning failed
	//if bstPos >= blen, return nonzero meaning failed
	//if aclen == 0, return nonzero meaning failed
	int seperator_cnt = 0;
	*aclen = 0;
	while (*bstPos < blen) {
		if (b[*bstPos] == ' ') (*bstPos)++;
		else break;
	}
	if (*bstPos >= blen)
		return 1;
	while (*bstPos < blen) {
		if (b[*bstPos] != '\r' && b[*bstPos] != '\n') {
			if (b[*bstPos] == '\'')
				seperator_cnt++;
			if (*aclen >= alen) return 2;
			a[*aclen] = b[*bstPos];
			(*aclen)++;
			(*bstPos)++;
			if (seperator_cnt == 2)
				break;
		}
		else break;
	}
	if (*aclen == 0)
		return 3;
	return 0;
}
void CHostBK::DoTest() {
	//brooks ASCII protocol test
	char st[100];
	//sprintf(st, "io wrid 4 '1234123412341234'\r\n");
	//sprintf(st, "io e84t 1 22 33 44 555 666 7\r\n");
	//sprintf(st, "io smcr\r\n");
	//sprintf(st, "io ene84 1 0\r\n");
	//sprintf(st, "io rde84 0\r\n");
	//sprintf(st, "io ho_avbl 1 0\r\n");
	//sprintf(st, "io es 1 1\r\n");
	sprintf(st, "io out_e84 0x49 1\r\n");
	// m_pSerial->SendBlock(st, strlen(st));
}
int CHostBK::ReadItemIO(char *newmsg, int newmsgMaxlen, int *newmsglen,
	char *msg, int msglen, int *msglenstPos) {
	//read an item in msg starting from bstPos, forward bstPos by item length 
	//plus length of space starting from bstPos and before the item.
	//store the item in newmsg and acutual length of item in aclen (>0)
	//if item length > newmsglen, return nonzero meaning failed
	//if msglenstPos >= msglen, return nonzero meaning failed
	//if aclen == 0, return nonzero meaning failed
	*newmsglen = 0;
	while (*msglenstPos < msglen) {
		if (msg[*msglenstPos] != 'i') (*msglenstPos)++;
		else break;
	}
	if (*msglenstPos >= msglen)
		return 1;
	while (*msglenstPos < msglen) {
		if (msg[*msglenstPos] != ' ' && msg[*msglenstPos] != '\r' && msg[*msglenstPos] != '\n') {
			if (*newmsglen >= newmsgMaxlen) return 2;
			newmsg[*newmsglen] = msg[*msglenstPos];
			(*newmsglen)++;
			(*msglenstPos)++;
		}
		else break;
	}
	if (*newmsglen == 0)
		return 3;
	return 0;
}

//End CHostBK Class Implementation
//////////////////////////////////
//////////////////////////////////
//Tas300 Class Implementation
CTas300::CTas300(int lpID, void *parent) {
	m_lpID = lpID;
	m_parent = parent;
	m_OpCmdState = IDLE;
	//m_curOpCmd = CMD_NONE;
	m_InfCmdState = IDLE;
	//m_curInfCmd = CMD_NONE;
	m_fpStatus = FPS_UNKNOWN;//-1
	m_fpEvent = FPEVT_NONE;

	m_Status.eqpStatus = '0';
	m_Status.mode = '0';
	m_Status.inited = '0';
	m_Status.opStatus = '0';

	int res = sem_init(&m_semOpCmdACK, 0, 0);
	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("m_semOpCmdACK initialization failed");

	res = sem_init(&m_semOpCmdINF, 0, 0);
	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("m_semOpCmdINF initialization failed");

	res = sem_init(&m_semInfCmdACK, 0, 0);
	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("m_semInfCmdACK initialization failed");

	((CLP*)m_parent)->m_log->WriteLog("CTas300 Constructor called");
	m_pSerial = new CSerial(m_lpID, "Tas"/*log file name base*/);
	m_pSerial->Initialize(TAS300, this, CallbackWrap);
}
CTas300::~CTas300() {
	((CLP*)m_parent)->m_log->WriteLog("CTas300 Destructor called");
	if (m_pSerial != NULL)
		delete m_pSerial;
}

int CTas300::OpenPort() {
	return m_pSerial->OpenConnection();
}
int CTas300::ClosePort() {
	return m_pSerial->CloseConnection();
}

void CTas300::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	CTas300 *pSelf = (CTas300*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

int CTas300::sem_reset(sem_t *sem)
{
	int res;
	while ((res = sem_trywait(sem)) == 0);
	//while((res = sem_trywait(&m_semInfCmdACK)) == 0)
	//  ((CLP*)m_parent)->m_log->WriteLog("sem_trywait succeeded!");
	if (res != -1)
		return 1;//internal error
	if (errno == EAGAIN) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EAGAIN");
		return 0;
	}
	if (errno == EDEADLK) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EDEADLK");
		return 2;//deadlock detected
	}
	if (errno == EINVAL) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINVAL");
		return 3;//invalid semaphore descriptor sem
	}
	if (errno == EINTR) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINTR");
		return 4;//A signal interrupted sem_trywait function
	}
	return 5;//unknown errno
}


void CTas300::Callback(char *pMsg, int len) {
	char *pM = new char[len];
	for (int l = 0; l < len; l++) {
		pM[l] = pMsg[l];
	}
	//Do processing of message bellow 
	((CLP*)m_parent)->m_log->WriteLog("Fr TAS300", pM, len);
	////////////////////////////////////
	//Analysing message
	int dlen = 0;
	int checksum = 0;
	char cs, csh, csl;
	char msgType[4];
	char msgName[6];
	char msgParam[PARAMLEN];
	int plen;
	//length and checksum
	if (len < 3) {
		((CLP*)m_parent)->m_log->WriteLog("Message too short");
		goto ex;
	}

	dlen = pM[1]; dlen << 8; dlen += pM[2];
	if ((dlen + 4) != len) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong length");
		goto ex;
	}

	for (int i = 1; i < len - 3; i++) checksum += pM[i];
	cs = checksum;
	csh = (cs >> 4) & 0x0F;
	csl = cs & 0x0F;
	if (csh < 0x0A) csh = '0' + csh;
	else csh = 'A' + csh - 10;
	if (csl < 0x0A) csl = '0' + csl;
	else csl = 'A' + csl - 10;

	if (pM[len - 2] != csl || pM[len - 3] != csh) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong checksum");
		goto ex;
	}
	//Header
	if (pM[0] != 0x01) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong header");
		goto ex;
	}
	//Tail
	if (pM[len - 1] != 0x03) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong tail");
		goto ex;
	}
	//Message Type
	for (int i = 0; i < 3; i++) msgType[i] = pM[5 + i]; msgType[3] = '\0';
	//colon :
	if (pM[8] != ':') {
		((CLP*)m_parent)->m_log->WriteLog("No : between type and name");
		goto ex;
	}
	//Message Name
	for (int i = 0; i < 5; i++) msgName[i] = pM[9 + i]; msgName[5] = '\0';
	//Message parameter length eg./0010...
	plen = len - 18;//including '/'
	if (plen < 0 || plen == 1 || (plen > 1 && pM[14] != '/')) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong format");
		goto ex;
	}
	//Message parameter
	msgParam[0] = '\0';
	if (plen > PARAMLEN) {
		((CLP*)m_parent)->m_log->WriteLog("Parameter too long");
		goto ex;
	}
	if (plen > 1) {
		int i;
		for (i = 0; i < plen - 1; i++) msgParam[i] = pM[15 + i];
		msgParam[i] = '\0';
	}
	/////////////////////////////////////////
	//ACK type
	if (strcmp("ACK", msgType) == 0) {
		if (m_InfCmdState == WAITACK && strcmp(msgName, m_curInfCmdName) == 0) {
			strcpy(m_InfCmdResParam, msgParam);
			m_InfCmdRes = RES_ACK;
			sem_post(&m_semInfCmdACK);
			goto ex;
		}
		if (m_OpCmdState == WAITACK && strcmp(msgName, m_curOpCmdName) == 0) {
			strcpy(m_OpCmdResParam, msgParam);
			m_OpCmdRes = RES_ACK;
			sem_post(&m_semOpCmdACK);
			goto ex;
		}
		((CLP*)m_parent)->m_log->WriteLog("Not expected Acknowledge");
		goto ex;
	}
	//////////////////////////////////////////
	//NAK type
	if (strcmp("NAK", msgType) == 0) {
		if (m_InfCmdState == WAITACK && strcmp(msgName, m_curInfCmdName) == 0) {
			strcpy(m_InfCmdResParam, msgParam);
			m_InfCmdRes = RES_NAK;
			sem_post(&m_semInfCmdACK);
			goto ex;
		}
		if (m_OpCmdState == WAITACK && strcmp(msgName, m_curOpCmdName) == 0) {
			strcpy(m_OpCmdResParam, msgParam);
			m_OpCmdRes = RES_NAK;
			sem_post(&m_semOpCmdACK);
			goto ex;
		}
		((CLP*)m_parent)->m_log->WriteLog("Unkown NAK");
		goto ex;
	}
	//////////////////////////////////////////
	//INF type
	if (strcmp("INF", msgType) == 0) {
		if (m_OpCmdState == WAITINF && strcmp(msgName, m_curOpCmdName) == 0) {
			strcpy(m_OpCmdResParam, msgParam);
			m_OpCmdRes = RES_INF;
			sem_post(&m_semOpCmdINF);
			goto ex;
		}
		char typedname[20];
		char frame[50];
		int cmdlen;
		sprintf(typedname, "FIN:%s", msgName);
		prepCmd(typedname, frame, 50, &cmdlen);
		//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
		//m_pSerial->SendBlock(frame, cmdlen);

		if (strcmp(msgName, "PODOF") == 0) {
			/*
			m_arg.pObj = m_parent;
			m_arg.prm1 = 0;
			pthread_t a_thread;
			int res = pthread_create(&a_thread,NULL,PodEvtThreadTas300,(void *)&m_arg);
			if(res != 0){
			  ((CLP*)m_parent)->m_log->WriteLog("Thread creation failed");
			}
			*/
			((CLP*)m_parent)->TasPodEvt(0);
		}
		else if (strcmp(msgName, "SMTON") == 0) {
			((CLP*)m_parent)->TasPodEvt(1);
		}
		else if (strcmp(msgName, "ABNST") == 0) {
			((CLP*)m_parent)->TasPodEvt(2);
		}
		else if (strcmp(msgName, "PODON") == 0) {
			/*
			m_arg.pObj = m_parent;
			m_arg.prm1 = 3;
			pthread_t a_thread;
			int res = pthread_create(&a_thread,NULL,PodEvtThreadTas300,(void *)&m_arg);
			if(res != 0){
			  ((CLP*)m_parent)->m_log->WriteLog("Thread creation failed");
			}
			*/
			((CLP*)m_parent)->TasPodEvt(3);
		}
		else if (strcmp(msgName, "MANSW") == 0) {
			((CLP*)m_parent)->TasManSwEvt();
		}
		else if (strcmp(msgName, "PGWFL") == 0) {
			strcpy(m_EventParam, msgParam);
			((CLP*)m_parent)->TasPGEvent(m_EventParam);
		}
		else
			((CLP*)m_parent)->m_log->WriteLog("Unxpected INF");
		goto ex;
	}
	//////////////////////////////////////////
	//ABS type
	if (strcmp("ABS", msgType) == 0) {
		if (m_OpCmdState == WAITINF && strcmp(msgName, m_curOpCmdName) == 0) {
			strcpy(m_OpCmdResParam, msgParam);
			m_OpCmdRes = RES_ABS;
			sem_post(&m_semOpCmdINF);
			goto ex;
		}
		char typedname[20];
		char frame[50];
		int cmdlen;
		sprintf(typedname, "FIN:%s", msgName);
		prepCmd(typedname, frame, 50, &cmdlen);
		//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
		//m_pSerial->SendBlock(frame, cmdlen);
		((CLP*)m_parent)->m_log->WriteLog("Unkown ABS");
		goto ex;
	}
	//////////////////////////////////////////
	//RIF type: Currently do not use
	if (strcmp("RIF", msgType) == 0) {
		((CLP*)m_parent)->m_log->WriteLog("Received RIF");
		goto ex;
	}
	//////////////////////////////////////////
	//RAS type: Currently do not use
	if (strcmp("RAS", msgType) == 0) {
		((CLP*)m_parent)->m_log->WriteLog("Received RAS");
		goto ex;
	}
	//////////////////////////////////////////
	//Unknown response type
	((CLP*)m_parent)->m_log->WriteLog("Unknow response type");

ex:  delete[] pM;
}

void CTas300::DoTest() {
	char frame[50];
	int cmdlen;
	int res = prepCmd("ABS:PODON", frame, 50, &cmdlen);
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
}

int CTas300::prepCmd(const char *cmdstr, char *frame, int fmaxlen, int *factulen) {
	//parameter cmdstr: example, "MOV:ORGSH"
	int checksum = 0;
	int len = strlen(cmdstr);
	int dlen = len + 5;//data len: from ADR to CSl
	*factulen = len + 9;//total len;
	if (*factulen > fmaxlen)
		return 1;//frame too short to hold command
	frame[0] = 0x01;//SOH
	frame[1] = dlen >> 8; frame[2] = dlen;//LEN(binary)
	frame[3] = '0';  frame[4] = '0';//ADR
	for (int i = 5; i < len + 5; i++) frame[i] = cmdstr[i - 5];
	frame[len + 5] = ';';//CMD
	for (int i = 1; i < len + 6; i++) checksum += frame[i];
	char cs = checksum;
	char csh = (cs >> 4) & 0x0F;
	char csl = cs & 0x0F;
	if (csh < 0x0A) frame[len + 6] = '0' + csh;
	else frame[len + 6] = 'A' + csh - 10;
	if (csl < 0x0A) frame[len + 7] = '0' + csl;
	else frame[len + 7] = 'A' + csl - 10;//CSh and CSl
	frame[len + 8] = 0x03;//ETX
	return 0;
}
int CTas300::statfxl() {
	//if succeed,
	//this member function will update the m_Status, m_statfxl[5], m_fpStatus
	//if return 8, m_fpStatus and m_statfxl[5] will not be updated, but m_Status
	//will still be updated
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	//no foup
	int res = prepCmd("ACK:STATE/00001A00101000101000", frame, 50, &cmdlen);
	//ABORGed
	//int res = prepCmd("ACK:STATE/00100010101000101000", frame, 50, &cmdlen);
	//clamped
	//int res = prepCmd("ACK:STATE/00100011101000101100", frame, 50, &cmdlen);
	//docked
	//int res = prepCmd("ACK:STATE/00100011101001101100", frame, 50, &cmdlen);
	//loaded
	//int res = prepCmd("ACK:STATE/00100011010111111000", frame, 50, &cmdlen);
#else
	int res = prepCmd("GET:STATE", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_InfCmdState = WAITACK;
	m_InfCmdRes = RES_NON;
	strcpy(m_curInfCmdName, "STATE");
	res = sem_reset(&m_semInfCmdACK);
	if (res != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Error happened during sem_reset!");
		return 3;
	}
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semInfCmdACK, &tm);
	m_InfCmdState = IDLE;
	if (res == -1)
		return 4;//wait timed out
	if (m_InfCmdRes == RES_NAK) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_InfCmdRes != RES_ACK)
		return 6;//non ack & non nak result
	if (strlen(m_InfCmdResParam) != 20)
		return 7;//incorrect parameter length
	  ///////////////////////////////////////
	  //process result: now m_InfCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_InfCmdResParam, strlen(m_InfCmdResParam));
	m_Status.eqpStatus = m_InfCmdResParam[0];
	m_Status.mode = m_InfCmdResParam[1];
	m_Status.inited = m_InfCmdResParam[2];
	m_Status.opStatus = m_InfCmdResParam[3];
	if (m_InfCmdResParam[4] >= '0' && m_InfCmdResParam[4] <= '9')
		m_Status.ecode = m_InfCmdResParam[4] - '0';
	else if (m_InfCmdResParam[4] >= 'A' && m_InfCmdResParam[4] <= 'F')
		m_Status.ecode = m_InfCmdResParam[4] - 'A' + 10;
	else if (m_InfCmdResParam[4] >= 'a' && m_InfCmdResParam[4] <= 'f')
		m_Status.ecode = m_InfCmdResParam[4] - 'a' + 10;
	else
		return 8;//incorrect parameters
	m_Status.ecode <<= 4;
	if (m_InfCmdResParam[5] >= '0' && m_InfCmdResParam[5] <= '9')
		m_Status.ecode += m_InfCmdResParam[5] - '0';
	else if (m_InfCmdResParam[5] >= 'A' && m_InfCmdResParam[5] <= 'F')
		m_Status.ecode += m_InfCmdResParam[5] - 'A' + 10;
	else if (m_InfCmdResParam[5] >= 'a' && m_InfCmdResParam[5] <= 'f')
		m_Status.ecode += m_InfCmdResParam[5] - 'a' + 10;
	else
		return 8;//incorrect parameters
	m_Status.fpPlace = m_InfCmdResParam[6];
	m_Status.fpClamp = m_InfCmdResParam[7];
	m_Status.ltchKey = m_InfCmdResParam[8];
	m_Status.vacuum = m_InfCmdResParam[9];
	m_Status.fpDoor = m_InfCmdResParam[10];
	m_Status.wfFlyOutSensor = m_InfCmdResParam[11];
	m_Status.zPos = m_InfCmdResParam[12];
	m_Status.yPos = m_InfCmdResParam[13];
	m_Status.mpArmPos = m_InfCmdResParam[14];
	m_Status.mpzPos = m_InfCmdResParam[15];
	m_Status.mpStoper = m_InfCmdResParam[16];
	m_Status.mapingStatus = m_InfCmdResParam[17];
	m_Status.intKey = m_InfCmdResParam[18];
	m_Status.infoPad = m_InfCmdResParam[19];
	unsigned char stfxl = 0xFF;//if assigned new value, won't still be 0xFF
	if (m_Status.fpPlace == '0') {//not present
		stfxl = 0x28;//0010`1000=>FPS_NOFOUP
		m_fpStatus = FPS_NOFOUP;
	}
	else if (m_Status.fpPlace == '1') {//normal placement
		if (m_Status.fpClamp == '0') {//released
			stfxl = 0x69;//0110`1001=>FPS_PLACED
			m_fpStatus = FPS_PLACED;
		}
		else if (m_Status.fpClamp == '1') {//clamped
			if (m_Status.yPos == '0') {//undock
				stfxl = 0x59;//0101`1001=>FPS_CLAMPED
				m_fpStatus = FPS_CLAMPED;
			}
			else if (m_Status.yPos == '1') {//docked
				if (m_Status.fpDoor == '1') {//door closed
					stfxl = 0x53;//0101`0011=>FPS_DOCKED
					m_fpStatus = FPS_DOCKED;
				}
				else if (m_Status.fpDoor == '0') {//door open
					stfxl = 0x57;//0101`0111=>FPS_OPENED
					m_fpStatus = FPS_OPENED;
				}
				else {// if(m_Status.fpDoor == '0'){//undefined door open status
				  //Do nothing, do not update status
				}
			}
		}
		else {// if(m_Status.fpClamp == '?'){//undefined clamp status
		  //Do nothing, do not update status
		}
	}
	else {//error placement  if(m_Status.fpPlace == '2'){
		stfxl = 0x68;//0110`1000=>FPS_ERROR(FPS present but not placed)
		//m_fpStatus = FPS_UNKNOWN;//don't update foup status
	}
	if (stfxl == 0xFF) return 9;
	m_statfxl[0] = '0';
	m_statfxl[1] = 'x';
	m_statfxl[3] = '0' + (stfxl & 0x0F);
	m_statfxl[2] = '0' + ((stfxl >> 4) & 0x0F);
	m_statfxl[4] = '\0';
	//end process result
	///////////////////////////////////////
	return 0;
}

int CTas300::statn2purge() {
	//if succeed, this member function will update the m_n2pStatus
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	//gas pressure normal and nozzle position down
	int res = prepCmd("ACK:PGSTA/00000000000000000000", frame, 50, &cmdlen);
	//gas pressure abnormal and nozzle position down
	//int res = prepCmd("ACK:PGSTA/0000000000000E000000", frame, 50, &cmdlen);
	//gas pressure normal and nozzle position up
	//int res = prepCmd("ACK:PGSTA/00000000000000100000", frame, 50, &cmdlen);
	//gas pressure normal and nozzle position abnormal
	//int res = prepCmd("ACK:PGSTA/00000000000000200000", frame, 50, &cmdlen);
#else
	int res = prepCmd("GET:PGSTA", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_InfCmdState = WAITACK;
	m_InfCmdRes = RES_NON;
	strcpy(m_curInfCmdName, "PGSTA");
	res = sem_reset(&m_semInfCmdACK);
	if (res != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Error happened during sem_reset!");
		return 3;
	}
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semInfCmdACK, &tm);
	m_InfCmdState = IDLE;
	if (res == -1)
		return 4;//wait timed out
	if (m_InfCmdRes == RES_NAK) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_InfCmdRes != RES_ACK)
		return 6;//non ack & non nak result
	if (strlen(m_InfCmdResParam) != 20)
		return 7;//incorrect parameter length
	  ///////////////////////////////////////
	  //process result: now m_InfCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_InfCmdResParam, strlen(m_InfCmdResParam));
	m_n2pStatus.gasPressure = m_InfCmdResParam[13];
	m_n2pStatus.nozzlePos = m_InfCmdResParam[16];
	//end process result
	///////////////////////////////////////
	return 0;
}

int CTas300::mapResult() {
	//if succeed,
	//this member function will update the m_mapRes[26]
	//if failed, m_mapRes will not be updated
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:MAPRD/0010001101011111100011111", frame, 50, &cmdlen);
#else
	int res = prepCmd("GET:MAPRD", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_InfCmdState = WAITACK;
	m_InfCmdRes = RES_NON;
	strcpy(m_curInfCmdName, "MAPRD");
	res = sem_reset(&m_semInfCmdACK);
	if (res != 0)
		return 3;//Error happened during sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semInfCmdACK, &tm);
	m_InfCmdState = IDLE;
	if (res == -1)
		return 4;//wait timed out
	if (m_InfCmdRes == RES_NAK) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_InfCmdRes != RES_ACK)
		return 6;//non ack & non nak result
	if (strlen(m_InfCmdResParam) != 25)
		return 7;//incorrect parameter length
	  ///////////////////////////////////////
	  //process result: now m_InfCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_InfCmdResParam, strlen(m_InfCmdResParam));
	strcpy(m_mapRes, m_InfCmdResParam);
	//end process result
	///////////////////////////////////////
	return 0;
}

int CTas300::evtON() {
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:EVTON", frame, 50, &cmdlen);
#else
	int res = prepCmd("EVT:EVTON", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_InfCmdState = WAITACK;
	m_InfCmdRes = RES_NON;
	strcpy(m_curInfCmdName, "EVTON");
	res = sem_reset(&m_semInfCmdACK);
	if (res != 0)
		return 3;//Error happened during sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semInfCmdACK, &tm);
	m_InfCmdState = IDLE;
	if (res == -1)
		return 4;//wait timed out
	if (m_InfCmdRes == RES_NAK) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_InfCmdRes != RES_ACK)
		return 6;//non ack & non nak result
	  ///////////////////////////////////////
	  //process result: now m_InfCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_InfCmdResParam, strlen(m_InfCmdResParam));
	//end process result
	///////////////////////////////////////
	return 0;
}

int CTas300::fpeON() {
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:FPEON", frame, 50, &cmdlen);
#else
	int res = prepCmd("EVT:FPEON", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_InfCmdState = WAITACK;
	m_InfCmdRes = RES_NON;
	strcpy(m_curInfCmdName, "FPEON");
	res = sem_reset(&m_semInfCmdACK);
	if (res != 0)
		return 3;//Error happened during sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semInfCmdACK, &tm);
	m_InfCmdState = IDLE;
	if (res == -1)
		return 4;//wait timed out
	if (m_InfCmdRes == RES_NAK) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_InfCmdRes != RES_ACK)
		return 6;//non ack & non nak result
	  ///////////////////////////////////////
	  //process result: now m_InfCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_InfCmdResParam, strlen(m_InfCmdResParam));
	//end process result
	///////////////////////////////////////
	return 0;
}

int CTas300::rstErr() {
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:RESET", frame, 50, &cmdlen);
#else
	int res = prepCmd("SET:RESET", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_OpCmdState = WAITACK;
	m_OpCmdRes = RES_NON;
	strcpy(m_curOpCmdName, "RESET");
	res = sem_reset(&m_semOpCmdACK);
	if (res != 0)
		return 3;//Error happened during m_semOpCmdACK sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semOpCmdACK, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_OpCmdRes == RES_NAK) {
		m_OpCmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_OpCmdRes != RES_ACK) {
		m_OpCmdState = IDLE;
		return 6;//non ack & non nak result
	}
	///////////////////////////////////////
	//process result: now m_OpCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_OpCmdResParam, strlen(m_OpCmdResParam));
	//end process result
	///////////////////////////////////////
	m_OpCmdState = WAITINF;//ACKed, wait for INF 
	m_OpCmdRes = RES_NON;
	res = sem_reset(&m_semOpCmdINF);
	if (res != 0)
		return 7;//Error happened during m_semOpCmdINF sem_reset
#ifdef _OfflineTest
	res = prepCmd("ABS:RESET/INTER", frame, 50, &cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
#endif
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 10;
	res = sem_timedwait(&m_semOpCmdINF, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 8;//wait INF timed out
	}
	if (m_OpCmdRes != RES_INF && m_OpCmdRes != RES_ABS) {
		m_OpCmdState = IDLE;
		return 9;//non ABS & non INF result
	}
	res = prepCmd("FIN:RESET", frame, 50, &cmdlen);
	//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	//m_pSerial->SendBlock(frame, cmdlen);
	m_OpCmdState = IDLE;
	if (m_OpCmdRes == RES_ABS) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 10;//ABS result
	}
	return 0;
}

int CTas300::prgInit() {
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:INITL", frame, 50, &cmdlen);
#else
	int res = prepCmd("SET:INITL", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_OpCmdState = WAITACK;
	m_OpCmdRes = RES_NON;
	strcpy(m_curOpCmdName, "INITL");
	res = sem_reset(&m_semOpCmdACK);
	if (res != 0)
		return 3;//Error happened during m_semOpCmdACK sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semOpCmdACK, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_OpCmdRes == RES_NAK) {
		m_OpCmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_OpCmdRes != RES_ACK) {
		m_OpCmdState = IDLE;
		return 6;//non ack & non nak result
	}
	///////////////////////////////////////
	//process result: now m_OpCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_OpCmdResParam, strlen(m_OpCmdResParam));
	//end process result
	///////////////////////////////////////
	m_OpCmdState = WAITINF;//ACKed, wait for INF 
	m_OpCmdRes = RES_NON;
	res = sem_reset(&m_semOpCmdINF);
	if (res != 0)
		return 7;//Error happened during m_semOpCmdINF sem_reset
#ifdef _OfflineTest
	res = prepCmd("INF:INITL", frame, 50, &cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
#endif
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 10;
	res = sem_timedwait(&m_semOpCmdINF, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 8;//wait INF timed out
	}
	if (m_OpCmdRes != RES_INF && m_OpCmdRes != RES_ABS) {
		m_OpCmdState = IDLE;
		return 9;//non ABS & non INF result
	}
	res = prepCmd("FIN:INITL", frame, 50, &cmdlen);
	//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	//m_pSerial->SendBlock(frame, cmdlen);
	m_OpCmdState = IDLE;
	if (m_OpCmdRes == RES_ABS) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 10;//ABS result
	}
	return 0;
}

int CTas300::lampOP(int lmpNo, int opCode) {
	//if(lmpNo<1 || lmpNo>9 || lmpNo==6 ||opCode<LMP_OF || opCode>LMP_BL)
	if (lmpNo < 1 || lmpNo>9 || opCode<LMP_OF || opCode>LMP_BL)
		return 0;//for non exist lamp or opCode, just return succeed.
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char cmdName[6];
	switch (opCode) {
	case LMP_OF:
		sprintf(cmdName, "LOF%02d", lmpNo);
		break;
	case LMP_ON:
		sprintf(cmdName, "LON%02d", lmpNo);
		break;
	case LMP_BL:
		sprintf(cmdName, "LBL%02d", lmpNo);
		break;
	default:
		sprintf(cmdName, "LOF%02d", lmpNo);
	}
	char SetCmdName[10];
#ifdef _OfflineTest
	sprintf(SetCmdName, "ACK:%s", cmdName);
#else
	sprintf(SetCmdName, "SET:%s", cmdName);
#endif
	char frame[50];
	int cmdlen;
	int res = prepCmd(SetCmdName, frame, 50, &cmdlen);
	if (res != 0)
		return 2;//internal error, won't happen;
	m_OpCmdState = WAITACK;
	m_OpCmdRes = RES_NON;
	strcpy(m_curOpCmdName, cmdName);
	res = sem_reset(&m_semOpCmdACK);
	if (res != 0)
		return 3;//Error happened during m_semOpCmdACK sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semOpCmdACK, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_OpCmdRes == RES_NAK) {
		m_OpCmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_OpCmdRes != RES_ACK) {
		m_OpCmdState = IDLE;
		return 6;//non ack & non nak result
	}
	///////////////////////////////////////
	//process result: now m_OpCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_OpCmdResParam, strlen(m_OpCmdResParam));
	//end process result
	///////////////////////////////////////
	m_OpCmdState = WAITINF;//ACKed, wait for INF 
	m_OpCmdRes = RES_NON;
	res = sem_reset(&m_semOpCmdINF);
	if (res != 0)
		return 7;//Error happened during m_semOpCmdINF sem_reset
#ifdef _OfflineTest
	sprintf(SetCmdName, "INF:%s", cmdName);
	res = prepCmd(SetCmdName, frame, 50, &cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
#endif
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 10;
	res = sem_timedwait(&m_semOpCmdINF, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 8;//wait INF timed out
	}
	if (m_OpCmdRes != RES_INF && m_OpCmdRes != RES_ABS) {
		m_OpCmdState = IDLE;
		return 9;//non ABS & non INF result
	}
	sprintf(SetCmdName, "FIN:%s", cmdName);
	res = prepCmd(SetCmdName, frame, 50, &cmdlen);
	//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	//m_pSerial->SendBlock(frame, cmdlen);
	m_OpCmdState = IDLE;
	if (m_OpCmdRes == RES_ABS) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 10;//ABS result
	}
	return 0;
}

int CTas300::movOP(const char *name) {
	if (strlen(name) != 5)
		return 0;//non exist command name, do nothing
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
	char typedname[20];
#ifdef _OfflineTest
	sprintf(typedname, "ACK:%s", name);
	//sprintf(typedname, "NAK:%s/INTPI", name);
#else
	sprintf(typedname, "MOV:%s", name);
#endif
	int res = prepCmd(typedname, frame, 50, &cmdlen);
	if (res != 0)
		return 2;//internal error, won't happen;
	m_OpCmdState = WAITACK;
	m_OpCmdRes = RES_NON;
	strcpy(m_curOpCmdName, name);
	res = sem_reset(&m_semOpCmdACK);
	if (res != 0)
		return 3;//Error happened during m_semOpCmdACK sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semOpCmdACK, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_OpCmdRes == RES_NAK) {
		m_OpCmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_OpCmdRes != RES_ACK) {
		m_OpCmdState = IDLE;
		return 6;//non ack & non nak result
	}
	///////////////////////////////////////
	//process result: now m_OpCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_OpCmdResParam, strlen(m_OpCmdResParam));
	//end process result
	///////////////////////////////////////
	m_OpCmdState = WAITINF;//ACKed, wait for INF 
	m_OpCmdRes = RES_NON;
	res = sem_reset(&m_semOpCmdINF);
	if (res != 0)
		return 7;//Error happened during m_semOpCmdINF sem_reset
#ifdef _OfflineTest
	sprintf(typedname, "INF:%s", name);
	//sprintf(typedname, "ABS:%s/ZLMIT", name);
	res = prepCmd(typedname, frame, 50, &cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
#endif
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 20;
	res = sem_timedwait(&m_semOpCmdINF, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 8;//wait INF timed out
	}
	if (m_OpCmdRes != RES_INF && m_OpCmdRes != RES_ABS) {
		m_OpCmdState = IDLE;
		return 9;//non ABS & non INF result
	}
	sprintf(typedname, "FIN:%s", name);
	res = prepCmd(typedname, frame, 50, &cmdlen);
	//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	//m_pSerial->SendBlock(frame, cmdlen);
	m_OpCmdState = IDLE;
	if (m_OpCmdRes == RES_ABS) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 10;//ABS result
	}
	return 0;
}

int CTas300::movABORT() {
	//MOV:if you don't use resend cmd, abort before send the same 
	if (m_InfCmdState != IDLE)
		return 1;//busy
	char frame[50];
	int cmdlen;
#ifdef _OfflineTest
	int res = prepCmd("ACK:ABORT", frame, 50, &cmdlen);
#else
	int res = prepCmd("MOV:ABORT", frame, 50, &cmdlen);
#endif
	if (res != 0)
		return 2;//internal error, won't happen;
	m_OpCmdState = WAITACK;
	m_OpCmdRes = RES_NON;
	strcpy(m_curOpCmdName, "ABORT");
	res = sem_reset(&m_semOpCmdACK);
	if (res != 0)
		return 3;//Error happened during m_semOpCmdACK sem_reset
	((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semOpCmdACK, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_OpCmdRes == RES_NAK) {
		m_OpCmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 5;//nak result
	}
	if (m_OpCmdRes != RES_ACK) {
		m_OpCmdState = IDLE;
		return 6;//non ack & non nak result
	}
	///////////////////////////////////////
	//process result: now m_OpCmdRes == RES_ACK
	((CLP*)m_parent)->m_log->WriteLog("Parm",
		m_OpCmdResParam, strlen(m_OpCmdResParam));
	//end process result
	///////////////////////////////////////
	m_OpCmdState = WAITINF;//ACKed, wait for INF 
	m_OpCmdRes = RES_NON;
	res = sem_reset(&m_semOpCmdINF);
	if (res != 0)
		return 7;//Error happened during m_semOpCmdINF sem_reset
#ifdef _OfflineTest
	res = prepCmd("INF:ABORT", frame, 50, &cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
#endif
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 10;
	res = sem_timedwait(&m_semOpCmdINF, &tm);
	if (res == -1) {
		m_OpCmdState = IDLE;
		return 8;//wait INF timed out
	}
	if (m_OpCmdRes != RES_INF && m_OpCmdRes != RES_ABS) {
		m_OpCmdState = IDLE;
		return 9;//non ABS & non INF result
	}
	res = prepCmd("FIN:ABORT", frame, 50, &cmdlen);
	//((CLP*)m_parent)->m_log->WriteLog("To TAS300", frame, cmdlen);
	//m_pSerial->SendBlock(frame, cmdlen);
	m_OpCmdState = IDLE;
	if (m_OpCmdRes == RES_ABS) {
		((CLP*)m_parent)->m_log->WriteLog("Parm",
			m_OpCmdResParam, strlen(m_OpCmdResParam));
		return 10;//ABS result
	}
	return 0;
}

//////////////////////////////////
//Assistant Thread for CTas300
/*
void *PodEvtThreadTas300(void *Arg){
  ThreadArgType arg;
  arg.pObj = ((ThreadArgType *)Arg)->pObj;
  arg.prm1 = ((ThreadArgType *)Arg)->prm1;
  CLP *pParent = (CLP *)arg.pObj;
  pParent->TasPodEvt(arg.prm1);
}
*/
//End Assistant Thread for CTas300

//////////////////////////////////
//End Tas300 Class Implementation
//////////////////////////////////

//////////////////////////////////
//BL600 Class Implementation
CBL600::CBL600(int lpID, void *parent) {
	m_lpID = lpID;
	m_parent = parent;
	m_waitingRes = false;
	((CLP*)m_parent)->m_log->WriteLog("CBL600 Constructor called");
	m_pSerial = new CSerial(m_lpID, "BL6"/*log file name base*/);
	m_pSerial->Initialize(BL600, this, CallbackWrap);
}
CBL600::~CBL600() {
	((CLP*)m_parent)->m_log->WriteLog("CBL600 Destructor called");
	if (m_pSerial != NULL)
		delete m_pSerial;
}

int CBL600::OpenPort() {
	int res = m_pSerial->OpenConnection();
	if (res != 0)
		return res;
	res = MotorOFF();
	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("Initial MotorOFF Failed");
	else
		((CLP*)m_parent)->m_log->WriteLog("Initial MotorOFF Succeeded");
	/*
	res = MotorON();
	if(res != 0)
	  ((CLP*)m_parent)->m_log->WriteLog("Initial MotorON Failed");
	else
	  ((CLP*)m_parent)->m_log->WriteLog("Initial MotorON Succeeded");
	  */
	return 0;
}
int CBL600::ClosePort() {
	return m_pSerial->CloseConnection();
}

void CBL600::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	CBL600 *pSelf = (CBL600*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

void CBL600::Callback(char *pMsg, int len) {
	char *pM = new char[len];
	for (int l = 0; l < len; l++) {
		pM[l] = pMsg[l];
	}
	//Do processing of message bellow 
	((CLP*)m_parent)->m_log->WriteLog("Fr BL600", pM, len);
	//////////////////////////////////////////////////////////////////
	char resStr[50];
	int rdlen;
	int res = ReadResStr(resStr, 50, &rdlen, pM, len);
	if (res != 0) {
		char str[50];
		sprintf(str, "Read BL600 ResStr Error=%d", res);
		((CLP*)m_parent)->m_log->WriteLog(str);
		goto ex;
	}
	//((CLP*)m_parent)->m_log->WriteLog(resStr);
	//response OK
	if (strcmp("OK", resStr) == 0 && m_waitingRes == false) {
		sem_post(&m_semCmdACK);
		((CLP*)m_parent)->m_log->WriteLog("sem_post(&m_semCmdACK);");
		goto ex;
	}
	//end response OK
	//////////////////////////////////////////////////////////////////
	else if (m_waitingRes) {
		for (int i = 0; i < len - 1; i++)
			m_bcode[i] = pM[i];//don't read the 0x0D
		m_bcodelen = len - 1;
		if (len > 3)
		{
			m_readBcodeFinish = true;
		}
		sem_post(&m_semEvtRead);
		goto ex;
	}
	//response...
	//unknown response
	//((CLP*)m_parent)->m_log->WriteLog("Unknow response");

ex:  delete[] pM;
}

int CBL600::ReadResStr(char *a, int alen, int *aclen,
	char *b, int blen) {
	//read an the response result ended by 0x0D in b
	//store the result in a and actual length in aclen(>0)
	//if blen > alen, return nonzero meaning failed
	//if aclen == 0, return nonzero meaning failed
	if (alen < blen + 1) {
		*aclen = 0;
		return 1;//alen not enough
	}
	for (int i = 0; i < blen; i++)
		a[i] = b[i];
	if (a[blen - 1] != 0x0D) {
		*aclen = blen;
		a[blen] = '\0';
		return 2;//result not ended by 0x0D
	}
	*aclen = blen - 1;
	a[blen - 1] = '\0';
	if (*aclen == 0)
		return 3;//actual read len is 0
	return 0;
}

int CBL600::sem_reset(sem_t *sem)
{
	int res;
	while ((res = sem_trywait(sem)) == 0);
	//while((res = sem_trywait(&m_semInfCmdACK)) == 0)
	//  ((CLP*)m_parent)->m_log->WriteLog("sem_trywait succeeded!");
	if (res != -1)
		return 1;//internal error
	if (errno == EAGAIN) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EAGAIN");
		return 0;
	}
	if (errno == EDEADLK) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EDEADLK");
		return 2;//deadlock detected
	}
	if (errno == EINVAL) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINVAL");
		return 3;//invalid semaphore descriptor sem
	}
	if (errno == EINTR) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINTR");
		return 4;//A signal interrupted sem_trywait function
	}
	return 5;//unknown errno
}

int CBL600::MotorON() {
	char s[100];
	int res = sem_reset(&m_semCmdACK);
	if (res != 0)
		return 1;//Error happened during sem_reset
#ifdef _OfflineTest
	sprintf(s, "OK");
	s[2] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 3);
	m_pSerial->SendBlock(s, 3);
#else
	sprintf(s, "MOTORON");
	s[7] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 8);
	m_pSerial->SendBlock(s, 8);
#endif
	/////////////////////////////////////////////////////
	//wait for 10 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 10;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1)
		return 2;//wait timed out
	return 0;
}

int CBL600::MotorOFF() {
	char s[100];
	int res = sem_reset(&m_semCmdACK);
	if (res != 0)
		return 1;//Error happened during sem_reset
#ifdef _OfflineTest
	sprintf(s, "OK");
	s[2] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 3);
	m_pSerial->SendBlock(s, 3);
#else
	sprintf(s, "MOTOROFF");
	s[8] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 9);
	m_pSerial->SendBlock(s, 9);
#endif
	/////////////////////////////////////////////////////
	//wait for 10 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;//changed from 10 to 3
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1)
		return 2;//wait timed out
	return 0;
}

int CBL600::Lock() {
	char s[100];
	int res = sem_reset(&m_semCmdACK);
	if (res != 0)
		return 1;//Error happened during sem_reset
#ifdef _OfflineTest
	sprintf(s, "OK");
	s[2] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 3);
	m_pSerial->SendBlock(s, 3);
#else
	sprintf(s, "LOCK");
	s[4] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 5);
	m_pSerial->SendBlock(s, 5);
#endif
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from breader
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1)
		return 2;//wait timed out
	return 0;
}

int CBL600::Unlock() {
	char s[100];
	int res = sem_reset(&m_semCmdACK);
	if (res != 0)
		return 1;//Error happened during sem_reset
#ifdef _OfflineTest
	sprintf(s, "OK");
	s[2] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 3);
	m_pSerial->SendBlock(s, 3);
#else
	sprintf(s, "UNLOCK");
	s[6] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 7);
	m_pSerial->SendBlock(s, 7);
#endif
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from breader
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1)
		return 2;//wait timed out
	return 0;
}
int CBL600::ReadBarCode(char *barcode, int len, int *aclen) {
#ifdef debug_log
	((CLP*)m_parent)->m_log->WriteLog("ReadBarCode");
#endif
	char s[100];
	int res = sem_reset(&m_semEvtRead);
	if (res != 0)
		return 1;//Error happened during sem_reset
	m_waitingRes = true;
#ifdef _OfflineTest
	sprintf(s, "W000S12345");
	s[10] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 11);
	m_pSerial->SendBlock(s, 11);
#else
	sprintf(s, "LON");
	s[3] = 0x0D;
	((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 4);
	m_pSerial->SendBlock(s, 4);
#endif
	/////////////////////////////////////////////////////
	//wait for 5 seconds for result to return from Tas300
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 5;
#ifdef debug_log
	((CLP*)m_parent)->m_log->WriteLog("tm.tv_sec += 5");
#endif
	res = sem_timedwait(&m_semEvtRead, &tm);
	if (res == -1) {
		((CLP*)m_parent)->m_log->WriteLog("res == -1");
		m_waitingRes = false;
		sprintf(s, "LOFF");
		s[4] = 0x0D;
		((CLP*)m_parent)->m_log->WriteLog("To BL600", s, 5);
		m_pSerial->SendBlock(s, 5);
		return 2;//wait timed out
	}
	if (len < m_bcodelen) {
#ifdef debug_log
		((CLP*)m_parent)->m_log->WriteLog("//input array too short");
#endif
		m_waitingRes = false;
		return 3;//input array too short
	}
	for (int i = 0; i < m_bcodelen; i++) {
		barcode[i] = m_bcode[i];
	}
	*aclen = m_bcodelen;
	((CLP*)m_parent)->m_log->WriteLog("Barcode Read", barcode, m_bcodelen);
	m_waitingRes = false;
	return 0;
}

void CBL600::DoTest() {
L:
	int res = MotorON();
	if (res == 1)
		((CLP*)m_parent)->m_log->WriteLog("Error happened during sem reset");
	else if (res == 2)
		((CLP*)m_parent)->m_log->WriteLog("Wait for OK time out");
	/*
	res = Unlock();
	if(res == 1)
	  ((CLP*)m_parent)->m_log->WriteLog("Error happened during sem reset");
	else if(res == 2)
	  ((CLP*)m_parent)->m_log->WriteLog("Wait for OK time out");
	*/
	int count = 0;
	char bcode[50];
	int len = 0;
	do {
		sleep(2);//2 seconds is enough for BL601, but BL601HAC1 need 12 seconds
		res = ReadBarCode(bcode, 50, &len);
		if (res == 1)
			((CLP*)m_parent)->m_log->WriteLog("Error happened during sem reset");
		else if (res == 2)
			((CLP*)m_parent)->m_log->WriteLog("Wait for result time out");
		else if (res == 3)
			((CLP*)m_parent)->m_log->WriteLog("Input array too short");
		count++;
		bcode[len] = '\0';
		printf("  Got code : %s with len = %d\n", bcode, len);
	} while (count < 7 && res == 0 && len == 2 && bcode[0] == 'N' && bcode[1] == 'G');

	if (res == 0) {
		bcode[len] = '\0';
		printf("Got Barcode : %s\n", bcode);
	}
	else if (res == 1)
		printf("Error happened during sem reset\n");
	else if (res == 2)
		printf("Wait for result time out\n");
	else if (res == 3)
		printf("Input array too short\n");


	res = MotorOFF();
	if (res == 1)
		((CLP*)m_parent)->m_log->WriteLog("Error happened during sem reset");
	else if (res == 2)
		((CLP*)m_parent)->m_log->WriteLog("Wait for OK time out");

	char ch;
	read(0, &ch, 1);
	if (ch == 'c' || ch == '\n')goto L;
	/*
	char tas[100];
  L:
	sprintf(tas, "OKON"); tas[4] = 0x0D;
	m_pSerial->SendBlock(tas, 5);
	sleep(3);
	sprintf(tas, "NGON"); tas[4] = 0x0D;
	m_pSerial->SendBlock(tas, 5);
	sleep(3);
	sprintf(tas, "ALLOFF"); tas[6] = 0x0D;
	m_pSerial->SendBlock(tas, 7);
	sleep(1);
	sprintf(tas, "MOTORON"); tas[7] = 0x0D;
	m_pSerial->SendBlock(tas, 8);
	sleep(6);
	sprintf(tas, "LON"); tas[3] = 0x0D;
	m_pSerial->SendBlock(tas, 4);
	sleep(5);
	sprintf(tas, "LOFF"); tas[4] = 0x0D;
	m_pSerial->SendBlock(tas, 5);
	sleep(1);
	sprintf(tas, "MOTOROFF"); tas[8] = 0x0D;
	m_pSerial->SendBlock(tas, 9);

	char ch;
	read(0, &ch, 1);
	if(ch == 'c'|| ch == '\n')goto L;
	*/
}
//End BL600 Class Implementation
//////////////////////////////////

//////////////////////////////////
//Hermos Class Implementation
CHermos::CHermos(int lpID, void *parent) {
	m_lpID = lpID;
	m_parent = parent;
	m_CmdState = IDLE;
	m_CurCmd = CMD_NULL;
	((CLP*)m_parent)->m_log->WriteLog("CHermos Constructor called");
	m_pSerial = new CSerial(m_lpID, "Hmos"/*log file name base*/);
	m_pSerial->Initialize(HERMOS, this, CallbackWrap);
	int res = sem_init(&m_semCmdACK, 0, 0);

	if (res != 0)
		((CLP*)m_parent)->m_log->WriteLog("m_semCmdACK initialization failed");

	((CLP*)m_parent)->m_log->WriteLog("CHermos Constructor called");

}
CHermos::~CHermos() {
	((CLP*)m_parent)->m_log->WriteLog("CHermos Destructor called");
	if (m_pSerial != NULL)
		delete m_pSerial;
}
int CHermos::OpenPort() {
	int res = m_pSerial->OpenConnection();
	if (res != 0)
		return res;
	//read version
	char ver[100];
	int aclen;

	res = AskVersion(ver, 50, &aclen);
	if (res == 0) {
		int i;
		for (i = 0; i < aclen / 2; i++) {
			char c1 = ver[2 * i];
			char c2 = ver[2 * i + 1];
			char tmp;
			tmp = c1;
			if ('0' <= tmp && tmp <= '9')
				tmp -= '0';
			else if ('A' <= tmp && tmp <= 'F')
				tmp = tmp - 'A' + 10;
			else if ('a' <= tmp && tmp <= 'f')
				tmp = tmp - 'a' + 10;
			c1 = tmp;
			tmp = c2;
			if ('0' <= tmp && tmp <= '9')
				tmp -= '0';
			else if ('A' <= tmp && tmp <= 'F')
				tmp = tmp - 'A' + 10;
			else if ('a' <= tmp && tmp <= 'f')
				tmp = tmp - 'a' + 10;
			c2 = tmp;

			ver[i] = c1;
			ver[i] <<= 4;
			ver[i] += c2;
		}
		ver[i] = '\0';
		((CLP*)m_parent)->m_log->WriteLog("Version", ver, strlen(ver));
	}
	else {
		sprintf(ver, "Return code = %d", res);
		((CLP*)m_parent)->m_log->WriteLog(ver);
		((CLP*)m_parent)->m_log->WriteLog("Ask Hermos Reader Version Failed");
	}
	return 0;
}
int CHermos::ClosePort() {
	return m_pSerial->CloseConnection();
}
void CHermos::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	CHermos *pSelf = (CHermos*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

void CHermos::Callback(char *pMsg, int len) {
	char *pM = new char[len];
	for (int l = 0; l < len; l++) {
		pM[l] = pMsg[l];
	}
	//Do processing of message bellow 
	((CLP*)m_parent)->m_log->WriteLog("Fr Hermos", pM, len);
	//////////////////////////////////////////////////////////////////
	//Analysing message
	int checksum = 0;
	char cs, csh, csl;
	char command;
	char address;
	char info[MAXINFOLEN];
	int infolen;
	//length
	if (len < 8) {
		((CLP*)m_parent)->m_log->WriteLog("Message too short");
		goto ex;
	}
	//Addition checksum
	for (int i = 0; i < len - 4; i++) checksum += pM[i];
	cs = checksum;
	csh = (cs >> 4) & 0x0F;
	csl = cs & 0x0F;
	if (csh < 0x0A) csh = '0' + csh;
	else csh = 'A' + csh - 10;
	if (csl < 0x0A) csl = '0' + csl;
	else csl = 'A' + csl - 10;

	if (pM[len - 1] != csl || pM[len - 2] != csh) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong addition checksum");
		goto ex;
	}
	//XOR checksum
	cs = 0;
	for (int i = 0; i < len - 4; i++) cs ^= pM[i];
	csh = (cs >> 4) & 0x0F;
	csl = cs & 0x0F;
	if (csh < 0x0A) csh = '0' + csh;
	else csh = 'A' + csh - 10;
	if (csl < 0x0A) csl = '0' + csl;
	else csl = 'A' + csl - 10;

	if (pM[len - 3] != csl || pM[len - 4] != csh) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong xor checksum");
		goto ex;
	}
	//Start
	if (pM[0] != 'S') {
		((CLP*)m_parent)->m_log->WriteLog("Wrong start");
		goto ex;
	}
	//End
	if (pM[len - 5] != 0x0D) {
		((CLP*)m_parent)->m_log->WriteLog("Wrong end");
		goto ex;
	}

	//Command
	command = pM[3];
	//Address
	address = pM[4];
	if (address != '0') {
		((CLP*)m_parent)->m_log->WriteLog("Source address is not 0!");
	}
	//Info length
	infolen = len - 10;
	if (infolen > 0) {
		if (infolen > MAXINFOLEN - 10) {
			((CLP*)m_parent)->m_log->WriteLog("Information too long");
			goto ex;
		}
		for (int i = 0; i < infolen; i++)info[i] = pM[i + 5];
	}
	////////////////////////////////
	//Command processing
	if (command == 'x') {
		if (m_CmdState == WAITACK) {
			m_CmdRes = ACKED_OK;
			if (infolen > 2) {//actually should be 18
				int i = 2;
				m_CmdResInfo[0] = ' ';
				m_CmdResInfo[1] = info[0];
				m_CmdResInfo[2] = info[1];
				m_CmdResInfo[3] = ' ';
				m_CmdResInfo[4] = '\'';
				for (i = 2; i < infolen; i++) {
					m_CmdResInfo[i + 3] = info[i];
				}
				m_CmdResInfo[i + 3] = '\'';
				m_CmdResInfo[i + 4] = '\0';
				m_CmdResInfoLen = infolen + 4;//not including the null terminator
			}
			else
				m_CmdResInfoLen = 0;
			sem_post(&m_semCmdACK);
		}
	}
	else if (command == 'w') {
		if (m_CmdState == WAITACK) {
			m_CmdRes = ACKED_OK;
			sem_post(&m_semCmdACK);
		}
	}
	else if (command == 'n') {
		((CLP*)m_parent)->m_log->WriteLog("Hardware reset event!");
	}
	else if (command == 'v') {
		if (m_CmdState == WAITACK) {
			m_CmdRes = ACKED_OK;
			int i;
			for (i = 0; i < infolen; i++) {
				m_CmdResInfo[i] = info[i];
			}
			m_CmdResInfo[i] = '\0';
			m_CmdResInfoLen = infolen;//not including the null terminator
			sem_post(&m_semCmdACK);
		}
	}
	else if (command == 'e') {
		if (m_CmdState == WAITACK) {
			m_CmdRes = ACKED_ERR;
			m_failCode[0] = info[0];
			m_failCode[1] = '\0';
			sem_post(&m_semCmdACK);
		}
	}
	else {
		((CLP*)m_parent)->m_log->WriteLog("Unknown command from reader!");
		goto ex;
	}

ex:  delete[] pM;
}

int CHermos::ReadRFID(int page, char *rfid, int len, int *aclen) {
	//if((page < 1 || page > 17) && page != 98 && page != 99)
	//  return 1;//illegal page number
	if (page < 1 || page > 17)
		return 1;//illegal single page number
	if (m_CmdState != IDLE)
		return 2;//busy

	char msg[5];
	char frame[50];
	int cmdlen;

	msg[0] = 'X';
	msg[1] = '0';
	msg[2] = '0' + page / 10;
	msg[3] = '0' + page % 10;
	msg[4] = '\0';

	int res = prepCmd(msg, frame, 50, &cmdlen);
	if (res != 0) {
		m_CmdState = IDLE;
		return 3;//internal error, won't happen;
	}
	m_CurCmd = RD_1PAGE;
	m_CmdState = WAITACK;
	res = sem_reset(&m_semCmdACK);
	if (res != 0) {
		m_CmdState = IDLE;
		return 4;//Error happened during m_semCmdACK sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Hermos", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Hermos
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1) {
		m_CmdState = IDLE;
		return 5;//wait timed out
	}
	if (m_CmdRes == ACKED_ERR) {
		m_CmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Failure Code",
			m_failCode, strlen(m_failCode));
		return 6;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == ACKED_OK
	((CLP*)m_parent)->m_log->WriteLog("Msg Info", m_CmdResInfo, m_CmdResInfoLen);
	if (m_CmdResInfoLen >= len) {
		((CLP*)m_parent)->m_log->WriteLog("Msg too long!");
		m_CmdState = IDLE;
		return 7;
	}
	strcpy(rfid, m_CmdResInfo);
	*aclen = m_CmdResInfoLen;
	//end process result
	///////////////////////////////////////
	m_CmdState = IDLE;
	return 0;
}

int CHermos::ReadMULTIPAGE(int page, char *content, int len, int *aclen) {
	//if((page < 1 || page > 17) && page != 98 && page != 99)
	//  return 1;//illegal page number
	if (page != 98 && page != 99)
		return 1;//illegal multi-page number
	if (m_CmdState != IDLE)
		return 2;//busy

	char msg[5];
	char frame[50];
	int cmdlen;

	msg[0] = 'X';
	msg[1] = '0';
	msg[2] = '0' + page / 10;
	msg[3] = '0' + page % 10;
	msg[4] = '\0';

	int res = prepCmd(msg, frame, 50, &cmdlen);
	if (res != 0) {
		m_CmdState = IDLE;
		return 3;//internal error, won't happen;
	}
	m_CurCmd = RD_MPAGE;
	m_CmdState = WAITACK;
	res = sem_reset(&m_semCmdACK);
	if (res != 0) {
		m_CmdState = IDLE;
		return 4;//Error happened during m_semCmdACK sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Hermos", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Hermos
	struct timespec tm;
	*aclen = 0;
loop:
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1) {
		m_CmdState = IDLE;
		return 5;//wait timed out
	}
	res = sem_reset(&m_semCmdACK);
	if (res != 0) {
		m_CmdState = IDLE;
		return 4;//Error happened during m_semCmdACK sem_reset
	}
	if (m_CmdRes == ACKED_ERR) {
		m_CmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Failure Code",
			m_failCode, strlen(m_failCode));
		return 6;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == ACKED_OK
	if (m_CmdResInfoLen != 0) {
		((CLP*)m_parent)->m_log->WriteLog("Msg Info", m_CmdResInfo, m_CmdResInfoLen);
		if ((*aclen + m_CmdResInfoLen) >= len) {
			((CLP*)m_parent)->m_log->WriteLog("Msg too long!");
			m_CmdState = IDLE;
			return 7;
		}
		strcpy(content + *aclen, m_CmdResInfo);
		*aclen += m_CmdResInfoLen;
		goto loop;
	}
	//end process result
	///////////////////////////////////////
	m_CmdState = IDLE;
	return 0;
}

int CHermos::WriteRFID(int page, const char *rfid, int len) {
	//if(page < 2 || page > 17)
	if (page < 1 || page > 17)
		return 1;//illegal single page number
	if (m_CmdState != IDLE)
		return 2;//busy

	if (len != 16)
		return 3;//invalid parameter

	char msg[21];
	char frame[50];
	int cmdlen;

	msg[0] = 'W';
	msg[1] = '0';
	msg[2] = '0' + page / 10;
	msg[3] = '0' + page % 10;
	for (int i = 0; i < len; i++)
		msg[i + 4] = rfid[i];
	msg[20] = '\0';

	int res = prepCmd(msg, frame, 50, &cmdlen);
	if (res != 0) {
		m_CmdState = IDLE;
		return 4;//internal error, won't happen;
	}
	m_CurCmd = WR_TAG;
	m_CmdState = WAITACK;
	res = sem_reset(&m_semCmdACK);
	if (res != 0) {
		m_CmdState = IDLE;
		return 5;//Error happened during m_semCmdACK sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Hermos", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Hermos
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdACK, &tm);
	if (res == -1) {
		m_CmdState = IDLE;
		return 6;//wait timed out
	}
	if (m_CmdRes == ACKED_ERR) {
		m_CmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Failure Code",
			m_failCode, strlen(m_failCode));
		return 7;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == ACKED_OK
	//nothing need to be done
	//end process result
	///////////////////////////////////////
	m_CmdState = IDLE;
	return 0;
}
int CHermos::AskVersion(char *version, int len, int *aclen) {
	if (m_CmdState != IDLE)
		return 1;//busy

	char msg[5];
	char frame[50];
	int cmdlen;

	msg[0] = 'V';
	msg[1] = '0';
	msg[2] = '\0';

	int res = prepCmd(msg, frame, 50, &cmdlen);
	if (res != 0) {
		m_CmdState = IDLE;
		return 2;//internal error, won't happen;
	}
	m_CurCmd = VS_ASK;
	m_CmdState = WAITACK;
	res = sem_reset(&m_semCmdACK);
	if (res != 0) {
		m_CmdState = IDLE;
		return 3;//Error happened during m_semCmdACK sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Hermos", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Hermos
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;

	try
	{
		res = sem_timedwait(&m_semCmdACK, &tm);
	}
	catch (...)
	{

	}

	if (res == -1) {
		m_CmdState = IDLE;
		return 4;//wait timed out
	}
	if (m_CmdRes == ACKED_ERR) {
		m_CmdState = IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Failure Code",
			m_failCode, strlen(m_failCode));
		return 5;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == ACKED_OK
	((CLP*)m_parent)->m_log->WriteLog("Msg Info", m_CmdResInfo, m_CmdResInfoLen);
	if (m_CmdResInfoLen >= len) {
		((CLP*)m_parent)->m_log->WriteLog("Msg too long!");
		m_CmdState = IDLE;
		return 6;
	}
	strcpy(version, m_CmdResInfo);
	*aclen = m_CmdResInfoLen;
	//end process result
	///////////////////////////////////////
	m_CmdState = IDLE;
	return 0;
}
int CHermos::prepCmd(const char *msg, char *frame, int fmaxlen, int *factulen) {
	//parameter msg(null terminated string),
	//example, "X002"
	int checksum = 0;
	int len = strlen(msg);
	*factulen = len + 8;//total len;
	if (*factulen > fmaxlen)
		return 1;//frame too short to hold command
	frame[0] = 'S';//start
	frame[1] = (len >> 4) & 0x0F;//message length highbyte
	if (0 <= frame[1] && frame[1] <= 9)
		frame[1] += '0';
	else if (10 <= frame[1] && frame[1] <= 15)
		frame[1] += 'A' - 10;
	else
		return 2;//wrong length symbol: not '0'~'F'
	frame[2] = len & 0x0F;//message length lowbyte
	if (0 <= frame[2] && frame[2] <= 9)
		frame[2] += '0';
	else if (10 <= frame[2] && frame[2] <= 15)
		frame[2] += 'A' - 10;
	else
		return 2;//wrong length symbol: not '0'~'F'
	int icur = 3;
	for (int i = 0; i < len; i++) {
		frame[icur] = msg[i];
		icur++;
	}
	frame[icur] = 0x0D;
	icur++;
	//now make check sum until the end sign 0x0D(inclusive)
	int iend = icur;
	for (int i = 0; i < iend; i++) checksum ^= frame[i];
	char cs = checksum;
	char csh = (cs >> 4) & 0x0F;
	char csl = cs & 0x0F;
	if (csh < 0x0A) frame[iend] = '0' + csh;
	else frame[iend] = 'A' + csh - 10;
	if (csl < 0x0A) frame[iend + 1] = '0' + csl;
	else frame[iend + 1] = 'A' + csl - 10;//CSh and CSl

	checksum = 0;
	for (int i = 0; i < iend; i++) checksum += frame[i];
	cs = checksum;
	csh = (cs >> 4) & 0x0F;
	csl = cs & 0x0F;
	if (csh < 0x0A) frame[iend + 2] = '0' + csh;
	else frame[iend + 2] = 'A' + csh - 10;
	if (csl < 0x0A) frame[iend + 3] = '0' + csl;
	else frame[iend + 3] = 'A' + csl - 10;//CSh and CSl
	return 0;
}

void CHermos::DoTest() {
	char ch = 'c';
	char rfid[1000];
	int aclen;
	int res;
L1:
	res = AskVersion(rfid, 50, &aclen);
	if (res == 0) {
		printf(rfid); printf("\n");
	}
	else {
		printf("Return Code = %d\n", res);
		if (res == 5)//error result
			printf("AskV Fail Code = %s\n ", m_failCode);
	}

	res = WriteRFID(17, "0123456789ABCDEF", 16);
	if (res != 0) {
		printf("Return Code = %d\n", res);
		if (res == 7)//error result
			printf("Write Fail Code = %s\n ", m_failCode);
	}
	res = ReadRFID(17, rfid, 50, &aclen);
	if (res == 0) {
		printf(rfid); printf("\n");
	}
	else {
		printf("Return Code = %d\n", res);
		if (res == 6)//error result
			printf("Read Fail Code = %s\n ", m_failCode);
	}

	res = ReadMULTIPAGE(99, rfid, 500, &aclen);
	if (res == 0)
	{
		printf(rfid); printf("\n");
	}
	else {
		printf("Return Code = %d\n", res);
		if (res == 7)//error result
			printf("Read Fail Code = %s\n ", m_failCode);
	}

L2:
	read(0, &ch, 1);
	if (ch == 'c')goto L1;
	else if (ch == '\n')goto L2;
}


/*void CHermos::DoTest(){
  char ch = 'c';
  char tas[100];
  int cmdlen;
  int count = 0;
  char msg[10];
  msg[0] = 'X'; msg[1] = '0'; msg[2] = '0'; msg[4] = 0;
L1:
  count++;
  if(count == 10)
	count = 1;
  msg[3] = count + '0';
  if(prepCmd(msg, tas, 100, &cmdlen) != 0)
	printf("prepCmd failed\n");
  else
	printf("prepCmd succeeded\n");

  tas[cmdlen] = 0;
  ((CLP*)m_parent)->m_log->WriteLog(tas);
  m_pSerial->SendBlock(tas, cmdlen);

L2:
  read(0, &ch, 1);
  if(ch == 'c')goto L1;
  else if(ch == '\n')goto L2;

  ///////////////////////////////
  //End Action
  if(prepCmd("V0", tas, 100, &cmdlen) != 0)
	printf("prepCmd failed\n");
  else
	printf("prepCmd succeeded\n");

  tas[cmdlen] = 0;
  ((CLP*)m_parent)->m_log->WriteLog(tas);
  m_pSerial->SendBlock(tas, cmdlen);
}
*/
int CHermos::sem_reset(sem_t *sem)
{
	int res;
	while ((res = sem_trywait(sem)) == 0);
	//while((res = sem_trywait(&m_semInfCmdACK)) == 0)
	//  ((CLP*)m_parent)->m_log->WriteLog("sem_trywait succeeded!");
	if (res != -1)
		return 1;//internal error
	if (errno == EAGAIN) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EAGAIN");
		return 0;
	}
	if (errno == EDEADLK) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EDEADLK");
		return 2;//deadlock detected
	}
	if (errno == EINVAL) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINVAL");
		return 3;//invalid semaphore descriptor sem
	}
	if (errno == EINTR) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINTR");
		return 4;//A signal interrupted sem_trywait function
	}
	return 5;//unknown errno
}
//End Hermos Class Implementation
//////////////////////////////////

//////////////////////////////////
//COmron Class Implementation
COmron::COmron(int lpID, int fmt, void *parent) {
	m_lpID = lpID;
	m_cntntFMT = fmt;//content format is used at run time
	m_parent = parent;
	m_CmdState = OM_IDLE;
	m_CurCmd = OM_NUL;
	m_CompleteCode[2] = '\0';
	((CLP*)m_parent)->m_log->WriteLog("COmron Constructor called");
	m_pSerial = new CSerial(m_lpID, "Omron"/*log file name base*/);
	m_pSerial->Initialize(OMRON, this, CallbackWrap);
}

COmron::~COmron() {
	((CLP*)m_parent)->m_log->WriteLog("COmron Destructor called");
	if (m_pSerial != NULL)
		delete m_pSerial;
}

int COmron::OpenPort() {
	int res = m_pSerial->OpenConnection();
	if (res != 0)
		return res;
	return 0;
}

int COmron::ClosePort() {
	return m_pSerial->CloseConnection();
}

void COmron::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	COmron *pSelf = (COmron*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

void COmron::Callback(char *pMsg, int len) {
	char *pM = new char[len];
	for (int l = 0; l < len; l++) {
		pM[l] = pMsg[l];
	}
	//Do processing of message bellow 
	((CLP*)m_parent)->m_log->WriteLog("Fr Omron", pM, len);
	//////////////////////////////////////////////////////////////////
	if (len < 3 || pM[len - 1] != 0x0D) {
		char str[50];
		sprintf(str, "Response not in standard frame.");
		((CLP*)m_parent)->m_log->WriteLog(str);
		goto ex;
	}

	if (m_CmdState == OM_IDLE) {// OM_WAITRES
		char str[50];
		sprintf(str, "Unsolicited message!");
		((CLP*)m_parent)->m_log->WriteLog(str);
		goto ex;
	}
	//else if(m_CmdState == OM_WAITRES)
	m_CompleteCode[0] = pM[0];
	m_CompleteCode[1] = pM[1];
	m_CompleteCode[2] = '\0';
	m_CmdResParamLen = 0;
	for (int i = 2; i < len - 1; i++) {
		m_CmdResParam[m_CmdResParamLen] = pM[i];
		m_CmdResParamLen++;
	}
	m_CmdResParam[m_CmdResParamLen] = '\0';

	if (m_CompleteCode[0] == '0' && m_CompleteCode[1] == '0')
		m_CmdRes = OM_RESOK;
	else
		m_CmdRes = OM_RESERR;

	sem_post(&m_semCmdRes);

ex:  delete[] pM;
}

int COmron::sem_reset(sem_t *sem)
{
	int res;
	while ((res = sem_trywait(sem)) == 0);
	//while((res = sem_trywait(&m_semInfCmdACK)) == 0)
	//  ((CLP*)m_parent)->m_log->WriteLog("sem_trywait succeeded!");
	if (res != -1)
		return 1;//internal error
	if (errno == EAGAIN) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EAGAIN");
		return 0;
	}
	if (errno == EDEADLK) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EDEADLK");
		return 2;//deadlock detected
	}
	if (errno == EINVAL) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINVAL");
		return 3;//invalid semaphore descriptor sem
	}
	if (errno == EINTR) {
		//((CLP*)m_parent)->m_log->WriteLog("errno = EINTR");
		return 4;//A signal interrupted sem_trywait function
	}
	return 5;//unknown errno
}

int COmron::ReadRFID(int page, char *rfid, int len, int *aclen) {
	if (page < 1 || page > 30)
		return 1;//illegal single page number
	if (m_CmdState != OM_IDLE)
		return 2;//busy

	m_CmdState = OM_WAITRES;
	m_CurCmd = OM_RD;

	char frame[13];
	int cmdlen = 13;
	//Command type
	frame[0] = '0';
	frame[1] = '1';//Read command
	//Content format
	if (m_cntntFMT == OM_ASCII)
		frame[2] = '1';//ASCII format
	else//OM_HEX
		frame[2] = '0';//HEX format
	//Transmission operation
	frame[3] = '0';//Single Trigger
	//Page specification
	frame[4] = '0';//p30~27
	frame[5] = '0';//p26~23
	frame[6] = '0';//p22~19
	frame[7] = '0';//p18~15

	frame[8] = '0';//p14~11
	frame[9] = '0';//p10~7
	frame[10] = '0';//p6~3
	frame[11] = '0';//p2~1
	int offset = 0;
	switch (page) {
	case 1:
		if (m_cntntFMT == OM_ASCII)//p2~1
		{
			frame[11] = 'C';//ASCII format, read P1 and P2
		}
		else//OM_HEX
		{
			frame[11] = '4';//HEX format, read P1 only
		}
		break;
	case 2:
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P2 and P3
		{
			frame[10] = '1';//p6~3
			frame[11] = '8';//p2~1
		}
		else//OM_HEX: HEX format, read P2 only
		{
			frame[10] = '0';//p6~3
			frame[11] = '8';//p2~1
		}
		break;
	case 3:
	case 7:
	case 11:
	case 15:
	case 19:
	case 23:
	case 27:
		offset = page / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P3 and P4
		{
			frame[10 - offset] = '3';//p6~3
		}
		else//OM_HEX: HEX format, read P3 only
		{
			frame[10 - offset] = '1';//p6~3
		}
		break;
	case 4:
	case 8:
	case 12:
	case 16:
	case 20:
	case 24:
	case 28:
		offset = (page - 1) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P4 and P5
		{
			frame[10 - offset] = '6';//p6~3
		}
		else//OM_HEX: HEX format, read P4 only
		{
			frame[10 - offset] = '2';//p6~3
		}
		break;
	case 5:
	case 9:
	case 13:
	case 17:
	case 21:
	case 25:
	case 29:
		offset = (page - 2) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P5 and P6
		{
			frame[10 - offset] = 'C';//p6~3
		}
		else//OM_HEX: HEX format, read P5 only
		{
			frame[10 - offset] = '4';//p6~3
		}
		break;
	case 6:
	case 10:
	case 14:
	case 18:
	case 22:
	case 26:
	case 30:
		offset = (page - 3) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P6 and P7
		{
			if (page < 30)//can not read non-existent P31
				frame[9 - offset] = '1';//p10~7
			frame[10 - offset] = '8';//p6~3
		}
		else//OM_HEX: HEX format, read P6 only
		{
			frame[10 - offset] = '8';//p6~3
		}
		break;
	default:
		if (m_cntntFMT == OM_ASCII)//p2~1
			frame[11] = 'C';//ASCII format, read P1 and P2
		else//OM_HEX
			frame[11] = '4';//HEX format, read P1 only
	}
	//Terminator
	frame[12] = 0x0D;

	int res = sem_reset(&m_semCmdRes);
	if (res != 0) {
		m_CmdState = OM_IDLE;
		return 4;//Error happened during m_semCmdRes sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Omron", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Omron
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdRes, &tm);
	if (res == -1) {
		m_CmdState = OM_IDLE;
		return 5;//wait timed out
	}
	if (m_CmdRes == OM_RESERR) {
		m_CmdState = OM_IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Completion Error Code",
			m_CompleteCode, strlen(m_CompleteCode));
		return 6;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == OM_RESOK
	((CLP*)m_parent)->m_log->WriteLog("Read Data", m_CmdResParam, m_CmdResParamLen);
	if (m_CmdResParamLen >= len) {
		((CLP*)m_parent)->m_log->WriteLog("Read Data too long!");
		m_CmdState = OM_IDLE;
		return 7;
	}
	strcpy(rfid, m_CmdResParam);
	*aclen = m_CmdResParamLen;
	//end process result
	///////////////////////////////////////
	m_CmdState = OM_IDLE;
	return 0;
}

int COmron::WriteRFID(int page, char *rfid, int len) {
	if (page < 1 || page > 17)
		return 1;//illegal single page number
	if (m_CmdState != OM_IDLE)
		return 2;//busy

	if (len != 16)
		return 3;//invalid parameter

	m_CmdState = OM_WAITRES;
	m_CurCmd = OM_RD;

	char frame[29];
	int cmdlen = 29;//13 + 16;
	//Command type
	frame[0] = '0';
	frame[1] = '2';//Write command
	//Content format
	if (m_cntntFMT == OM_ASCII)
		frame[2] = '1';//ASCII format
	else//OM_HEX
		frame[2] = '0';//HEX format
	//Transmission operation
	frame[3] = '0';//Single Trigger
	//Page specification
	frame[4] = '0'; frame[5] = '0';//p30~23
	frame[6] = '0'; frame[7] = '0';//p22~15
	frame[8] = '0'; frame[9] = '0';//p14~7
	frame[10] = '0';//p6~3
	frame[11] = '0';//p2~1
	int offset = 0;
	switch (page) {
	case 1:
		if (m_cntntFMT == OM_ASCII)//p2~1
		{
			frame[11] = 'C';//ASCII format, read P1 and P2
		}
		else//OM_HEX
		{
			frame[11] = '4';//HEX format, read P1 only
		}
		break;
	case 2:
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P2 and P3
		{
			frame[10] = '1';//p6~3
			frame[11] = '8';//p2~1
		}
		else//OM_HEX: HEX format, read P2 only
		{
			frame[10] = '0';//p6~3
			frame[11] = '8';//p2~1
		}
		break;
	case 3:
	case 7:
	case 11:
	case 15:
	case 19:
	case 23:
	case 27:
		offset = page / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P3 and P4
		{
			frame[10 - offset] = '3';//p6~3
		}
		else//OM_HEX: HEX format, read P3 only
		{
			frame[10 - offset] = '1';//p6~3
		}
		break;
	case 4:
	case 8:
	case 12:
	case 16:
	case 20:
	case 24:
	case 28:
		offset = (page - 1) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P4 and P5
		{
			frame[10 - offset] = '6';//p6~3
		}
		else//OM_HEX: HEX format, read P4 only
		{
			frame[10 - offset] = '2';//p6~3
		}
		break;
	case 5:
	case 9:
	case 13:
	case 17:
	case 21:
	case 25:
	case 29:
		offset = (page - 2) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P5 and P6
		{
			frame[10 - offset] = 'C';//p6~3
		}
		else//OM_HEX: HEX format, read P5 only
		{
			frame[10 - offset] = '4';//p6~3
		}
		break;
	case 6:
	case 10:
	case 14:
	case 18:
	case 22:
	case 26:
	case 30:
		offset = (page - 3) / 4;
		if (m_cntntFMT == OM_ASCII)//ASCII format, read P6 and P7
		{
			if (page < 30)//can not read non-existent P31
				frame[9 - offset] = '1';//p10~7
			frame[10 - offset] = '8';//p6~3
		}
		else//OM_HEX: HEX format, read P6 only
		{
			frame[10 - offset] = '8';//p6~3
		}
		break;
	default:
		if (m_cntntFMT == OM_ASCII)//p2~1
			frame[11] = 'C';//ASCII format, write P1 and P2
		else//OM_HEX
			frame[11] = '4';//HEX format, write P1 only
	}
	//Content to be written
	for (int i = 0; i < len; i++)
		frame[i + 12] = rfid[i];
	//Terminator
	frame[28] = 0x0D;

	int res = sem_reset(&m_semCmdRes);
	if (res != 0) {
		m_CmdState = OM_IDLE;
		return 5;//Error happened during m_semCmdRes sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Omron", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for response from Omron
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdRes, &tm);
	if (res == -1) {
		m_CmdState = OM_IDLE;
		return 6;//wait timed out
	}
	if (m_CmdRes == OM_RESERR) {
		m_CmdState = OM_IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Completion Error Code",
			m_CompleteCode, strlen(m_CompleteCode));
		return 7;//error result
	}
	m_CmdState = OM_IDLE;
	return 0;
}
int COmron::LoopBackTS(char *tststr, int len, int *aclen) {
	if (m_CmdState != OM_IDLE)
		return 2;//busy

	m_CmdState = OM_WAITRES;
	m_CurCmd = OM_TS;

	char frame[13];
	int cmdlen = 13;
	//Command type
	frame[0] = '1';
	frame[1] = '0';//Test command
	//Test String
	frame[2] = '1'; frame[3] = '2';
	frame[4] = '3'; frame[5] = '4';
	frame[6] = '5'; frame[7] = '6';
	frame[8] = '7'; frame[9] = '8';
	frame[10] = '9'; frame[11] = 'A';
	//Terminator
	frame[12] = 0x0D;

	int res = sem_reset(&m_semCmdRes);
	if (res != 0) {
		m_CmdState = OM_IDLE;
		return 4;//Error happened during m_semCmdRes sem_reset
	}
	((CLP*)m_parent)->m_log->WriteLog("To Omron", frame, cmdlen);
	m_pSerial->SendBlock(frame, cmdlen);
	/////////////////////////////////////////////////////
	//wait for 3 seconds for result to return from Omron
	struct timespec tm;
	clock_gettime(CLOCK_REALTIME, &tm);
	tm.tv_sec += 3;
	res = sem_timedwait(&m_semCmdRes, &tm);
	if (res == -1) {
		m_CmdState = OM_IDLE;
		return 5;//wait timed out
	}
	if (m_CmdRes == OM_RESERR) {
		m_CmdState = OM_IDLE;
		((CLP*)m_parent)->m_log->WriteLog("Completion Error Code",
			m_CompleteCode, strlen(m_CompleteCode));
		return 6;//error result
	}
	///////////////////////////////////////
	//process result: now m_CmdRes == OM_RESOK
	((CLP*)m_parent)->m_log->WriteLog("Read Data", m_CmdResParam, m_CmdResParamLen);
	if (m_CmdResParamLen >= len) {
		((CLP*)m_parent)->m_log->WriteLog("Read Data too long!");
		m_CmdState = OM_IDLE;
		return 7;
	}
	strcpy(tststr, m_CmdResParam);
	*aclen = m_CmdResParamLen;
	//end process result
	///////////////////////////////////////
	m_CmdState = OM_IDLE;
	return 0;
}

void COmron::DoTest() {

}
//End COmron Class Implementation
//////////////////////////////////

////////////////////////////////////
//Begin CConfig Class Implementation
CConfig::CConfig()
{
	menu[0] = "idcf - Change Carrier ID Reader Configuration";
	menu[1] = "ltcf - Change Light Curtain Configuration";
	menu[2] = "quit - Quit TDK Load Port Control";
	menu[3] = NULL;
	lpsubmenu[0] = "ldp1 - Load Port 1";
	lpsubmenu[1] = "ldp2 - Load Port 2";
	lpsubmenu[2] = "retn - Return To Main Menu";
	lpsubmenu[3] = NULL;
	lpsubmenuStr = "-------------------------------------------------------------------------\n--Carrier ID Reader config submenu: Select load port 1 or 2.\n--Allways go back to main menu to view changes.\n-------------------------------------------------------------------------\n";

	lp1submenu[0] = "barc - Use Keyence Barcode Reader";
	lp1submenu[1] = "herm - Use Hermos RFID";
	lp1submenu[2] = "omas - Use Omron RFID(ASCII format)";
	lp1submenu[3] = "omhx - Use Omron RFID(HEX format)";
	lp1submenu[4] = "retn - Return To Up Menu";
	lp1submenu[5] = NULL;
	lp1submenuStr = "-------------------------------------------------------------------------\n--Load port 1 config submenu: Select a carrier ID reader.\n--Allways go back to main menu to view changes.\n-------------------------------------------------------------------------\n";

	lp2submenu[0] = "barc - Use Keyence Barcode Reader";
	lp2submenu[1] = "herm - Use Hermos RFID";
	lp2submenu[2] = "omas - Use Omron RFID(ASCII format)";
	lp2submenu[3] = "omhx - Use Omron RFID(HEX format)";
	lp2submenu[4] = "retn - Return To Up Menu";
	lp2submenu[5] = NULL;
	lp2submenuStr = "-------------------------------------------------------------------------\n--Load port 2 config submenu: Select a carrier ID reader.\n--Allways go back to main menu to view changes.\n-------------------------------------------------------------------------\n";

	ltsubmenu[0] =
		"dslt - Disable Light Curtain";
	ltsubmenu[1] =
		"enl0 - Enable Light Curtain; Set LTC Onleve to 0V";
	ltsubmenu[2] =
		"enl1 - Enable Light Curtain; Set LTC Onlevel to 24V";
	ltsubmenu[3] =
		"enl2 - Enable Light Curtain; Detect LTC Always,not only InTransfer.";
	ltsubmenu[4] =
		"retn - Return To Main Menu";
	ltsubmenu[5] = NULL;
	ltsubmenuStr = "-------------------------------------------------------------------------\n--Light curtain submenu: Select a configuration.\n--Allways go back to main menu to view changes.\n-------------------------------------------------------------------------\n";

	strcpy(cfgfileStr, "1h2h:En0Lv1");
	cfgfileExplnStr = "\nString before ':'\n   1 for lp1, 2 for lp2;\n   b for barcode reader, h for Hermos RFID, o for Omron RFID(ASCII), m for Omron RFID(HEX).\nString after ':'\n   En0 for LTC disabled, En1 for LTC enabled-InTransfer, En2 for LTC enabled-Always;\n   Lv0 for LTC Onlevel is 0V, Lv1 for LTC Onlevel is 24V";

}

//prmSel: 1-_N2PurgeNozzleDown_InE84; 
int CConfig::rdSingleCfgFile(int prmSel)
{
	char filename[100];
	char valstr[100];
	int rdVal = -1;

	if (prmSel == 1)
	{
		strcpy(filename, "/usr/tdk/lp/n2.conf");
	}
	else
		return rdVal;

	FILE *stream = NULL;
	if ((stream = fopen(filename, "rb")) == NULL)
	{
		printf("Open .conf failed, Creating a default one\n");
		sleep(1);
		if ((stream = fopen(filename, "wb")) == NULL)
		{
			printf("Creating .conf failed!\n");
			sleep(1);
			rdVal = -1;
		}
		else
		{
			sprintf(valstr, "%d", 0);
			//write default config
			fwrite(valstr, sizeof(char), strlen(valstr), stream);
			rdVal = 0;
		}
		sleep(1);
	}
	else
	{
		if (fgets(valstr, 20, stream) == NULL)
		{
			printf("Read config error!\n");
			sleep(1);
			rdVal = -1;
		}
		else
		{
			rdVal = valstr[0] - 0x30;
		}
	}

	if (stream != NULL)
		fclose(stream);

	return rdVal;
}

int CConfig::wrSingleCfgFile(int prmSel, int prmVal)
{
	char filename[100];
	char valstr[100];

	if (prmSel == 1)
	{
		strcpy(filename, "/usr/tdk/lp/n2.conf");
	}
	else
		return 1;

	FILE *stream = NULL;
	if ((stream = fopen(filename, "wb")) == NULL)
	{
		printf("Creating .conf failed!\n");
		sleep(1);
		return 2;
	}
	else
	{
		sprintf(valstr, "%d", prmVal);
		//write default config
		fwrite(valstr, sizeof(char), strlen(valstr), stream);
	}
	if (stream != NULL)
		fclose(stream);

	return 0;
}

void CConfig::rdCfgnSet(char *mmenuStr, char *cfgfStr)
{
	FILE *stream = NULL;
	if ((stream = fopen("/usr/tdk/lp/lp.conf", "rb")) == NULL) {
		printf("Open lp.conf failed, Creating a default one\n");
		sleep(1);
		if ((stream = fopen("/usr/tdk/lp/lp.conf", "wb")) == NULL) {
			printf("Creating lp.conf failed!\n");
			sleep(1);
		}
		else {//Writing default lp.conf
		  //Set current configuration display string and the 2 control variables
			strcpy(cfgfStr, "1h2h:En0Lv1");
			setmmStrnVar(0, mmenuStr, &_LP1IdReader, &_LP2IdReader,
				&_LTCEnDis, &_LTCOnLevel, cfgfStr);
			//write default config
			char writeconf[500];
			strcpy(writeconf, cfgfStr);
			strcat(writeconf, cfgfileExplnStr);
			fwrite(writeconf, sizeof(char), strlen(writeconf), stream);
		}
		sleep(1);
	}
	else {
		if (fgets(cfgfStr, 20, stream) == NULL) {
			printf("Read config error!\n");
			sleep(1);
		}
		else {
			int res;
			res = setmmStrnVar(0, mmenuStr, &_LP1IdReader, &_LP2IdReader,
				&_LTCEnDis, &_LTCOnLevel, cfgfStr);
			if (res != 0) {
				if (stream != NULL)
					fclose(stream);
				printf("Wrong format in lp.conf, Recreate default file!\n");
				sleep(2);
				stream = NULL;
				if ((stream = fopen("/usr/tdk/lp/lp.conf", "wb")) == NULL) {
					printf("Creating lp.conf failed!\n");
					sleep(1);
				}
				else {
					//Set current configuration display string and the 2 contrl variables
					strcpy(cfgfStr, "1h2h:En0Lv1");
					setmmStrnVar(0, mmenuStr, &_LP1IdReader, &_LP2IdReader,
						&_LTCEnDis, &_LTCOnLevel, cfgfStr);
					//write default config
					char writeconf[500];
					strcpy(writeconf, cfgfStr);
					strcat(writeconf, cfgfileExplnStr);
					fwrite(writeconf, sizeof(char), strlen(writeconf), stream);
				}
			}
		}
	}
	if (stream != NULL)
		fclose(stream);
}

void CConfig::wrCfgnSet(char orgchged, char *mmenuStr, char *cfgfStr)
{
	FILE *stream = NULL;
	if ((stream = fopen("/usr/tdk/lp/lp.conf", "wb")) == NULL) {
		printf("Creating lp.conf failed!\n");
		sleep(1);
	}
	else {
		//Set current configuration display string and the 2 control variables
		setmmStrnVar(orgchged, mmenuStr, &_LP1IdReader, &_LP2IdReader,
			&_LTCEnDis, &_LTCOnLevel, cfgfStr);
		//write default config
		char writeconf[500];
		strcpy(writeconf, cfgfStr);
		strcat(writeconf, cfgfileExplnStr);
		fwrite(writeconf, sizeof(char), strlen(writeconf), stream);
	}

	if (stream != NULL)
		fclose(stream);
}

int CConfig::setmmStrnVar(char lpcfchgedfrmorg, char *mmString, int *lp1reader,
	int *lp2reader, int *ltEnDis, int *ltOnlevl, char *conf)
{
	if (strlen(conf) != 11 && strlen(conf) != 12) //"1h2h:En0Lv1"
		return 1;
	if (conf[0] != '1' || conf[2] != '2')
		return 2;
	if (conf[4] != ':'
		|| conf[5] != 'E' || conf[6] != 'n' || (conf[7] != '0' && conf[7] != '1'&& conf[7] != '2')
		|| conf[8] != 'L' || conf[9] != 'v' || (conf[10] != '0' && conf[10] != '1'))
		return 2;

	if (conf[1] == 'b')
		*lp1reader = 1;
	else if (conf[1] == 'h')
		*lp1reader = 2;
	else if (conf[1] == 'o')
		*lp1reader = 3;
	else if (conf[1] == 'm')
		*lp1reader = 4;
	else
		*lp1reader = 2;

	if (conf[3] == 'b')
		*lp2reader = 1;
	else if (conf[3] == 'h')
		*lp2reader = 2;
	else if (conf[3] == 'o')
		*lp2reader = 3;
	else if (conf[3] == 'm')
		*lp2reader = 4;
	else
		*lp2reader = 2;

	*ltEnDis = conf[7] - 0x30;
	*ltOnlevl = conf[10] - 0x30;

	char str1[100];
	char str2[100];
	char str_lp1IDreader[30];
	char str_lp2IDreader[30];
	switch (*lp1reader)
	{
	case 1:
		sprintf(str_lp1IDreader, "Barcode Reader");
		break;
	case 2:
		sprintf(str_lp1IDreader, "Hermos RFID");
		break;
	case 3:
		sprintf(str_lp1IDreader, "Omron RFID(ASCII)");
		break;
	case 4:
		sprintf(str_lp1IDreader, "Omron RFID(HEX)");
		break;
	default:
		sprintf(str_lp1IDreader, "Hermos RFID");
	}

	switch (*lp2reader)
	{
	case 1:
		sprintf(str_lp2IDreader, "Barcode Reader");
		break;
	case 2:
		sprintf(str_lp2IDreader, "Hermos RFID");
		break;
	case 3:
		sprintf(str_lp2IDreader, "Omron RFID(ASCII)");
		break;
	case 4:
		sprintf(str_lp2IDreader, "Omron RFID(HEX)");
		break;
	default:
		sprintf(str_lp2IDreader, "Hermos RFID");
	}

	sprintf(str1, "\n    Load port 1 uses %s; Load port 2 uses %s.", str_lp1IDreader, str_lp2IDreader);
	if (*ltEnDis != 0)
		sprintf(str2, "\n    Light curtain %s; Onlevel is %s.", (*ltEnDis == 2) ? "[Enable-Always]" : "[Enable-InTransfer]", (*ltOnlevl) ? "[24V]" : "[0V]");
	else
		sprintf(str2, "\n    Light curtain [Disabled]");

	strcpy(mmString, "-------------------------------------------------------------------------");
	strcat(mmString, "\n--Main Menu: Change configuration or Quit.");
	strcat(mmString, "\n--Current Configuration:");
	strcat(mmString, str1);
	strcat(mmString, str2);
	strcat(mmString, "\n--Note: CFG may be changed by Host,Press enter to update.");
	if (lpcfchgedfrmorg)
		strcat(mmString, "\n--ID reader configuration changed, quit and restart the controller!");
	strcat(mmString, "\n-------------------------------------------------------------------------\n");

	return 0;
}

int CConfig::getchoice(char *greet, char *choices[])
{
	int chosen = 0;//indicate whether a valid choice in choices[] has been made
	int index = 0;//if valid choice has been made, the index of the choice
	char input[100];//user input string of choice
	char **option;
	static int counter = 0;
	do {
		system("clear");
		printf("\n\n\n\n\n");
		printf("               ########################################\n");
		printf("               ##        TDK Load Port Control       ##\n");
		printf("               ##     Hermes Microvision Inc. 2007   ##\n");
		printf("               ########################################\n");
		printf("%s\n", greet);
		option = choices;
		while (*option) {
			printf("%s\n", *option);
			option++;
		}
		input[0] = '\0';
		//gets(input);//This function is dangerous
		read(0, input, 80);//read from stdin
		option = choices;
		index = 0;
		while (*option) {
			if (input[0] == (*option)[0] &&
				input[1] == (*option)[1] &&
				input[2] == (*option)[2] &&
				input[3] == (*option)[3]) {
				chosen = 1;
				break;
			}
			option++;
			index++;
		}
		if (!chosen) {
			if (counter != 0) {//deal with the first "a\n" problem when startup
				if (input[0] != '\n') {
					printf("Incorrect choice, select again\n");
					sleep(1);
				}
			}
		}
		counter++;
		if (counter == 1000)counter = 1;
	} while (!chosen);
	return index;//only return when a valid choice has been made
}

void CConfig::InitConfig()
{
	rdCfgnSet(mainmenuStr, cfgfileStr);
	strcpy(orgcfgfStr, cfgfileStr);
	int val = rdSingleCfgFile(1);
	if (val != 0 && val != 1)
		_N2PurgeNozzleDown_InE84 = 0;
	else
		_N2PurgeNozzleDown_InE84 = val;
}

int CConfig::MenuSelection()//return 0: choice no change
{                           //return 1: choice changed
	char lpcfchged = 0;
	int choice;
	int subchoice;
	int lp1CIDChoice;
	int lp2CIDChoice;
	do {
		choice = getchoice(mainmenuStr, menu);
		if (choice == 0) {//"idcf" selected
			do {
				subchoice = getchoice(lpsubmenuStr, lpsubmenu);
				if (subchoice != 2) {//"retn" not selected
					if (subchoice == 0) {//"lp1" selected 
						lp1CIDChoice = getchoice(lp1submenuStr, lp1submenu);
						switch (lp1CIDChoice)
						{
						case 0:
							cfgfileStr[1] = 'b';//"Use Keyence Barcode Reader";
							break;
						case 1:
							cfgfileStr[1] = 'h';//"Use Hermos RFID";
							break;
						case 2:
							cfgfileStr[1] = 'o';//"Use Omron RFID(ASCII format)"
							break;
						case 3:
							cfgfileStr[1] = 'm';//"Use Omron RFID(HEX format)"
							break;
						default://"retn" selected, do nothing
							break;
						}
					}
					else if (subchoice == 1) {//"lp2" selected
						lp2CIDChoice = getchoice(lp2submenuStr, lp2submenu);
						switch (lp2CIDChoice)
						{
						case 0:
							cfgfileStr[3] = 'b';
							break;
						case 1:
							cfgfileStr[3] = 'h';
							break;
						case 2:
							cfgfileStr[3] = 'o';
							break;
						case 3:
							cfgfileStr[3] = 'm';
							break;
						default://"retn" selected, do nothing
							break;
						}
					}

					if (cfgfileStr[1] != orgcfgfStr[1] ||
						cfgfileStr[3] != orgcfgfStr[3])
						lpcfchged = 1;
					else
						lpcfchged = 0;
					wrCfgnSet(lpcfchged, mainmenuStr, cfgfileStr);
				}
			} while (subchoice != 2);
		}
		else if (choice == 1) {//"ltcf" selected
			subchoice = getchoice(ltsubmenuStr, ltsubmenu);
			if (subchoice != 4) {//"retn" not selected
				if (subchoice == 0) {
					cfgfileStr[7] = '0';
				}
				else if (subchoice == 1) {
					cfgfileStr[7] = '1'; cfgfileStr[10] = '0';
				}
				else if (subchoice == 2) {
					cfgfileStr[7] = '1'; cfgfileStr[10] = '1';
				}
				else if (subchoice == 3) {
					cfgfileStr[7] = '2';
				}

				if (cfgfileStr[1] != orgcfgfStr[1] ||
					cfgfileStr[3] != orgcfgfStr[3])
					lpcfchged = 1;
				else
					lpcfchged = 0;
				wrCfgnSet(lpcfchged, mainmenuStr, cfgfileStr);
			}
		}
	} while (choice != 2);//"quit" not selected
	if (cfgfileStr[1] != orgcfgfStr[1] ||
		cfgfileStr[3] != orgcfgfStr[3])
		return 1;
	return 0;
}////////////////////////////////////

//////////////////////////////////
//Test area
class CTest {
public:
	static void CallbackWrap(void *pCallbackObj, char *pMsg, int len);
	CSerial *m_pSerial;
	CTest();
	~CTest();
	void DoTest();
private:
	void Callback(char *pMsg, int len);
};

CTest::CTest() {
	printf("CTest Constructor called!\n");
	m_pSerial = new CSerial(1, "TestA"/*log file name base*/);
}
CTest::~CTest() {
	printf("CTest Destructor called!\n");
	if (m_pSerial != NULL)
		delete m_pSerial;
}

void CTest::CallbackWrap(void *pCallbackObj, char *pMsg, int len)
{
	CTest *pSelf = (CTest*)pCallbackObj;
	pSelf->Callback(pMsg, len);
}

void CTest::Callback(char *pMsg, int len)
{
	char *pM = new char[len + 1];
	for (int i = 0; i < len; i++) {
		pM[i] = pMsg[i];
		if (pM[i] == 0x0D)
			pM[i] = '_';
	}
	pM[len] = '\0';
	printf("CTest::Callback, Received callback message: %s\n", pM);
	delete[] pM;
}

/*void CTest::DoTest(){
  m_pSerial->Initialize(BL600, this, CallbackWrap);
  m_pSerial->fakeCB();
  m_pSerial->OpenConnection();
  // m_pSerial->SendBlock("Test block1", 11);
  sleep(1);
  char tas[100];
L:  tas[0] = 'O';
  tas[1] = 'K';  tas[2] = 'O';  tas[3] = 'N'; tas[4] = 0x0D;
  m_pSerial->SendBlock(tas, 5);
  sleep(3);
  tas[0] = 'N';  tas[1] = 'G';  tas[2] = 'O';  tas[3] = 'N';  tas[4] = 0x0D;
  m_pSerial->SendBlock(tas, 5);
  sleep(3);  tas[0] = 'A';  tas[1] = 'L';  tas[2] = 'L';  tas[3] = 'O';
  tas[4] = 'F';  tas[5] = 'F';  tas[6] = 0x0D;
  m_pSerial->SendBlock(tas, 7);

  sleep(1);
  tas[0] = 'M';  tas[1] = 'O';  tas[2] = 'T';  tas[3] = 'O';  tas[4] = 'R';
  tas[5] = 'O';  tas[6] = 'N';  tas[7] = 0x0D;
  m_pSerial->SendBlock(tas, 8);

  sleep(6);
  tas[0] = 'L';  tas[1] = 'O';  tas[2] = 'N';  tas[3] = 0x0D;
  m_pSerial->SendBlock(tas, 4);

  sleep(5);
  tas[0] = 'L';  tas[1] = 'O';  tas[2] = 'F';  tas[3] = 'F';  tas[4] = 0x0D;
  m_pSerial->SendBlock(tas, 5);

  sleep(1);
  tas[0] = 'M';  tas[1] = 'O';  tas[2] = 'T';  tas[3] = 'O';  tas[4] = 'R';
  tas[5] = 'O';  tas[6] = 'F';  tas[7] = 'F';  tas[8] = 0x0D;
  m_pSerial->SendBlock(tas, 9);

  char ch;
  read(0, &ch, 1);
  if(ch == 'c'|| ch == '\n')goto L;

  m_pSerial->CloseConnection();
  sleep(1);
}*/
void CTest::DoTest() {
	m_pSerial->Initialize(HERMOS, this, CallbackWrap);
	m_pSerial->fakeCB();
	m_pSerial->OpenConnection();
	sleep(1);
	char tas[100];
L:
	tas[0] = 'S';  tas[1] = '0';  tas[2] = '4';//Package Header
	tas[3] = 'X';  tas[4] = '0';  tas[5] = '0';  tas[6] = '2'; //Message
	tas[7] = 0x0D;  tas[8] = '3';  tas[9] = '0'; tas[10] = 'A';  tas[11] = 'E';//Package End
	m_pSerial->SendBlock(tas, 12);

	char ch;
	read(0, &ch, 1);
	if (ch == 'c' || ch == '\n')goto L;

	m_pSerial->CloseConnection();
	sleep(1);
}
//End Test area
int main()
{
	char ch;
	int restart = 0;
	CLog::RunMaintain();
	CConfig *cfg;
	CLP *lpObj1;
	CLP *lpObj2;
	cfg = new CConfig();
	cfg->InitConfig();
	lpObj1 = new CLP(1);
	lpObj2 = new CLP(2);
	lpObj1->Setbrother(lpObj2);
	lpObj2->Setbrother(lpObj1);
	lpObj1->Setconfig(cfg);
	lpObj2->Setconfig(cfg);
	lpObj1->EnableOperation();
	lpObj2->EnableOperation();
	lpObj1->CheckHWstatus();
	lpObj1->SendToHostMaxReceiveTimes();
	lpObj1->Brkid();

	;	printf("Press 'a' and 'enter' keys to go to main menu...\n");
L:
	read(0, &ch, 1);
	if (ch != 'a')goto L;
	restart = cfg->MenuSelection();
	lpObj1->DisableOperation();
	lpObj2->DisableOperation();
	delete lpObj1;
	delete lpObj2;
	delete cfg;
	if (restart != 0) {
		printf("                You have changed ID reader configuration.\n             Please Restart The Controller To Take Effect!!!\n\n\n");
		//sleep(2);//this will cause segmentation fault
	}
	return 0;
}
//begin Hermos test
/*
int main()
{
  CLP *lpObj;
  lpObj = new CLP(1);
  lpObj->EnableOperation();
  lpObj->m_hmos->DoTest();
  sleep(1);
  lpObj->DisableOperation();
  return 0;
  //CTest tstObj;
  //tstObj.DoTest();
  //return 0;
}*/
////////////////////////////////////////////////
//compile option
//g++ -o lp lp.cc -lpthread /usr/lib/librt.a seaio.a -g
//End compile option
////////////////////////////////////////////////
/*
///////////////////////////
//E84 test
int main()
{
  CLP lpObj;
  lpObj.EnableOperation();
  lpObj.m_e84->DoTest();
  sleep(1);
  lpObj.DisableOperation();
  return 0;
}*/
//begin bl600 test
/*
int main()
{
  CConfig *cfg;
  CLP *lpObj1;
  CLP *lpObj2;
  cfg = new CConfig();
  cfg->InitConfig();
  lpObj1 = new CLP(1);
  lpObj2 = new CLP(2);
  lpObj1->EnableOperation();
  printf("Start Barcode Test:\n");
  lpObj1->m_bl600->DoTest();
  sleep(1);
  lpObj1->DisableOperation();
  return 0;
}
int CLP::EnableOperation(){
  m_log->WriteLog("Enable Load Port Operation");
  if(m_prgState != prg_NOTINIT)
	return 1;

	//  int res = m_host->OpenPort();
	//if(res != 0){
	//m_log->WriteLog("Open Port for HostBK failed");
	//return 2;
	//}

	//  int res = m_tas300->OpenPort();
	//if(res != 0){
	//m_log->WriteLog("Open Port for Tas300 failed");
	//return 3;
	//}

  int res = m_bl600->OpenPort();
  if(res != 0){
	m_log->WriteLog("Open Port for Bl600 failed");
	return 4;
  }
  m_prgState = prg_READY;
  m_log->WriteLog("PRG state => prg_READY");
  return 0;
}
int CLP::DisableOperation(){
  m_log->WriteLog("Disable Load Port Operation");

  while(m_prgState != prg_READY)usleep(100000);//100ms
  m_prgState = prg_NOTINIT;
  m_log->WriteLog("PRG state => prg_NOTINIT");

  //int res = m_host->ClosePort();
  //if(res != 0){
  // m_log->WriteLog("Close Port for HostBK failed");
  //return 1;
  //}
  //int res = m_tas300->ClosePort();
  //if(res != 0){
  //m_log->WriteLog("Close Port for Tas300 failed");
  //return 2;
  //}
  int res = m_bl600->ClosePort();
  if(res != 0){
	m_log->WriteLog("Close Port for Bl600 failed");
	return 3;
  }

  return 0;
}
//end bl600 test
int main()
{
  //CTest tstObj;
  //tstObj.DoTest();
  CLP lpObj;
  lpObj.EnableOperation();
  //lpObj.m_host->DoTest();
  //lpObj.m_tas300->DoTest();
  lpObj.m_bl600->DoTest();
L:
  char ch;
  read(0, &ch, 1);
  if(ch == 'c'|| ch == '\n')goto L;
  lpObj.DisableOperation();
  sleep(1);
  return 0;
}

void CTas300::DoTest(){
  char st[100];
  //sprintf(st, "io es 1 1\r\n");
  // m_pSerial->SendBlock(st, strlen(st));
  int res = statfxl();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request ACKed", m_statfxl, strlen(m_statfxl));
	if(m_fpStatus == 0)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_NOFOUP");
	else if(m_fpStatus == 1)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_PLACED");
	else if(m_fpStatus == 2)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_CLAMPED");
	else if(m_fpStatus == 3)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_DOCKED");
	else if(m_fpStatus == 4)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_OPENED");
	else if(m_fpStatus == -1)
	  ((CLP*)m_parent)->m_log->WriteLog("FPS_UNKNOWN");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error in Statfxl()");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error in Statfxl()");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout in Statfxl()");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
   ((CLP*)m_parent)->m_log->WriteLog("Incorrect Param Length");
  else if(res == 8)
   ((CLP*)m_parent)->m_log->WriteLog("Incorrect Parameters");
  else if(res == 9)
   ((CLP*)m_parent)->m_log->WriteLog("m_fpStatus and m_statfxl not updated");
}

void CTas300::DoTest(){
  char st[100];
  int res = mapResult();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request ACKed", m_mapRes, strlen(m_mapRes));
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error in mapResult()");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error in mapResult()");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout in mapResult()");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
   ((CLP*)m_parent)->m_log->WriteLog("Incorrect Param Length");
  else if(res == 8)
   ((CLP*)m_parent)->m_log->WriteLog("Incorrect Parameters");
}

void CTas300::DoTest(){
  char st[100];
  int res = evtON();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request ACKed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error in evtON()");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error in evtON()");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout in evtON()");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
}

void CTas300::DoTest(){
  char st[100];
  int res = fpeON();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request ACKed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error in fpeON()");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error in fpeON()");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout in fpeON()");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
}

void CTas300::DoTest(){
  char st[100];
  int res = rstErr();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request INFed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error in rstErr()");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error in rstErr()");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout in rstErr()");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore 2 error in rstErr()");
  else if(res == 8)
	((CLP*)m_parent)->m_log->WriteLog("Wait INF timeout in rstErr");
  else if(res == 9)
	((CLP*)m_parent)->m_log->WriteLog("Non ABS and non INF result");
  else if(res == 10)
	((CLP*)m_parent)->m_log->WriteLog("Request ABSed");
}

void CTas300::DoTest(){
  char st[100];
  int res = prgInit();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request INFed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore 2 error");
  else if(res == 8)
	((CLP*)m_parent)->m_log->WriteLog("Wait INF timeout");
  else if(res == 9)
	((CLP*)m_parent)->m_log->WriteLog("Non ABS and non INF result");
  else if(res == 10)
	((CLP*)m_parent)->m_log->WriteLog("Request ABSed");
}
void CTas300::DoTest(){
  char st[100];
  int res = lampOP(6, LMP_BL);
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request INFed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore 2 error");
  else if(res == 8)
	((CLP*)m_parent)->m_log->WriteLog("Wait INF timeout");
  else if(res == 9)
	((CLP*)m_parent)->m_log->WriteLog("Non ABS and non INF result");
  else if(res == 10)
	((CLP*)m_parent)->m_log->WriteLog("Request ABSed");
}

void CTas300::DoTest(){
  char st[100];
  int res = movOP("ABORG");
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request INFed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore 2 error");
  else if(res == 8)
	((CLP*)m_parent)->m_log->WriteLog("Wait INF timeout");
  else if(res == 9)
	((CLP*)m_parent)->m_log->WriteLog("Non ABS and non INF result");
  else if(res == 10)
	((CLP*)m_parent)->m_log->WriteLog("Request ABSed");
}

void CTas300::DoTest(){
  char st[100];
  int res = movABORT();
  if(res == 0){
	((CLP*)m_parent)->m_log->
	  WriteLog("Request INFed");
  }
  else if(res == 1)
	((CLP*)m_parent)->m_log->WriteLog("Busy!");
  else if(res == 2)
	((CLP*)m_parent)->m_log->WriteLog("Internal error");
  else if(res == 3)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore error");
  else if(res == 4)
	((CLP*)m_parent)->m_log->WriteLog("Wait timeout");
  else if(res == 5)
	((CLP*)m_parent)->m_log->WriteLog("Request NAKed");
  else if(res == 6)
	((CLP*)m_parent)->m_log->WriteLog("Request neither ACKed or NAKed");
  else if(res == 7)
	((CLP*)m_parent)->m_log->WriteLog("Reset semaphore 2 error");
  else if(res == 8)
	((CLP*)m_parent)->m_log->WriteLog("Wait INF timeout");
  else if(res == 9)
	((CLP*)m_parent)->m_log->WriteLog("Non ABS and non INF result");
  else if(res == 10)
	((CLP*)m_parent)->m_log->WriteLog("Request ABSed");
}
/////////////////////////////////////////////////////////////
//Omron V700-CD1D controller communication protocol with Host
	case OMRON:
	  if(msgCurRetries < msgMaxRetries){
		  if(msgTail > 0) msgCurRetries++;//retry begin only after first !0 read
		  int msgRemainLen = MAXSIZE - msgTail;
		  bytesRd = read(m_fd, buffer, msgRemainLen);
		  if(bytesRd < 0){
			  bytesRd = 0;
			  //  this->m_log->WriteLog(
			  //	"In Receiving thread: read error! Port does not exist! ");
		  }
		  msgTail += bytesRd;
		  for(int i=premsgTail; i<msgTail; i++)
			  msg[i] = buffer[i-premsgTail];
		  premsgTail = msgTail;
		  ////////////////////////////
		  //judge if message complete
		  if(msgTail == 0)
			  break;
		  if(msg[0] != 0x02){//1st chararcter is not STX
			  int iS = 0;
			  while(iS<msgTail){
				  if(msg[iS] == 0x02) break;
				  iS++;
			  }
			  int len = iS;
			  this->m_log->WriteLog("Received", msg, len);//send the garbage
			  this->m_CallbackW(m_pCBObj, msg, len);
			  if(len < msgTail){//move the remaining msg part to its head
				  for(int i=len; i<msgTail; i++)msg[i-len] = msg[i];
			  }
			  msgCurRetries = 0;
			  msgTail -= len;//most probably will be 0
			  premsgTail = msgTail;
		  }
		  else{//1st character is STX
			  if(msgTail<3)break;
			  int iCheck = premsgTail;
			  while(iCheck < msgTail-1){//considering BCC or wait for one more byte after ETX
				  if(iCheck > 0){
					  if(msg[iCheck] == 0x02){//meet STX again
						  int len = iCheck;
						  this->m_log->WriteLog("Received", msg, len);//send the garbage
						  this->m_CallbackW(m_pCBObj, msg, len);
						  if(len < msgTail){//move the remaining msg part to its head
							  for(int i=len; i<msgTail; i++)msg[i-len] = msg[i];
						  }
						  msgCurRetries = 0;
						  msgTail -= len;//most probably will be 0
						  premsgTail = msgTail;
						  break;
					  }
					  else if(msg[iCheck] == 0x03){//ETX found
						  int len = iCheck+2;
						  this->m_log->WriteLog("Received", msg, len);//send the frame
						  this->m_CallbackW(m_pCBObj, msg, len);
						  if(len < msgTail){//move the remaining msg part to its head
							  for(int i=len; i<msgTail; i++)msg[i-len] = msg[i];
						  }
						  msgCurRetries = 0;
						  msgTail -= len;//most probably will be 0
						  premsgTail = msgTail;
						  break;
					  }
				  }
				  iCheck++;
			  }
		  }
	  }
	  else{//reached maximum retry time, message still incomplete, end anyway
		  this->m_log->WriteLog("Received", msg, msgTail);
		  this->m_CallbackW(m_pCBObj, msg, msgTail);
		  msgCurRetries = 0;
		  msgTail = 0;
		  premsgTail = 0;
	  }
//End Omron V700-CD1D controller communication protocol with Host
/////////////////////////////////////////////////////////////////
*/
