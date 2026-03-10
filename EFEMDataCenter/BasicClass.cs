using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;

namespace EFEM.DataCenter
{
    public class SyncEvent
    {
        public AutoResetEvent _event = new AutoResetEvent(false);
        public bool _isWaitingResponse = false;
        public byte[] _buf = new byte[1100];
        public int _length = 0;

        public SyncEvent() { }

        virtual public void Prepare()
        {
            _event.Reset();
            _isWaitingResponse = true;
        }

        virtual public void Abort()
        {
            _isWaitingResponse = false;
        }

        virtual public void Set()
        {
            _event.Set();
        }

        virtual public bool WaitOne(int millisecondsTimeout)
        {
            return _event.WaitOne(millisecondsTimeout, false);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HMIDescriptionAttribute : Attribute
    {
        private string _representation;
        public HMIDescriptionAttribute(string representation)
        {
            this._representation = representation;
        }
        public override string ToString()
        {
            return this._representation;
        }

        public static string GetDescription(object enumValue)
        {
            MemberInfo[] members = enumValue.GetType().GetMember(enumValue.ToString());
            return (GetMemberString(members));
        }

        private static string GetMemberString(MemberInfo[] members)
        {
            if (members != null && members.Length > 0)
            {
                object[] attributes = members[0].GetCustomAttributes(typeof(HMIDescriptionAttribute), false);
                if (attributes.Length > 0)
                    return ((HMIDescriptionAttribute)attributes[0]).ToString();
                else
                    return null;
            }
            else
                return null;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct,
                       AllowMultiple = true)]
    public class HMIAuthor : System.Attribute
    {
        private string name;
        public double version;
        public string Remark;

        public HMIAuthor(string name)
        {
            this.name = name;
            version = 1.0;
            Remark = "";
        }
    }
}
