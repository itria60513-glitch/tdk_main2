using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;
using EFEM.FileUtilities;
using EFEM.DataCenter;
using EFEM.LogUtilities;

namespace EFEM.GUIControls
{
	public class UserLoginForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label lUserName;
		private System.Windows.Forms.Label lPassword;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.Button btnCancel;
		private SlideDialog advance;
		private System.Windows.Forms.Button btnAdvance;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.TextBox password;
		private Form ownerForm = null;
        private ComboBox comboBox_historyUser;
        private static UserLoginForm instance = null;
        private LoginType curLoginType = LoginType.None;
        public delegate void delLoginTypeChanged(LoginType type);
        public event delLoginTypeChanged OnLoginTypeChanged = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        public AbstractFileUtilities FU = null;

		private UserLoginForm()
		{
			InitializeComponent();
            if (advance == null)
            {
                advance = new SlideDialog(this, 0.05f);
                advance.SlideDirection = SlideDialog.SLIDE_DIRECTION.BOTTOM;
            }
		}

        public static UserLoginForm GetInstance()
        {
            if (instance == null)
                instance = new UserLoginForm();

            return instance;
        }

        public LoginType CurrentLoginType
        {
            private set
            {
                curLoginType = value;
                if (OnLoginTypeChanged != null)
                {
                    try
                    {
                        OnLoginTypeChanged(curLoginType);
                    }
                    catch
                    {

                    }
                }
            }
            get { return curLoginType; }
        }

        private string appOwnerName = "EFEMGUI";
        public string AppOwnerName
        {
            get { return appOwnerName; }
        }
		public void SetFormOwner(Form owner)
		{
			ownerForm = owner;
            Type ownerType = owner.GetType();
            appOwnerName = ownerType.Namespace;
		}
        private bool lockModebyUser = false;
        public bool lock4Maintain = false;
        public string locked_UserId = "";
        public string locked_UserType = "";

        public void SetUserLockMode(bool lockmode)
        {
            SetUserLockMode(lockmode, UserLoginId, UserLoginType);
        }

        public void SetUserLockMode(bool lockmode, string userId,string userType)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { SetUserLockMode(lockmode, userId, userType); };
                this.Invoke(del);
            }
            else
            {
                lockModebyUser = lockmode;
                locked_UserId = userId;
                locked_UserType = userType;
                this.userName.Text = locked_UserId;
                if (lockmode)
                {
                    this.password.Text = "";
                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] Locked by {1}.", appOwnerName, userId));
                }
            }
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserLoginForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.userName = new System.Windows.Forms.TextBox();
            this.password = new System.Windows.Forms.TextBox();
            this.lUserName = new System.Windows.Forms.Label();
            this.lPassword = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAdvance = new System.Windows.Forms.Button();
            this.comboBox_historyUser = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(328, 104);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // userName
            // 
            this.userName.Location = new System.Drawing.Point(104, 112);
            this.userName.Name = "userName";
            this.userName.Size = new System.Drawing.Size(159, 20);
            this.userName.TabIndex = 1;
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(104, 144);
            this.password.MaxLength = 25;
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(176, 20);
            this.password.TabIndex = 3;
            this.password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.password_KeyPress);
            // 
            // lUserName
            // 
            this.lUserName.Location = new System.Drawing.Point(32, 112);
            this.lUserName.Name = "lUserName";
            this.lUserName.Size = new System.Drawing.Size(64, 24);
            this.lUserName.TabIndex = 0;
            this.lUserName.Text = "User Name:";
            this.lUserName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lPassword
            // 
            this.lPassword.Location = new System.Drawing.Point(32, 144);
            this.lPassword.Name = "lPassword";
            this.lPassword.Size = new System.Drawing.Size(64, 24);
            this.lPassword.TabIndex = 2;
            this.lPassword.Text = "Password:";
            this.lPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.SystemColors.Control;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnLogin.Location = new System.Drawing.Point(208, 176);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(104, 32);
            this.btnLogin.TabIndex = 6;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(96, 176);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(104, 32);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAdvance
            // 
            this.btnAdvance.BackColor = System.Drawing.SystemColors.Control;
            this.btnAdvance.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAdvance.Location = new System.Drawing.Point(8, 176);
            this.btnAdvance.Name = "btnAdvance";
            this.btnAdvance.Size = new System.Drawing.Size(80, 32);
            this.btnAdvance.TabIndex = 4;
            this.btnAdvance.Text = "Advance";
            this.btnAdvance.UseVisualStyleBackColor = false;
            this.btnAdvance.Click += new System.EventHandler(this.btnAdvance_Click);
            // 
            // comboBox_historyUser
            // 
            this.comboBox_historyUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_historyUser.FormattingEnabled = true;
            this.comboBox_historyUser.Location = new System.Drawing.Point(104, 112);
            this.comboBox_historyUser.Name = "comboBox_historyUser";
            this.comboBox_historyUser.Size = new System.Drawing.Size(176, 20);
            this.comboBox_historyUser.TabIndex = 8;
            this.comboBox_historyUser.SelectedIndexChanged += new System.EventHandler(this.comboBox_historyUser_SelectedIndexChanged);
            this.comboBox_historyUser.Click += new System.EventHandler(this.comboBox_historyUser_Click);
            // 
            // UserLoginForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(328, 214);
            this.Controls.Add(this.btnAdvance);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lPassword);
            this.Controls.Add(this.lUserName);
            this.Controls.Add(this.password);
            this.Controls.Add(this.userName);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.comboBox_historyUser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserLoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.Closed += new System.EventHandler(this.UserLoginForm_Closed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserLoginForm_FormClosing);
            this.Load += new System.EventHandler(this.UserLoginForm_Load);
            this.VisibleChanged += new System.EventHandler(this.UserLoginForm_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private string _userLoginType = "";
        public string UserLoginType
        {
            internal set
            {
                if (_userLoginType != value)
                {
                    _userLoginType = value;
                    LoginType newType = LoginType.None;

                    switch (_userLoginType)
                    {
                        case "Admin":
                            newType = LoginType.Admin;
                            break;
                        case "Service":
                            newType = LoginType.Service;
                            break;
                        case "Engineer":
                            newType = LoginType.Engineer;
                            break;
                        case "Operator":
                            newType = LoginType.Operator;
                            break;
                        case "SWRD":
                            newType = LoginType.SWRD;
                            break;
                        default:
                            newType = LoginType.None;
                            break;
                    }

                    CurrentLoginType = newType;
                }
            }
            get
            {
                return _userLoginType;
            }
        }

        private string _userLoginID = "";
        public string UserLoginId
        {
            internal set
            {
                _userLoginID = value;
            }
            get
            {
                return _userLoginID;
            }
        }

        //Login Button Click
		private void btnLogin_Click(object sender, System.EventArgs e)
		{
            if (!this.lockModebyUser)
            {
                //Login
                string type = this.CheckLogin(this.userName.Text, this.password.Text);

                if (type != "")
                {
                    //Login OK !
                    //record history
                    UserLoginType = type;
                    UserLoginId = this.userName.Text;

                    this.saveHistory();
                    this.password.Text = "";
                    this.userName.Text = "";
                    HideAdvance();
                    this.Hide();
                }
                else
                {
                    if (this.lock4Maintain && curLoginType == LoginType.None)
                    {
                        MessageBox.Show(this, string.Format("You need to login to unlock for Maintenance.", this.locked_UserId));
                    }
                }
            }
            else
            {
                //lock mode
                //verify Login type
                string user = this.userName.Text;
                string pw = this.password.Text;
                string type = this.VerifyLogin(user, pw);

                if (type == "Admin" || type == "SWRD")
                {
                    this.SetUserLockMode(false, "", "");
                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] UnLocked by {1} (Admin).", appOwnerName, user));
                    
                    //change to new login user
                    UserLoginType = type;
                    UserLoginId = user;

                    this.password.Text = "";
                    this.userName.Text = "";
                    HideAdvance();
                    this.Hide();
                    return;
                }
                else if (type != "" && user == this.locked_UserId)
                {
                    //self unlock
                    this.SetUserLockMode(false, "", "");
                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] UnLocked by {1} (Self unlock).", appOwnerName, user));

                    UserLoginType = type;
                    UserLoginId = user;
                    this.password.Text = "";
                    this.userName.Text = "";
                    HideAdvance();
                    this.Hide();
                    return;
                }
                else
                {
                    if (type != "")
                    {
                        //someone try to unlock others
                        //check userType and allow to unlock based on account level
                        //Admin > Service > Engineer > Operator
                        switch (locked_UserType)
                        {
                            case "Admin":
                                //only Admin can unlock
                                break;
                            case "Service": //Will enable RD page
                                //only Admin and self can unlock
                                break;
                            case "Engineer":
                                //Admin/Service and self can unlock
                                if (type == "Service")
                                {
                                    this.SetUserLockMode(false, "", "");
                                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] UnLocked by {1} ({2}).", appOwnerName, user, type));

                                    UserLoginType = type;
                                    UserLoginId = user;
                                    this.password.Text = "";
                                    this.userName.Text = "";

                                    HideAdvance();
                                    this.Hide();
                                    return;
                                }
                                break;
                            case "Operator":
                                //Admin/Service/Engineer and self can unlock
                                if (type == "Service" || type == "Engineer")
                                {
                                    this.SetUserLockMode(false, "", "");
                                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] UnLocked by {1} ({2}).", appOwnerName, user, type));

                                    UserLoginType = type;
                                    UserLoginId = user;
                                    this.password.Text = "";
                                    this.userName.Text = "";
                                    HideAdvance();
                                    this.Hide();
                                    return;
                                }
                                break;
                            default: //locked in non-login state
                                if (type == "Service" || type == "Engineer" || type == "Operator")
                                {
                                    this.SetUserLockMode(false, "", "");
                                    GUIBasic.Instance().WirteUserActionLog(LogHeadType.Info, string.Format("[{0}] UnLocked by {1} ({2}).", appOwnerName, user, type));

                                    UserLoginType = type;
                                    UserLoginId = user;
                                    this.password.Text = "";
                                    this.userName.Text = "";
                                    HideAdvance();
                                    this.Hide();
                                    return;
                                }
                                break;
                        }
                    }
                }
                if (this.lockModebyUser)
                {
                    //not allow to unlock
                    if (type != "")
                    {
                        if (string.IsNullOrWhiteSpace(locked_UserType))
                            MessageBox.Show(this, string.Format("Application is in use and has been locked. You need to login to unlock.", this.locked_UserId));
                        else
                        {
                            if (locked_UserType != "Admin")
                                MessageBox.Show(this, string.Format("Application is in use and has been locked by {0}. Only {1} or Admin or higher level account can unlock.", this.locked_UserId, this.UserLoginType));
                            else
                                MessageBox.Show(this, string.Format("Application is in use and has been locked by {0}. Only Admin or higher level account can unlock.", this.locked_UserId));
                        }
                    }
                }         
            }
		}

        //Cancel button Click
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
            if (this.lock4Maintain && curLoginType == LoginType.None)
            {
                //cannot cancel
                this.userName.Text = "";
                this.password.Text = "";
                //this.UserLoginType = "";
                HideAdvance();

                if (sender != null && sender.ToString() != "Close")
                    MessageBox.Show("EFEM GUI is locked. Please login first for maintenance.");

                return;
            }

            if (this.lockModebyUser)
            {
                if (sender != null && sender.ToString() != "Close")
                {
                    if (string.IsNullOrWhiteSpace(locked_UserId))
                        MessageBox.Show("EFEM GUI is locked by user. Please login to unlock.");
                    else
                        MessageBox.Show(string.Format("EFEM GUI is locked by {0}. Please login to unlock.", this.locked_UserId));
                }

                //cannot cancel
                this.userName.Text = "";
                this.password.Text = "";
                HideAdvance();
            }
            else
            {
                //Cancel
                this.userName.Text = "";
                this.password.Text = "";
                HideAdvance();
                this.Hide();
            }
		}

        public void CancelLockMaintain()
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate { CancelLockMaintain(); };
                this.Invoke(del);
            }
            else
            {
                lock4Maintain = false;

                if (!lockModebyUser)
                {
                    HideAdvance();
                    this.Hide();
                }
            }
        }

        private void HideAdvance()
        {
            this.advance.Hide();
            slided = false;
        }
		
		private bool slided = false;
        //Advance button Click
		private void btnAdvance_Click(object sender, System.EventArgs e)
		{
			//advance
            if (!slided)
            {
                string[] type = GetUserTypeList();
                if (type != null)
                    this.advance.setUserType(type);
                this.advance.TopMost = true;
                //this.advance.BringToFront();
                this.advance.Slide();
            }
            else
            {
                this.advance.TopMost = false;
                this.advance.Slide();
            }
            
            slided = !slided;
		}
        private string VerifyLogin(string username, string password)
        {
            ConnectFileUtilityInstance();
            if (this.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataKey.ApplicationOwner] = this.AppOwnerName;
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.VerifyUserType;
                commandTable[UserDataKey.UserName] = username;
                commandTable[UserDataKey.Password] = password;

                bool b = this.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                    //this.advance.TopMost = true;
                }
                if (!b)
                {
                    //faile to login
                    return "";
                }
                else
                {
                    if (returnTable[UserDataKey.LoginType] != null)
                    {
                        string loginType = returnTable[UserDataKey.LoginType].ToString();
                        return loginType;
                    }
                    return "";
                }
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
            }
            return "";
        }
        private string CheckLogin(string username, string password)
        {
            ConnectFileUtilityInstance();
            if (this.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataKey.ApplicationOwner] = this.AppOwnerName;
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.Login;
                commandTable[UserDataKey.UserName] = username;
                commandTable[UserDataKey.Password] = password;

                bool b = this.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                }
                if (!b)
                {
                    //faile to login
                    return "";
                }
                else
                {
                    if (returnTable[UserDataKey.LoginType] != null)
                    {
                        string loginType = returnTable[UserDataKey.LoginType].ToString();
                        return loginType;
                    }
                    return "";
                }
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
            }
            return "";
        }
        public string Logout(string username)
        {
            try
            {
                ConnectFileUtilityInstance();
                if (this.FU != null)
                {
                    Hashtable commandTable = new Hashtable();
                    Hashtable returnTable = new Hashtable();
                    commandTable[UserDataKey.ApplicationOwner] = this.AppOwnerName;
                    commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.Logout;
                    commandTable[UserDataKey.UserName] = username;

                    bool b = this.FU.UserDataQuery(commandTable, ref returnTable);
                    string message = "";
                    if (returnTable[UserDataKey.ReturnMessage] != null)
                    {
                        message = returnTable[UserDataKey.ReturnMessage].ToString();
                    }
                    if (!b)
                    {
                        //faile to logout
                        this.userName.Text = "";
                        this.password.Text = "";
                        this.UserLoginType = "";
                        this.UserLoginId = "";
                        return message;
                    }
                    else
                    {
                        this.userName.Text = "";
                        this.password.Text = "";
                        this.UserLoginType = "";
                        this.UserLoginId = "";
                        return "";
                    }
                }
                else
                {
                    this.userName.Text="";
                    this.password.Text="";
                    this.UserLoginType = "";
                    this.UserLoginId = "";
                    MessageBox.Show(this, "Cannot connect to Login control center!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            return "";
        }
        private string[] GetUserTypeList()
        {
            ConnectFileUtilityInstance();
            if (this.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.GetUserType;
                bool b = this.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                }
                if (!b)
                {
                    //faile to GetUserTypeList 
                    return null;
                }
                else
                {
                    if (returnTable[UserDataKey.TypeList] != null)
                    {
                        return (string[])returnTable[UserDataKey.TypeList];
                    }
                    return null;
                }
            }
            return null;
        }
        

        public void ConnectFileUtilityInstance()
        {
            if (FU == null)
                FU = FileUtility.GetUniqueInstance();
        }


		private void password_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar ==(char)(int)Keys.Enter)
			{
                this.btnLogin.PerformClick();
			}
		}

		private void UserLoginForm_Load(object sender, System.EventArgs e)
		{
			this.userName.Focus();
            updateUserIdHistory();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
            if (lock4Maintain && curLoginType == LoginType.None)
            {
                MessageBox.Show("EFEM GUI is locked. Please login first for maintenance.");
                e.Cancel = true;
                return;
            }

            if (lockModebyUser)
            {
                if (string.IsNullOrWhiteSpace(locked_UserId))
                    MessageBox.Show("EFEM GUI is locked by user. Please login to unlock.");
                else
                    MessageBox.Show(string.Format("EFEM GUI is locked by {0}. Please login to unlock.", this.locked_UserId));
                e.Cancel = true;
                return;
            }
			if(bCanClose)
                base.OnClosing(e);
			else
			{
				e.Cancel=true;
                HideAdvance();
			}
		}
		private bool bCanClose = true;
		public bool CanClose
		{
			set
			{
				this.bCanClose =value;
			}
			get
			{
				return this.bCanClose;
			}
		}
		private void UserLoginForm_Closed(object sender, System.EventArgs e)
		{
			if(!this.bCanClose) 
                return;

			if(ownerForm ==null) 
                return;

            if (ownerForm != this)
            {
                if (MessageBox.Show(this, "Do you want to cancel login process?", "Action Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
                else
                {
                    this.ownerForm.Close();
                }                
            }
		}

		private void UserLoginForm_VisibleChanged(object sender, System.EventArgs e)
		{	
			this.userName.Focus();
        }

        #region user login history
        public bool SaveSettingInRegistry(string name, string data)
        {
            try
            {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("Software", true);
                RegistryKey EliteGUIKey = softwareKey.CreateSubKey(ConstVC.Register.UserLoginHistory);
                EliteGUIKey.SetValue(name, data);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string ReadSettingFromRegistry(string name)
        {
            try
            {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("Software");
                RegistryKey EliteGUIKey = softwareKey.OpenSubKey(ConstVC.Register.UserLoginHistory);
                if (EliteGUIKey == null) return null;

                object obj = EliteGUIKey.GetValue(name, "");
                if (obj == null) return null;

                return (string)obj;
            }
            catch (Exception ex)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception, "ReadSettingFromRegistry() Failed. Cannot read setting from registry - " + name + ". Reason: " + ex.Message);
                return null;
            }
        }
        private void saveHistory()
        {
            //Do not record SWRD login info
            if (this.userName.Text.ToUpper() == "SWRD")
                return;

            bool exist = false;
            try
            {
                // save this.lotIdText.Text,this.waferIdText.Text
                string userId_History = this.ReadSettingFromRegistry("loginUserId");

                if (userId_History == null || userId_History == "")
                {
                    this.SaveSettingInRegistry("loginUserId", this.userName.Text);
                }
                else
                {
                    string[] str = userId_History.Split(',');
                    foreach (string s in str)
                    {
                        if (this.userName.Text == s)
                            exist = true;
                    }
                    if (!exist)
                    {
                        string newHistory = "";
                        if (str.Length <= 10)
                        {
                            this.SaveSettingInRegistry("loginUserId", userId_History + "," + this.userName.Text);
                        }
                        else
                        {
                            for (int i = 1; i < str.Length; i++)
                            {
                                newHistory = newHistory + str[i] + ",";
                            }
                            this.SaveSettingInRegistry("loginUserId", newHistory + this.userName.Text);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GUIBasic.Instance().WriteLog(LogHeadType.Exception, "saveHistory() Failed. Reason: " + ex.Message);
            }
        }

        private void updateUserIdHistory()
        {
            //get history
            string userId_History = this.ReadSettingFromRegistry("loginUserId");
            string[] str = null;
            if (userId_History!= null && userId_History != "")
            {
                str = userId_History.Split(',');
                if (str.Length != this.comboBox_historyUser.Items.Count)
                {
                    this.comboBox_historyUser.SuspendLayout();
                    this.comboBox_historyUser.Items.Clear();
                    this.comboBox_historyUser.Items.AddRange(str);
                    this.comboBox_historyUser.ResumeLayout();
                }
            }
            else // userId_History == ""
            {
                if (this.comboBox_historyUser.Items.Count > 0)
                {
                    //clean history
                    this.comboBox_historyUser.SuspendLayout();
                    this.comboBox_historyUser.Items.Clear();
                    this.comboBox_historyUser.ResumeLayout();
                }
            }
            
        }
        #endregion


        private void comboBox_historyUser_Click(object sender, EventArgs e)
        {
            updateUserIdHistory();
        }

        private void comboBox_historyUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_historyUser.SelectedItem != null)
            {
                this.userName.Text = this.comboBox_historyUser.SelectedItem.ToString();
            }
        }

        private void UserLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            btnCancel_Click("Close", null);
            //btnCancel.PerformClick();
        }
    }
}
