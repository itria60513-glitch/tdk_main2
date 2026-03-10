using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Reflection;

namespace EFEM.GUIControls
{
    public class IPAddressControl : System.Windows.Forms.Control
    {
        public const int FieldCount = 4;

        #region Private Data

        private FieldControl[] _fieldControls = new FieldControl[FieldCount];
        private DotControl[] _dotControls = new DotControl[FieldCount - 1];

        private bool _autoHeight = true;
        private BorderStyle _borderStyle = BorderStyle.Fixed3D;
        private bool _focused;
        private bool _readOnly;

        private bool _hasMouse;

        #endregion

        #region Public Events

        public event FieldChangedEventHandler FieldChangedEvent;

        #endregion

        #region Public Properies

        [Browsable(true)]
        public bool AllowInternalTab
        {
            get
            {
                foreach (FieldControl fc in _fieldControls)
                {
                    return fc.TabStop;
                }

                return false;
            }
            set
            {
                foreach (FieldControl fc in _fieldControls)
                {
                    fc.TabStop = value;
                }
            }
        }

        [Browsable(true)]
        public bool AnyBlank
        {
            get
            {
                foreach (FieldControl fc in _fieldControls)
                {
                    if (fc.Blank)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(true)]
        public bool AutoHeight
        {
            get
            {
                return _autoHeight;
            }
            set
            {
                _autoHeight = value;
                if (_autoHeight)
                {
                    AdjustSize();
                }
            }
        }

        [Browsable(true)]
        public bool Blank
        {
            get
            {
                foreach (FieldControl fc in _fieldControls)
                {
                    if (!fc.Blank)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [Browsable(true)]
        public BorderStyle BorderStyle
        {
            get
            {
                return _borderStyle;
            }
            set
            {
                _borderStyle = value;
                foreach (DotControl dc in _dotControls)
                {
                    dc.IgnoreTheme = (value != BorderStyle.Fixed3D);
                }
                LayoutControls();
                Invalidate();
            }
        }

        [Browsable(false)]
        public override bool Focused
        {
            get
            {
                foreach (FieldControl fc in _fieldControls)
                {
                    if (fc.Focused)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(true)]
        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;

                foreach (FieldControl fc in _fieldControls)
                {
                    fc.ReadOnly = _readOnly;
                }

                foreach (DotControl dc in _dotControls)
                {
                    dc.ReadOnly = _readOnly;
                }

                Invalidate();
            }
        }

        [Bindable(true), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder(); ;

                try
                {
                    for (int index = 0; index < _fieldControls.Length; ++index)
                    {
                        sb.Append(_fieldControls[index].Text);

                        if (index < _dotControls.Length)
                        {
                            sb.Append(_dotControls[index].Text);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                }

                return sb.ToString();
            }
            set
            {
                Parse(value);
            }
        }

        #endregion

        #region Public Functions

        public void Clear()
        {
            foreach (FieldControl fc in _fieldControls)
            {
                fc.Clear();
            }
        }

        public byte[] GetAddressBytes()
        {
            Byte[] bytes = new Byte[_fieldControls.Length];

            for (int index = 0; index < bytes.Length; ++index)
            {
                if (_fieldControls[index].TextLength > 0)
                {
                    bytes[index] = Convert.ToByte(_fieldControls[index].Text, CultureInfo.InvariantCulture);
                }
                else
                {
                    bytes[index] = (byte)(_fieldControls[index].RangeLower);
                }
            }

            return bytes;
        }

        public bool IsAddressValid()
        {
            for (int index = 0; index < _fieldControls.Length; ++index)
            {
                if (_fieldControls[index].TextLength == 0)
                    return false;
            }

            return true;
        }

        public void SetAddressBytes(Byte[] value)
        {
            Clear();

            if (value == null)
            {
                return;
            }

            int length = Math.Min(_fieldControls.Length, value.Length);

            for (int i = 0; i < length; ++i)
            {
                _fieldControls[i].Text = value[i].ToString(CultureInfo.InvariantCulture);
            }
        }

        public void SetFieldFocus(int field)
        {
            if ((field >= 0) && (field < _fieldControls.Length))
            {
                _fieldControls[field].TakeFocus(Direction.Forward, Selection.All);
            }
        }

        public void SetFieldRange(int field, int rangeLower, int rangeUpper)
        {
            if ((field >= 0) && (field < _fieldControls.Length))
            {
                _fieldControls[field].RangeLower = rangeLower;
                _fieldControls[field].RangeUpper = rangeUpper;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                for (int index = 0; index < _fieldControls.Length; ++index)
                {
                    sb.Append(_fieldControls[index].ToString());

                    if (index < _dotControls.Length)
                    {
                        sb.Append(_dotControls[index].ToString());
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            return sb.ToString();
        }

        #endregion

        #region Constructors

        public IPAddressControl()
        {
            BackColor = Color.FromKnownColor(KnownColor.Window);

            for (int index = 0; index < _fieldControls.Length; ++index)
            {
                _fieldControls[index] = new FieldControl();

                _fieldControls[index].CreateControl();

                _fieldControls[index].Name = "fieldControl" + index.ToString(CultureInfo.InvariantCulture);
                _fieldControls[index].Parent = this;
                _fieldControls[index].FieldIndex = index;

                _fieldControls[index].CedeFocusEvent += new CedeFocusHandler(OnCedeFocus);
                _fieldControls[index].Click += new EventHandler(OnSubControlClicked);
                _fieldControls[index].DoubleClick += new EventHandler(OnSubControlDoubleClicked);
                _fieldControls[index].GotFocus += new EventHandler(OnFieldGotFocus);
                _fieldControls[index].KeyDown += new KeyEventHandler(OnFieldKeyDown);
                _fieldControls[index].KeyPress += new KeyPressEventHandler(OnFieldKeyPressed);
                _fieldControls[index].KeyUp += new KeyEventHandler(OnFieldKeyUp);
                _fieldControls[index].LostFocus += new EventHandler(OnFieldLostFocus);
                _fieldControls[index].MouseEnter += new EventHandler(OnSubControlMouseEntered);
                _fieldControls[index].MouseHover += new EventHandler(OnSubControlMouseHovered);
                _fieldControls[index].MouseLeave += new EventHandler(OnSubControlMouseLeft);
                _fieldControls[index].MouseMove += new MouseEventHandler(OnSubControlMouseMoved);
                _fieldControls[index].TextChangedEvent += new TextChangedHandler(OnFieldTextChanged);

                Controls.Add(_fieldControls[index]);
            }

            for (int index = 0; index < _dotControls.Length; ++index)
            {
                _dotControls[index] = new DotControl();

                _dotControls[index].CreateControl();

                _dotControls[index].Name = "dotControl" + index.ToString(CultureInfo.InvariantCulture);
                _dotControls[index].Parent = this;
                _dotControls[index].IgnoreTheme = (BorderStyle != BorderStyle.Fixed3D);

                _dotControls[index].Click += new EventHandler(OnSubControlClicked);
                _dotControls[index].DoubleClick += new EventHandler(OnSubControlDoubleClicked);
                _dotControls[index].MouseEnter += new EventHandler(OnSubControlMouseEntered);
                _dotControls[index].MouseHover += new EventHandler(OnSubControlMouseHovered);
                _dotControls[index].MouseLeave += new EventHandler(OnSubControlMouseLeft);
                _dotControls[index].MouseMove += new MouseEventHandler(OnSubControlMouseMoved);

                Controls.Add(_dotControls[index]);
            }

            Cursor = Cursors.IBeam;
            Size = MinimumSize;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            LayoutControls();
        }

        #endregion

        #region Protected Overrides

        protected override void OnBackColorChanged(EventArgs e)
        {
            foreach (FieldControl fc in _fieldControls)
            {
                if (fc != null)
                {
                    fc.BackColor = BackColor;
                }
            }

            foreach (DotControl dc in _dotControls)
            {
                if (dc != null)
                {
                    dc.BackColor = BackColor;
                    dc.Invalidate();
                }
            }

            base.OnBackColorChanged(e);

            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            foreach (FieldControl fc in _fieldControls)
            {
                fc.SetFont(Font);
            }

            foreach (DotControl dc in _dotControls)
            {
                dc.SetFont(Font);
            }

            AdjustSize();
            LayoutControls();

            base.OnFontChanged(e);

            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            foreach (FieldControl fc in _fieldControls)
            {
                fc.ForeColor = ForeColor;
            }

            foreach (DotControl dc in _dotControls)
            {
                dc.ForeColor = ForeColor;
            }

            base.OnForeColorChanged(e);

            Invalidate(true);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _focused = true;
            _fieldControls[0].TakeFocus(Direction.Forward, Selection.All);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!Focused)
            {
                _focused = false;
                base.OnLostFocus(e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_hasMouse)
            {
                _hasMouse = true;
                base.OnMouseEnter(e);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!HasMouse)
            {
                base.OnMouseLeave(e);
                _hasMouse = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool themed = NativeMethods.IsThemed();

            if (DesignMode || !themed || (themed && BorderStyle != BorderStyle.Fixed3D))
            {
                OnPaintStandard(e);
            }
            else
            {
                OnPaintThemed(e);
            }

            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            LayoutControls();
            base.OnSizeChanged(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_WINDOWPOSCHANGING:

                    NativeMethods.WINDOWPOS lParam = (NativeMethods.WINDOWPOS)m.GetLParam(typeof(NativeMethods.WINDOWPOS));

                    if (lParam.cx < MinimumSize.Width)
                    {
                        lParam.flags |= NativeMethods.SWP_NOMOVE;
                        lParam.cx = MinimumSize.Width;
                    }

                    if (lParam.cy < MinimumSize.Height)
                    {
                        lParam.flags |= NativeMethods.SWP_NOMOVE;
                        lParam.cy = MinimumSize.Height;
                    }

                    if (AutoHeight && lParam.cy != MinimumSize.Height)
                    {
                        lParam.flags |= NativeMethods.SWP_NOMOVE;
                        lParam.cy = MinimumSize.Height;
                    }

                    Marshal.StructureToPtr(lParam, m.LParam, true);

                    break;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Private Properties

        private bool HasMouse
        {
            get
            {
                return DisplayRectangle.Contains(PointToClient(MousePosition));
            }
        }

        private Size MinimumSize
        {
            get
            {
                Size retVal = new Size(0, 0);

                foreach (FieldControl fc in _fieldControls)
                {
                    retVal.Width += fc.Width;
                    retVal.Height = Math.Max(retVal.Height, fc.Height);
                }

                foreach (DotControl dc in _dotControls)
                {
                    retVal.Width += dc.Width;
                    retVal.Height = Math.Max(retVal.Height, dc.Height);
                }

                switch (BorderStyle)
                {
                    case BorderStyle.Fixed3D:
                        retVal.Width += 6;
                        retVal.Height += 7;
                        break;
                    case BorderStyle.FixedSingle:
                        retVal.Width += 4;
                        retVal.Height += 7;
                        break;
                }

                return retVal;
            }
        }

        #endregion

        #region Private Functions

        private void AdjustSize()
        {
            Size newSize = MinimumSize;

            if (Width > newSize.Width)
            {
                newSize = new Size(Width, newSize.Height);
            }

            if (Height > newSize.Height)
            {
                newSize = new Size(Height, newSize.Width);
            }

            if (AutoHeight)
            {
                Size = new Size(newSize.Width, MinimumSize.Height);
            }
            else
            {
                Size = newSize;
            }
        }

        private void LayoutControls()
        {
            SuspendLayout();

            int difference = Width - MinimumSize.Width;

            //if (difference <= 0)
            //    difference = 1;
            //Debug.Assert(difference >= 0);

            int numOffsets = _fieldControls.Length + _dotControls.Length + 1;

            int div = difference / (numOffsets);
            int mod = difference % (numOffsets);

            int[] offsets = new int[numOffsets];

            for (int index = 0; index < numOffsets; ++index)
            {
                offsets[index] = div;

                if (index < mod)
                {
                    ++offsets[index];
                }
            }

            int x = 0;
            int y = 0;

            switch (BorderStyle)
            {
                case BorderStyle.Fixed3D:
                    x = 3;
                    y = 3;
                    break;
                case BorderStyle.FixedSingle:
                    x = 2;
                    y = 2;
                    break;
            }

            int offsetIndex = 0;

            x += offsets[offsetIndex++];

            for (int i = 0; i < _fieldControls.Length; ++i)
            {
                _fieldControls[i].Location = new Point(x, y);

                x += _fieldControls[i].Width;

                if (i < _dotControls.Length)
                {
                    x += offsets[offsetIndex++];
                    _dotControls[i].Location = new Point(x, y);
                    x += _dotControls[i].Width;
                    x += offsets[offsetIndex++];
                }
            }

            ResumeLayout(false);
        }

        private void OnCedeFocus(int fieldIndex, Direction direction, Selection selection, Action action)
        {
            switch (action)
            {
                case Action.Home:
                    _fieldControls[0].TakeFocus(Action.Home);
                    return;

                case Action.End:
                    _fieldControls[FieldCount - 1].TakeFocus(Action.End);
                    return;

                case Action.Trim:
                    if (fieldIndex == 0)
                    {
                        return;
                    }
                    _fieldControls[fieldIndex - 1].TakeFocus(Action.Trim);
                    return;
            }

            if ((direction == Direction.Reverse && fieldIndex == 0) ||
                 (direction == Direction.Forward && fieldIndex == (FieldCount - 1)))
            {
                return;
            }

            if (direction == Direction.Forward)
            {
                ++fieldIndex;
            }
            else
            {
                --fieldIndex;
            }

            _fieldControls[fieldIndex].TakeFocus(direction, selection);
        }

        private void OnFieldGotFocus(object sender, EventArgs e)
        {
            if (!_focused)
            {
                _focused = true;
                base.OnGotFocus(e);
            }
        }

        private void OnFieldLostFocus(object sender, EventArgs e)
        {
            if (!Focused)
            {
                base.OnLostFocus(e);
                _focused = false;
            }
        }

        private void OnFieldKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void OnFieldKeyPressed(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void OnFieldKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void OnFieldTextChanged(int fieldIndex, string text)
        {
            if (FieldChangedEvent != null)
            {
                FieldChanged_EventArgs args = new FieldChanged_EventArgs();
                args.FieldIndex = fieldIndex;
                args.Text = text;
                FieldChangedEvent(this, args);
            }

            OnTextChanged(EventArgs.Empty);
        }

        private void OnPaintStandard(PaintEventArgs e)
        {
            SolidBrush ctrlBrush = null;

            if (Enabled)
            {
                if (ReadOnly)
                {
                    if (BackColor.ToKnownColor() == KnownColor.Window)
                    {
                        ctrlBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
                    }
                    else
                    {
                        ctrlBrush = new SolidBrush(BackColor);
                    }
                }
                else
                {
                    ctrlBrush = new SolidBrush(BackColor);
                }
            }
            else
            {
                if (BackColor.ToKnownColor() == KnownColor.Window)
                {
                    ctrlBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
                }
                else
                {
                    ctrlBrush = new SolidBrush(BackColor);
                }
            }

            using (ctrlBrush)
            {
                e.Graphics.FillRectangle(ctrlBrush, ClientRectangle);
            }

            switch (BorderStyle)
            {
                case BorderStyle.Fixed3D:
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
                    break;
                case BorderStyle.FixedSingle:
                    ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                       Color.FromKnownColor(KnownColor.WindowFrame), ButtonBorderStyle.Solid);
                    break;
            }
        }

        private void OnPaintThemed(PaintEventArgs e)
        {
            NativeMethods.RECT rect = new NativeMethods.RECT();

            rect.left = ClientRectangle.Left;
            rect.top = ClientRectangle.Top;
            rect.right = ClientRectangle.Right;
            rect.bottom = ClientRectangle.Bottom;

            IntPtr hdc = new IntPtr();
            hdc = e.Graphics.GetHdc();

            if (BackColor.ToKnownColor() != KnownColor.Window)
            {
                e.Graphics.ReleaseHdc(hdc);

                using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);
                }

                hdc = e.Graphics.GetHdc();

                IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Edit");

                NativeMethods.DTBGOPTS options = new NativeMethods.DTBGOPTS();
                options.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(options);
                options.dwFlags = NativeMethods.DTBG_OMITCONTENT;

                int state = NativeMethods.ETS_NORMAL;

                if (!Enabled)
                {
                    state = NativeMethods.ETS_DISABLED;
                }
                else
                    if (ReadOnly)
                    {
                        state = NativeMethods.ETS_READONLY;
                    }

                NativeMethods.DrawThemeBackgroundEx(hTheme, hdc,
                   NativeMethods.EP_EDITTEXT, state, ref rect, ref options);

                if (IntPtr.Zero != hTheme)
                {
                    NativeMethods.CloseThemeData(hTheme);
                }
            }
            else if (Enabled & !ReadOnly)
            {
                IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Edit");

                NativeMethods.DrawThemeBackground(hTheme, hdc, NativeMethods.EP_EDITTEXT,
                   NativeMethods.ETS_NORMAL, ref rect, IntPtr.Zero);

                if (IntPtr.Zero != hTheme)
                {
                    NativeMethods.CloseThemeData(hTheme);
                }
            }
            else
            {
                IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Globals");

                IntPtr hBrush = NativeMethods.GetThemeSysColorBrush(hTheme, 15);

                NativeMethods.FillRect(hdc, ref rect, hBrush);

                if (IntPtr.Zero != hBrush)
                {
                    NativeMethods.DeleteObject(hBrush);
                    hBrush = IntPtr.Zero;
                }

                if (IntPtr.Zero != hTheme)
                {
                    NativeMethods.CloseThemeData(hTheme);
                    hTheme = IntPtr.Zero;
                }

                hTheme = NativeMethods.OpenThemeData(Handle, "Edit");

                NativeMethods.DTBGOPTS options = new NativeMethods.DTBGOPTS();
                options.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(options);
                options.dwFlags = NativeMethods.DTBG_OMITCONTENT;

                NativeMethods.DrawThemeBackgroundEx(hTheme, hdc,
                   NativeMethods.EP_EDITTEXT, NativeMethods.ETS_DISABLED, ref rect, ref options);

                if (IntPtr.Zero != hTheme)
                {
                    NativeMethods.CloseThemeData(hTheme);
                }
            }

            e.Graphics.ReleaseHdc(hdc);
        }

        private void Parse(string text)
        {
            Clear();

            if (null == text)
            {
                return;
            }

            int textIndex = 0;

            int index = 0;

            for (index = 0; index < _dotControls.Length; ++index)
            {
                int findIndex = text.IndexOf(_dotControls[index].Text, textIndex);

                if (findIndex >= 0)
                {
                    _fieldControls[index].Text = text.Substring(textIndex, findIndex - textIndex);
                    textIndex = findIndex + _dotControls[index].Text.Length;
                }
                else
                {
                    break;
                }
            }

            _fieldControls[index].Text = text.Substring(textIndex);
        }

        private void OnSubControlClicked(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void OnSubControlDoubleClicked(object sender, EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void OnSubControlMouseEntered(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void OnSubControlMouseHovered(object sender, EventArgs e)
        {
            OnMouseHover(e);
        }

        private void OnSubControlMouseLeft(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void OnSubControlMouseMoved(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        #endregion
    }

    internal delegate void CedeFocusHandler(int fieldIndex, Direction direction, Selection selection, Action action);
    internal delegate void TextChangedHandler(int fieldIndex, string newText);

    internal class NativeMethods
    {
        private NativeMethods()
        {
        }

        // GDI-related

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        public static int GetRValue(uint colorref)
        {
            return (int)(colorref & 0xff);
        }

        public static int GetGValue(uint colorref)
        {
            return (int)((colorref >> 8) & 0xff);
        }

        public static int GetBValue(uint colorref)
        {
            return (int)((colorref >> 16) & 0xff);
        }

        public static uint RGB(int r, int g, int b)
        {
            return (uint)(((uint)r) | (((uint)g) << 8) | (((uint)b) << 16));
        }

        // ComCtl-related

        [DllImport("comctl32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint DllGetVersion(ref DLLVERSIONINFO pdvi);

        // Theme-related

        public const int EP_EDITTEXT = 1;
        public const int EP_CARET = 2;

        public const int ETS_NORMAL = 1;
        public const int ETS_HOT = 2;
        public const int ETS_SELECTED = 3;
        public const int ETS_DISABLED = 4;
        public const int ETS_FOCUSED = 5;
        public const int ETS_READONLY = 6;
        public const int ETS_ASSIST = 7;

        public const uint DTBG_CLIPRECT = 0x00000001;
        public const uint DTBG_DRAWSOLID = 0x00000002;
        public const uint DTBG_OMITBORDER = 0x00000004;
        public const uint DTBG_OMITCONTENT = 0x00000008;
        public const uint DTBG_COMPUTINGREGION = 0x00000010;
        public const uint DTBG_MIRRORDC = 0x00000020;

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int CloseThemeData(IntPtr hTheme);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId,
           int iStateId, ref RECT pRect, IntPtr pClipRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DrawThemeBackgroundEx(IntPtr hTheme, IntPtr hdc, int iPartId,
           int iStateId, ref RECT pRect, ref DTBGOPTS pOptions);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetThemeSysColor(IntPtr hTheme, int iColorID);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetThemeSysColorBrush(IntPtr hTheme, int iColorID);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsAppThemed();

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsThemeActive();

        [DllImport("uxtheme", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenThemeData(IntPtr hWnd, string lpString);

        public static bool IsThemed()
        {
            bool retval = false;

            if (Environment.OSVersion.Version.Major >= 5 &&
                 Environment.OSVersion.Version.Minor >= 1)
            {
                bool appThemed = NativeMethods.IsAppThemed();
                bool themeActive = NativeMethods.IsThemeActive();

                if (appThemed && themeActive)
                {
                    DLLVERSIONINFO dvi = new DLLVERSIONINFO();
                    dvi.cbSize = (uint)Marshal.SizeOf(dvi);

                    NativeMethods.DllGetVersion(ref dvi);

                    retval = (dvi.dwMajorVersion >= 6);
                }
            }

            return retval;
        }

        // User-related

        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_CONTEXTMENU = 0x007b;

        public const uint SWP_NOMOVE = 0x0002;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, IntPtr lParam);

        // Win32 structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct DLLVERSIONINFO
        {
            public uint cbSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformID;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DTBGOPTS
        {
            public uint dwSize;
            public uint dwFlags;
            public RECT rcClip;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }
    }

    internal class Utility
    {
        private Utility()
        {
        }

        public static Size CalculateStringSize(IntPtr handle, Font font, string text)
        {
            StringFormat stringFormat = new StringFormat();
            RectangleF rect = new RectangleF(0, 0, 9999, 9999);

            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

            Region[] regions = new Region[1];

            stringFormat.SetMeasurableCharacterRanges(ranges);

            Graphics g = Graphics.FromHwnd(handle);

            regions = g.MeasureCharacterRanges(text,
               font, rect, stringFormat);

            rect = regions[0].GetBounds(g);

            g.Dispose();

            float fudgeFactor = (font.SizeInPoints / 8.25F) * 3.0F;

            return new Size((int)(rect.Width + fudgeFactor), (int)(rect.Height));
        }
    }

    internal class DotControl : System.Windows.Forms.Control
    {
        private bool _readOnly;
        private bool _ignoreTheme;

        public bool IgnoreTheme
        {
            get
            {
                return _ignoreTheme;
            }
            set
            {
                _ignoreTheme = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;
                Invalidate();
            }
        }

        public void SetFont(Font font)
        {
            Font = font;
            Size = CalculateControlSize();
        }

        public override string ToString()
        {
            return Text;
        }

        public DotControl()
        {
            BackColor = Color.FromKnownColor(KnownColor.Window);
            Font = Control.DefaultFont;
            TabStop = false;

            //ResourceManager rm = new ResourceManager("EFEM.GUIControls.Strings", Assembly.GetExecutingAssembly());
            //Text = rm.GetString("FieldSeparator");
            Text = ".";

            Size = CalculateControlSize();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            Size = CalculateControlSize();
            Invalidate();
        }

        private Size CalculateControlSize()
        {
            return Utility.CalculateStringSize(Handle, Font, Text);
        }

        protected void OnPaintStandard(PaintEventArgs e)
        {
            SolidBrush ctrlBrush = null;
            SolidBrush textBrush = null;

            if (Enabled)
            {
                if (ReadOnly)
                {
                    if (BackColor.ToKnownColor() == KnownColor.Window)
                    {
                        ctrlBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
                        textBrush = new SolidBrush(Color.FromKnownColor(KnownColor.WindowText));
                    }
                    else
                    {
                        ctrlBrush = new SolidBrush(BackColor);
                        textBrush = new SolidBrush(ForeColor);
                    }
                }
                else
                {
                    ctrlBrush = new SolidBrush(BackColor);
                    textBrush = new SolidBrush(ForeColor);
                }
            }
            else
            {
                if (BackColor.ToKnownColor() == KnownColor.Window)
                {
                    ctrlBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
                }
                else
                {
                    ctrlBrush = new SolidBrush(BackColor);
                }

                if (ForeColor.ToKnownColor() == KnownColor.Control)
                {
                    textBrush = new SolidBrush(ForeColor);
                }
                else
                {
                    textBrush = new SolidBrush(Color.FromKnownColor(KnownColor.GrayText));
                }
            }

            using (ctrlBrush)
            {
                e.Graphics.FillRectangle(ctrlBrush, ClientRectangle);
            }

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;

            using (textBrush)
            {
                e.Graphics.DrawString(Text, Font, textBrush, ClientRectangle, stringFormat);
            }
        }

        protected void OnPaintThemed(PaintEventArgs e)
        {
            NativeMethods.RECT rect = new NativeMethods.RECT();

            rect.left = ClientRectangle.Left;
            rect.top = ClientRectangle.Top;
            rect.right = ClientRectangle.Right;
            rect.bottom = ClientRectangle.Bottom;

            IntPtr hdc = new IntPtr();
            hdc = e.Graphics.GetHdc();

            if (BackColor.ToKnownColor() != KnownColor.Window)
            {
                e.Graphics.ReleaseHdc(hdc);

                using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);
                }

                hdc = e.Graphics.GetHdc();
            }
            else
                if (Enabled & !ReadOnly)
                {
                    IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Edit");

                    NativeMethods.DTBGOPTS options = new NativeMethods.DTBGOPTS();
                    options.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(options);
                    options.dwFlags = NativeMethods.DTBG_OMITBORDER;

                    NativeMethods.DrawThemeBackgroundEx(hTheme, hdc,
                       NativeMethods.EP_EDITTEXT, NativeMethods.ETS_NORMAL, ref rect, ref options);

                    if (IntPtr.Zero != hTheme)
                    {
                        NativeMethods.CloseThemeData(hTheme);
                    }
                }
                else
                {
                    IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Globals");

                    IntPtr hBrush = NativeMethods.GetThemeSysColorBrush(hTheme, 15);

                    NativeMethods.FillRect(hdc, ref rect, hBrush);

                    if (IntPtr.Zero != hBrush)
                    {
                        NativeMethods.DeleteObject(hBrush);
                        hBrush = IntPtr.Zero;
                    }

                    if (IntPtr.Zero != hTheme)
                    {
                        NativeMethods.CloseThemeData(hTheme);
                        hTheme = IntPtr.Zero;
                    }
                }

            e.Graphics.ReleaseHdc(hdc);

            uint colorref = 0;

            if (Enabled)
            {
                if (ForeColor.ToKnownColor() != KnownColor.WindowText)
                {
                    colorref = NativeMethods.RGB(ForeColor.R, ForeColor.G, ForeColor.B);
                }
                else
                {
                    IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Globals");
                    colorref = NativeMethods.GetThemeSysColor(hTheme, 6);
                    if (IntPtr.Zero != hTheme)
                    {
                        NativeMethods.CloseThemeData(hTheme);
                        hTheme = IntPtr.Zero;
                    }
                }
            }
            else
            {
                IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Globals");
                colorref = NativeMethods.GetThemeSysColor(hTheme, 16);
                if (IntPtr.Zero != hTheme)
                {
                    NativeMethods.CloseThemeData(hTheme);
                    hTheme = IntPtr.Zero;
                }
            }

            int r = NativeMethods.GetRValue(colorref);
            int g = NativeMethods.GetGValue(colorref);
            int b = NativeMethods.GetBValue(colorref);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;

            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(r, g, b)))
            {
                e.Graphics.DrawString(Text, Font, textBrush, ClientRectangle, stringFormat);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool themed = NativeMethods.IsThemed();

            if (DesignMode || !themed || (themed && IgnoreTheme))
            {
                OnPaintStandard(e);
            }
            else
            {
                OnPaintThemed(e);
            }

            base.OnPaint(e);
        }
    }

    internal enum Direction
    {
        Forward,
        Reverse
    };

    internal enum Selection
    {
        None,
        All
    };

    internal enum Action
    {
        None,
        Trim,
        Home,
        End
    }

    internal class FieldControl : System.Windows.Forms.TextBox
    {
        public event CedeFocusHandler CedeFocusEvent;
        public event TextChangedHandler TextChangedEvent;

        public const int MinimumValue = 0;
        public const int MaximumValue = 255;

        #region Private Data

        private int _fieldIndex = -1;

        private bool _invalidKeyDown;

        private int _rangeLower;  // using " = MinimumValue; " here is flagged by FxCop
        private int _rangeUpper = MaximumValue;

        #endregion

        #region Public Properties

        public bool Blank
        {
            get
            {
                if (TextLength > 0)
                {
                    return false;
                }

                return true;
            }
        }

        public int FieldIndex
        {
            get { return _fieldIndex; }
            set { _fieldIndex = value; }
        }

        public int RangeLower
        {
            get { return _rangeLower; }
            set
            {
                if (value < MinimumValue)
                {
                    _rangeLower = MinimumValue;
                }
                else
                    if (value > _rangeUpper)
                    {
                        _rangeLower = _rangeUpper;
                    }
                    else
                    {
                        _rangeLower = value;
                    }

                if (Value < _rangeLower)
                {
                    Text = _rangeLower.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public int RangeUpper
        {
            get { return _rangeUpper; }
            set
            {
                if (value < _rangeLower)
                {
                    _rangeUpper = _rangeLower;
                }
                else
                    if (value > MaximumValue)
                    {
                        _rangeUpper = MaximumValue;
                    }
                    else
                    {
                        _rangeUpper = value;
                    }

                if (Value > _rangeUpper)
                {
                    Text = _rangeUpper.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        #endregion

        #region Public Functions

        public void SetFont(Font font)
        {
            Font = font;
            Size = CalculateControlSize();
        }

        public void TakeFocus(Action action)
        {
            Focus();

            switch (action)
            {
                case Action.Trim:

                    if (TextLength > 0)
                    {
                        int newLength = TextLength - 1;
                        base.Text = Text.Substring(0, newLength);
                    }
                    SelectionStart = TextLength;
                    return;

                case Action.Home:

                    SelectionStart = 0;
                    SelectionLength = 0;
                    return;

                case Action.End:

                    SelectionStart = TextLength;
                    return;
            }
        }

        public void TakeFocus(Direction direction, Selection selection)
        {
            Focus();

            if (selection == Selection.All)
            {
                SelectionStart = 0;
                SelectionLength = TextLength;
            }
            else
            {
                SelectionStart = (direction == Direction.Forward) ? 0 : TextLength;
            }
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Constructor

        public FieldControl()
        {
            AutoSize = false;
            BorderStyle = BorderStyle.None;
            MaxLength = 3;
            TabStop = false;
            TextAlign = HorizontalAlignment.Center;

            Size = CalculateControlSize();
        }

        #endregion

        #region Overrides

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            _invalidKeyDown = false;

            switch (e.KeyCode)
            {
                case Keys.Home:
                    SendCedeFocusEvent(Action.Home);
                    return;

                case Keys.End:
                    SendCedeFocusEvent(Action.End);
                    return;
            }

            if (IsCedeFocusKey(e))
            {
                SendCedeFocusEvent(Direction.Forward, Selection.All);
                _invalidKeyDown = true;
                return;
            }
            else if (IsForwardKey(e))
            {
                if (e.Control)
                {
                    SendCedeFocusEvent(Direction.Forward, Selection.All);
                    return;
                }
                else if (SelectionLength == 0 && SelectionStart == TextLength)
                {
                    SendCedeFocusEvent(Direction.Forward, Selection.None);
                    return;
                }
            }
            else if (IsReverseKey(e))
            {
                if (e.Control)
                {
                    SendCedeFocusEvent(Direction.Reverse, Selection.All);
                    return;
                }
                else if (SelectionLength == 0 && SelectionStart == 0)
                {
                    SendCedeFocusEvent(Direction.Reverse, Selection.None);
                    return;
                }
            }
            else if (IsBackspaceKey(e))
            {
                HandleBackspaceKey();
            }
            else if (!IsNumericKey(e) && !IsEditKey(e) && !IsEnterKey(e))
            {
                _invalidKeyDown = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (_invalidKeyDown)
            {
                e.Handled = true;
                return;
            }

            base.OnKeyPress(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (!Blank)
            {
                try
                {
                    int value = Int32.Parse(Text, CultureInfo.InvariantCulture);

                    if (value > RangeUpper)
                    {
                        base.Text = RangeUpper.ToString(CultureInfo.InvariantCulture);
                        SelectionStart = 0;
                    }
                    else if ((TextLength == MaxLength) && (value < RangeLower))
                    {
                        base.Text = RangeLower.ToString(CultureInfo.InvariantCulture);
                        SelectionStart = 0;
                    }
                    else
                    {
                        int originalLength = TextLength;
                        int newSelectionStart = SelectionStart;

                        base.Text = value.ToString(CultureInfo.InvariantCulture);

                        if (TextLength < originalLength)
                        {
                            newSelectionStart -= (originalLength - TextLength);
                            SelectionStart = Math.Max(0, newSelectionStart);
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                    Text = String.Empty;
                }
                catch (FormatException)
                {
                    Text = String.Empty;
                }
                catch (OverflowException)
                {
                    Text = String.Empty;
                }
            }

            if (TextChangedEvent != null)
            {
                TextChangedEvent(FieldIndex, Text);
            }

            if (TextLength == MaxLength && Focused && SelectionStart == TextLength)
            {
                SendCedeFocusEvent(Direction.Forward, Selection.All);
            }
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            if (!Blank)
            {
                if (Value < RangeLower)
                {
                    Text = RangeLower.ToString(CultureInfo.InvariantCulture);
                }
            }

            base.OnValidating(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_CONTEXTMENU:
                    return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Private Functions

        private Size CalculateControlSize()
        {
            //string text = new string('0', MaxLength);
            string text = new string('0', MaxLength);
            Size size = Utility.CalculateStringSize(Handle, Font, text);
            //size.Width += 5;
            return size;
        }

        private void HandleBackspaceKey()
        {
            if (!ReadOnly && (TextLength == 0 || (SelectionStart == 0 && SelectionLength == 0)))
            {
                SendCedeFocusEvent(Action.Trim);
                _invalidKeyDown = true;
            }
        }

        private static bool IsBackspaceKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                return true;
            }

            return false;
        }

        private bool IsCedeFocusKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.OemPeriod ||
                 e.KeyCode == Keys.Decimal ||
                 e.KeyCode == Keys.Space)
            {
                if (TextLength != 0 && SelectionLength == 0 && SelectionStart != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEditKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back ||
                 e.KeyCode == Keys.Delete)
            {
                return true;
            }
            else if (e.Modifiers == Keys.Control &&
                      (e.KeyCode == Keys.C ||
                        e.KeyCode == Keys.V ||
                        e.KeyCode == Keys.X))
            {
                return true;
            }

            return false;
        }

        private static bool IsEnterKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter ||
                 e.KeyCode == Keys.Return)
            {
                return true;
            }

            return false;
        }

        private static bool IsForwardKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right ||
                 e.KeyCode == Keys.Down)
            {
                return true;
            }

            return false;
        }

        private static bool IsNumericKey(KeyEventArgs e)
        {
            if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
            {
                if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsReverseKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left ||
                 e.KeyCode == Keys.Up)
            {
                return true;
            }

            return false;
        }

        private void SendCedeFocusEvent(Action action)
        {
            if (CedeFocusEvent != null)
            {
                CedeFocusEvent(FieldIndex, Direction.Forward, Selection.None, action);
            }
        }

        private void SendCedeFocusEvent(Direction direction, Selection selection)
        {
            if (CedeFocusEvent != null)
            {
                CedeFocusEvent(FieldIndex, direction, selection, Action.None);
            }
        }

        #endregion

        #region Private Properties

        private int Value
        {
            get
            {
                try
                {
                    return Int32.Parse(Text, CultureInfo.InvariantCulture);
                }
                catch (ArgumentNullException)
                {
                    return RangeLower;
                }
                catch (FormatException)
                {
                    return RangeLower;
                }
                catch (OverflowException)
                {
                    return RangeLower;
                }
            }
        }

        #endregion
    }

    public class FieldChanged_EventArgs : EventArgs
    {
        private int _fieldIndex;
        private String _text;

        public int FieldIndex
        {
            get
            {
                return _fieldIndex;
            }
            set
            {
                _fieldIndex = value;
            }
        }

        public String Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }
    }

    public delegate void FieldChangedEventHandler(object sender, FieldChanged_EventArgs e);
}
