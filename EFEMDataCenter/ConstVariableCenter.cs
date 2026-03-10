using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EFEM.DataCenter
{
    //Const Variable Center
    public static class ConstVC
    {
        public static class FilePath
        {
            public const string CrashReportFolder = @"D:\EFEMManifest";
            public const string ConfigFolder = @"D:\TDKConfig\";
            public const string AutoBackupFolder = @"D:\EFEMConfig\AutoBackup\";
            public const string ScriptFolder = @"D:\EFEMConfig\EFEMScript";
            public const string EFEMConfig = "EFEMConfig.xml";
            public const string EFEMConfigAutoBackup = "EFEMConfig_AutoBackup.xml";
            public const string EFEMExceptionInfo = "EFEMExceptionInfo.xml";
            public const string EFEMAlarmHistory = "EFEMAlarmHistory.xml";
            public const string EFEMObjectInfo = "EFEMObjectInfo.xml";
            public const string EFEMVariableCenter = "EFEMVarCenter.xml";
            public const string TCPIP = "TCPIP.xml";
            public const string RS232 = "RS232.xml";
            public const string User = "EFEMUser.xml";

            public const string TaughtPositionXml = "TaughtPosition.xml";
            public const string TaughtPositionXmlAutoBackup = "TaughtPosition_AutoBackup.xml";
        }

        public static class ObjectName
        {
            public const string GUI = "EFEMGUI";
            public const string HostProxy = "HostProxy";
            public const string LogService = "LogService";
            public const string SystemEvents = "SystemEvents";
            public const string ExceptionManagement = "ExceptionManagement";
            public const string DigitalIOBoard = "DigitalIOBoard";
            public const string AnalogIOBoard = "AnalogIOBoard";
            public const string MachineControl = "MachineControl";
            public const string KawasakiRobot = "KawasakiRob";
            public const string VariableCenter = "VariableCenter";
            public const string FFUUnit = "FFUUnit";
            public const string UserAction = "UserAction";
            public const string ScriptRun = "ScriptRun";            
        }

        public static class LogConfig
        {
            public static bool LogVCInGUI = false;  //False for release
            public static bool LogVCInHost = false; //False for release
            //<0: Log all received package, 0: Do not log, others: only x bytes
            public const int LogIODataStreamLength = 8; //Only the first 8 bytes have meaning

            public static int MaxLoggingPackageLength = 1024;

            public const string DateTimeFormat = "MM/dd/yyyy HH:mm:ss:fffffff";
            public const string EventIDFormat = "000";
            public const string ConfigFolder = @"D:\EFEMConfig\";
            public const string LogFolder = @"D:\EFEMLOG\";
            public const string LogConfigNameBase = "LogConfig";
            public const string LogObjectNameBase = "LogObjectInfo";

            public static string LogXMLFileFullName
            {
                get { return ConfigFolder + LogConfigNameBase + ".xml"; }
            }

            public static string LogXSDFileFullName
            {
                get { return ConfigFolder + LogConfigNameBase + ".xsd"; }
            }

            public static string LogObjectXMLFileFullName
            {
                get { return ConfigFolder + LogObjectNameBase + ".xml"; }
            }

            public static string LogObjectXSDFileFullName
            {
                get { return ConfigFolder + LogObjectNameBase + ".xsd"; }
            }

            public static string LogXMLFileName
            {
                get { return LogConfigNameBase + ".xml"; }
            }

            public static string LogXSDFileName
            {
                get { return LogConfigNameBase + ".xsd"; }
            }

            public static string LogObjectXMLFileName
            {
                get { return LogObjectNameBase + ".xml"; }
            }

            public static string LogObjectXSDFileName
            {
                get { return LogObjectNameBase + ".xsd"; }
            }
        }

        public static class EFEM
        {
            public const int DefaultLPCount = 2;
            public const int MaxLPSupportCount = 3;
            public const int MaxSlotSupportCount = 50;
            public const int RegularLPSlotCount = 25;
            //Max slot count support for regular size LoadPort
            public const int MaxRegularLPSlotCount = 26;

            public const double DefaultPreAlignAngle = 134.0;

            //For Run
            public const int DefaultRunLevel = 2; //Default in HostComputer is 4. For safty concern, use level 2 before Host's command. 
            public readonly static float[] DefaultRobotSpeedForRun = new float[4] { 20.0f, 40.0f, 60.0f, 80.0f };
            public readonly static float[] DefaultAlignerSpeedForRun = new float[4] { 20.0f, 40.0f, 60.0f, 80.0f };

            //For Maintain
            public const float DefaultAlignerSpeedForMaintain = 20.0f; //1 to 100 (%)
            public const float DefaultRobotSpeedForMaintain = 20.0f; //1 to 100 (%)            

            public static int CurrentLPCount = 0;             //0: Unknown
            public static string CurrentLoginUserType = null; //null: Unknown 
            public static int ExtendSensorDelay_Get = 200;
            public static int ExtendSensorDelay_Put = 200;
        }

        public static class Kawasaki
        {
            public const bool Default_GERRRET = false;
            public const bool Default_PERRRET = false;
            public const bool Default_FANR1 = true;
            public const float Default_SPDPERR = 10.0f;
            public const float Default_SPDGERR = 10.0f;
            public const float Default_SPDUPDN = 10.0f;
            public const float Default_A1POS_3LP = 180.0f;
            public const float Default_A1POS_2LP = 0.0f;
            public const float Default_HOMZR1 = 0.0f;

            public static bool DefaultForceSendIngoreCS = false;
        }

        public static class FFU
        {
            public const int DefaultStdDev = 20;
            public const int DefaultUpperLimit = 2500;
            public const int DefaultLowerLimit = 500;
            public const int DefaultQueryInterval = 1000;
        }

        public static class Register
        {
            public const string UserLoginHistory = "EFEMGUI";
        }

        public static class Display
        {
            public const string StatusInfoDateTimeFormat = "HH:mm:ss:fff";
            public const string NotificationDateTimeFormat = "MM/dd/yyyy HH:mm:ss";
            public const string ControllerDataTimeFormat = "yyyy/MM/dd HH:mm:ss";
            public const string AlarmListDataTimeFormat = "yyyy/MM/dd HH:mm:ss.ff";
        }

        public static class VariableCenter
        {
            public const string SubscriberForNewVariable = "1,2,3";
            public const string CurrentStatus = "CurrentStatusMessage";
        }

        public static class NetWork
        {
            public const string PreferredNICDomain = "10.202.0.0"; //**************Will need to move to configuration file
        }

        public enum CommunicationType
        {
            RS232 = 0,
            TCPTP = 1,
            UDP = 2
        }

        public static class Debug
        {
            public static bool DisableHostGUIInterlock = false;
            public static bool ShowRDGUI = false;
            public static bool IsDebugCommandLine = false;
            public static bool InCommDebugMode = false;

            public static bool ShowCtrlCommStatus = false;
            public static bool ShowHostCommStatus = false;
            public static bool ShowFFUCommStatus = false;

            public static bool KeepOfflineAfterStartup = false;

            public static bool bSimulate_TaughtPosInfo = false;
            public static bool bSimulate_Controller = false;
            public static bool bSimulate_FFU = false;

            public static bool MonitorCtrlComm
            {
                get { return InCommDebugMode && ShowCtrlCommStatus; }
            }

            public static bool MonitorHostComm
            {
                get { return InCommDebugMode && ShowHostCommStatus; }
            }

            public static bool MonitorFFUComm
            {
                get { return InCommDebugMode && ShowFFUCommStatus; }
            }
        }

        public static class Notification
        {
            public static bool AllowUserToCloseAutoAlarm = true;
        }
    }

    public class PreciseDatetime
    {
        // using DateTime.Now resulted in many many log events with the same timestamp.
        // use static variables in case there are many instances of this class in use in the same program
        // (that way they will all be in sync)
        private static readonly Stopwatch myStopwatch = new Stopwatch();
        private static System.DateTime myStopwatchStartTime;

        static PreciseDatetime()
        {
            Reset();

            try
            {
                // In case the system clock gets updated
                SystemEvents.TimeChanged += SystemEvents_TimeChanged;
            }
            catch (Exception)
            {
            }
        }

        static void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            Reset();
        }

        static public void Reset()
        {
            myStopwatchStartTime = System.DateTime.Now;
            myStopwatch.Restart();
        }

        public static System.DateTime Now { get { return myStopwatchStartTime.Add(myStopwatch.Elapsed); } }

        private static Stopwatch myCalcWatch = new Stopwatch();
        private static string ItemName = null;

        public static string TimeCalc_START(string strName)
        {
            string msg = null;
            
            ItemName = strName;

            myCalcWatch.Reset();
            myCalcWatch = Stopwatch.StartNew();

            msg = string.Format("[==TimeCalc_START==]{0} at [{1}]", ItemName, System.DateTime.Now.ToString(ConstVC.Display.StatusInfoDateTimeFormat));

            return msg;
        }

        public static string TimeCalc_STOP()
        {
            string msg = null;

            myCalcWatch.Stop();
            TimeSpan TimeSpent = myCalcWatch.Elapsed;

            string strTimeSpent = string.Format("Elaped time:[{0:00}:{1:00}:{2:00}:{3:000000}]", TimeSpent.Hours, TimeSpent.Minutes, TimeSpent.Seconds, TimeSpent.Milliseconds);
            msg = string.Format("[==TimeCalc_STOP==]{0} at :[{1}],{2}",ItemName, System.DateTime.Now.ToString(ConstVC.Display.StatusInfoDateTimeFormat), strTimeSpent);

            ItemName = null;

            return msg;
        }
    }
}
