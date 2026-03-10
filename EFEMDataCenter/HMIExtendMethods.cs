using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;


namespace EFEM.DataCenter
{
    public static class ExtendMethods
    {
        public static string ToStringHelper(object value, string separator = ", ")
        {
            if (value == null)
                return "<NULL>";
            else if (value is Array || value is ArrayList)
            {
                string[] str = ((System.Collections.IEnumerable)value)
                      .Cast<object>()
                      .Select(x => x.ToString())
                      .ToArray();

                return string.Join(separator, str);
            }
            else if (value is Enum)
            {
                HMIDescriptionAttribute att = GetAttribute<HMIDescriptionAttribute>(value as Enum);
                return (att == null ? value.ToString() : att.ToString());
            }
            else if (value is IDictionary)
            {
                IDictionary idic = (IDictionary)value;
                string[] str = new string[idic.Count];

                int i = 0;
                foreach (var index in idic.Keys)
                    str[i++] = "#"+index.ToString()+"-"+idic[index].ToString();

                return string.Join(separator, str);
            }

            return value.ToString();
        }

        public static bool IsNumeric(this object x) 
        { 
            return (x == null ? false : IsNumeric(x.GetType())); 
        }

        public static bool IsNumeric(Type type) 
        { 
            return IsNumeric(type, Type.GetTypeCode(type)); 
        }

        public static bool IsNumeric(Type type, TypeCode typeCode) 
        { 
            return (typeCode == TypeCode.Decimal || (type.IsPrimitive && typeCode != TypeCode.Object && typeCode != TypeCode.Boolean && typeCode != TypeCode.Char)); 
        }

        public static void AppendText(this RichTextBox box, string text, System.Drawing.Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public static void StretchLastColumn(this DataGridView dataGridView)
        {
            var lastColIndex = dataGridView.Columns.Count - 1;
            var lastCol = dataGridView.Columns[lastColIndex];
            lastCol.Frozen = false;
            lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        public static void StretchColumn(this DataGridView dataGridView, string columnName)
        {
            var selCol = dataGridView.Columns[columnName];
            if (selCol != null)
            {
                selCol.Frozen = false;
                selCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        public static string HexToBinString(string value, int bitLength)
        {
            return Convert.ToString(Convert.ToInt32(value, 16), 2).PadLeft(bitLength, '0');
        }

        public static byte[] StringToBytes(string str)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            return bytes;
        }

        public static string BytesToString(byte[] bytes, int startIdx , int len)
        {
            string str = System.Text.Encoding.Default.GetString(bytes, startIdx, len);
            
            return str;           
        }

        public static int ConvertHex(byte[] byarrA, int iStartPos, int iMaxLength, ref int iActualLength)
        {
            int iRes = 0, i = iStartPos;
            if (byarrA[i] == ' ')
                i++;
            if (byarrA[i] == '0' && (byarrA[i + 1] == 'x' || byarrA[i + 1] == 'X'))
                i += 2;
            for (int j = 0; j < iMaxLength && (byarrA[i] != 0 && byarrA[i] != ' ' && byarrA[i] != '/' && byarrA[i] != 0xd); j++, i++)
            {
                if (byarrA[i] >= '0' && byarrA[i] <= '9')
                    iRes = (iRes << 4) + byarrA[i] - '0';
                else
                    if (byarrA[i] >= 'a' && byarrA[i] <= 'f')
                        iRes = (iRes << 4) + 10 + byarrA[i] - 'a';
                    else
                        if (byarrA[i] >= 'A' && byarrA[i] <= 'F')
                            iRes = (iRes << 4) + 10 + byarrA[i] - 'A';
                        else
                        {
                            iActualLength = i - iStartPos;
                            return -1;//wrong data quit
                        }
            }
            iActualLength = i - iStartPos;
            return iRes;
        }

        public static string ConvertCommByteToString(string head, byte[] byPtBuf, int length)
        {
            string msg = null;
            
            if(head != null && head.Length>0)
                msg = head + " ";

            int len = length;
            if (len > 1024)
                len = 1024;
            for (int i = 0; i < len; i++)
            {
                string cc;
                if ((byPtBuf[i] >= (byte)33 && byPtBuf[i] <= (byte)122) || byPtBuf[i] == (byte)0x20)
                    cc = string.Format("{0}", (char)byPtBuf[i]);
                else
                    cc = string.Format("({0})", byPtBuf[i].ToString("X"));
                msg = string.Format("{0}{1}", msg, cc);
            }
            string aa = string.Format("  Length:{0}", length);
            msg = string.Format("{0}{1}", msg, aa);

            return msg;
        }

        public static T GetAttribute<T>(Enum enumValue) where T : Attribute
        {
            T attribute;
            MemberInfo memberInfo = enumValue.GetType().GetMember(enumValue.ToString())
                                            .FirstOrDefault();

            if (memberInfo != null)
            {
                attribute = (T)memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                return attribute;
            }
            return null;
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }


        //Sample Code: 
        // string demo = "R1";
        // RobotCode rst = demo.ToEnum<RobotCode>();
        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true, TEnum defaultValue = default(TEnum)) 
            where TEnum : struct,  IComparable, IFormattable, IConvertible
        {
            if (!typeof(TEnum).IsEnum) 
            { 
                throw new ArgumentException("TEnum must be an enumerated type"); 
            }
            if (string.IsNullOrEmpty(value)) 
            { 
                return defaultValue; 
            }
            TEnum lResult;
            if (Enum.TryParse(value, ignoreCase, out lResult)) 
                return lResult;
            else
                return defaultValue;
        }

        public static TEnum GetMaxValue<TEnum>() 
            where TEnum : IComparable, IConvertible, IFormattable
        {
            Type type = typeof(TEnum);

            if (!type.IsSubclassOf(typeof(Enum)))
                throw new
                    InvalidCastException
                        ("Cannot cast '" + type.FullName + "' to System.Enum.");

            return (TEnum)Enum.ToObject(type, Enum.GetValues(type).Cast<int>().Max());
        }

        public static int MaxValue(this Enum theEnum)
        {
            Type type = theEnum.GetType();

            if (!type.IsSubclassOf(typeof(Enum)))
                return -1;

            return Enum.GetValues(type).Cast<int>().Max();
        }

        #region extend methods for string builder
        public static bool Contains(this StringBuilder haystack, string needle)
        {
            return haystack.IndexOf(needle) != -1;
        }

        public static string Substring(this StringBuilder theStringBuilder, int startIndex, int length)
        {
            return theStringBuilder == null ? null : theStringBuilder.ToString(startIndex, length);
        }

        public static int IndexOf(this StringBuilder haystack, string needle)
        {
            if (haystack == null || needle == null)
                throw new ArgumentNullException();
            if (needle.Length == 0)
                return 0;//empty strings are everywhere!
            if (needle.Length == 1)//can't beat just spinning through for it
            {
                char c = needle[0];
                for (int idx = 0; idx != haystack.Length; ++idx)
                    if (haystack[idx] == c)
                        return idx;
                return -1;
            }
            int m = 0;
            int i = 0;
            int[] T = KMPTable(needle);
            while (m + i < haystack.Length)
            {
                if (needle[i] == haystack[m + i])
                {
                    if (i == needle.Length - 1)
                        return m == needle.Length ? -1 : m;//match -1 = failure to find conventional in .NET
                    ++i;
                }
                else
                {
                    m = m + i - T[i];
                    i = T[i] > -1 ? T[i] : 0;
                }
            }
            return -1;
        }

        private static int[] KMPTable(string sought)
        {
            int[] table = new int[sought.Length];
            int pos = 2;
            int cnd = 0;
            table[0] = -1;
            table[1] = 0;
            while (pos < table.Length)
                if (sought[pos - 1] == sought[cnd])
                    table[pos++] = ++cnd;
                else if (cnd > 0)
                    cnd = table[cnd];
                else
                    table[pos++] = 0;
            return table;
        }
        #endregion

    }


}
