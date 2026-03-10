using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TDKLogUtility.Module
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LogStringAttribute : Attribute
    {
        public string Representation;
        internal LogStringAttribute(string representation)
        {
            this.Representation = representation;
        }
        public override string ToString()
        {
            return this.Representation;
        }

        public static string GetLogHeadString(LogHeadType logHeadType, string Remark = null)
        {
            MemberInfo[] members = typeof(LogHeadType).GetMember(logHeadType.ToString());
            return (GetMemberString(members, Remark));
        }

        public static string GetLogHeadString(LogCateType logCateType)
        {
            MemberInfo[] members = typeof(LogCateType).GetMember(logCateType.ToString());
            return (GetMemberString(members, null));
        }

        private static string GetMemberString(MemberInfo[] members, string Remark = null)
        {
            if (members != null && members.Length > 0)
            {
                object[] attributes = members[0].GetCustomAttributes(typeof(LogStringAttribute), false);
                if (attributes.Length > 0)
                {
                    if (string.IsNullOrWhiteSpace(Remark))
                        return ("[" + (LogStringAttribute)attributes[0]).ToString() + "]";
                    else
                        return ("[" + (LogStringAttribute)attributes[0]).ToString() + ": " + Remark + "]";
                }
                else
                {
                    return "";
                }
            }
            else
                return "";
        }
    }


    #region LogHeadType
    public enum LogHeadType
    {
        [LogString("")]
        None = 0,
        [LogString("-------------------- NEW START -------------------")]
        System_NewStart,
        [LogString("--------------------- RESTART --------------------")]
        System_Restart,
        [LogString("---------------------- RESET ---------------------")]
        System_Reset,
        [LogString("INIT")]
        Initialize,
        [LogString("EQUIPMENT INITIATED")]
        Initialize_Equipment,
        [LogString("USER INITIATED")]
        Initialize_User,
        [LogString("INFO")]
        Info,
        [LogString("ERROR")]
        Error,
        [LogString("WARNING")]
        Warning,
        [LogString("ALARM")]
        Alarm,
        [LogString("EXCEPTION")]
        Exception,
        [LogString("EXCEPTION SET")]
        Exception_Set,
        [LogString("EXCEPTION CLEAR")]
        Exception_Clear,
        [LogString("EX_NOTIFY")]
        Exception_Notify,
        [LogString("EX_WARNING")]
        Exception_Warning,
        [LogString("EX_ERROR")]
        Exception_Error,
        [LogString("EX_ALARM")]
        Exception_Alarm,
        [LogString("NOTIFY")]
        Notify,
        [LogString("KEEP EYES ON")]
        KeepEyesOn,
        [LogString("ABORT")]
        Abort,
        [LogString("EVENT")]
        Event,
        [LogString("METHOD")]
        MethodEnter,
        [LogString("END_METHOD")]
        MethodExit,
        [LogString("CALL")]
        CallStart,
        [LogString("END_CALL")]
        CallEnd,
        [LogString("REGION")]
        RegionStart,
        [LogString("END_REGION")]
        RegionEnd,
        [LogString("DATA")]
        Data,
        [LogString("DATA RECEIVED")]
        Data_Received,
        [LogString("DATA SENT")]
        Data_Sent,
        [LogString("*SECURED")]
        Secured,
    }
    #endregion LogHeadType

    #region LogCategoryType
    public enum LogCateType
    {
        [LogString("")]
        None = 0,
        [LogString("Alignment")]
        Alignment,
        [LogString("Calibrate")]
        Calibrate,
        [LogString("Inspection")]
        Inspection,
        [LogString("LSInspection")]
        Inspection_LS,
        [LogString("CSInspection")]
        Inspection_CS,
        [LogString("HSInspection")]
        Inspection_HS,
        [LogString("HAInspection")]
        Inspection_HA,
        [LogString("VSInspection")]
        Inspection_VS,
        [LogString("Monitor")]
        MONITOR,
        [LogString("HS_ADRC")]
        Inspection_HS_ADRC,
        [LogString("HS_ADRK")]
        Inspection_HS_ADRK,
        [LogString("TestRun")]
        TestRun,
        [LogString("PWPRun")]
        PWPRun,
        [LogString("Review")]
        Review,
        [LogString("AutoBC")]
        Auto_BC,
        [LogString("AutoFocus")]
        Auto_Focus,
        [LogString("LocalFocus")]
        Local_Focus,
        [LogString("AutoStigmator")]
        Auto_Stigmator,
        [LogString("FocusTracking")]
        Focus_Tracking,
        [LogString("SM")]
        System_Monitoring,
        [LogString("MM")]
        Machine_Matching,
        [LogString("PreCharge")]
        Pre_Charging,
        [LogString("ID")]
        Image_Display,
        [LogString("IC")]
        Image_Computer,
        [LogString("PatternMatch")]
        Pattern_Matching,
        [LogString("Recipe")]
        Recipe,
        [LogString("TDK")]
        TDK,
    }
    #endregion LogCategoryType
}
