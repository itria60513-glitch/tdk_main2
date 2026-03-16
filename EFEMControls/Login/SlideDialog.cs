using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using EFEM.FileUtilities;
using EFEM.DataCenter;

namespace EFEM.GUIControls
{
	public class SlideDialog : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer timer1;
        protected UserLoginForm _oOwner;
		public enum SLIDE_DIRECTION {TOP, LEFT, BOTTOM, RIGHT};
		protected SLIDE_DIRECTION _eSlideDirection;
		private float _fRatio; 
		private float _fStep;
		private bool _bExpand;
		private SizeF _oOffset;
		private SizeF _oStep;
		private System.Windows.Forms.TextBox NewUser;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnAddUser;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox pw1;
		private System.Windows.Forms.TextBox adminpw;
		private System.Windows.Forms.TextBox pw2;
		private System.Windows.Forms.TextBox change_oldpw;
		private System.Windows.Forms.TextBox change_newpw;
		private System.Windows.Forms.TextBox change_user;
		private System.Windows.Forms.TextBox del_adminpw;
		private System.Windows.Forms.TextBox change_confirmpw;
		private System.Windows.Forms.TextBox del_user;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox cbUserType;
		private System.Windows.Forms.Label label_alluser;
        private Button button_CleanHistory;
		private Point _oOrigin;
		
		/// <summary>
		/// Return the state of the form (expanded or not)
		/// </summary>
		public bool IsExpanded
		{
			get
			{
				return _bExpand;
			}
		}
		/// <summary>
		/// Direction of sliding
		/// </summary>
		public SLIDE_DIRECTION SlideDirection
		{
			set
			{
				_eSlideDirection = value;
			}
		}
		/// <summary>
		/// Slide step of the motion
		/// </summary>
		public float SlideStep
		{
			set
			{
				_fStep = value;
			}
		}
		/// <summary>
		/// Default constructor
		/// </summary>
		public SlideDialog() : this(null, 0)
		{
		}
		/// <summary>
		/// Constructor with parent window and step of sliding motion
		/// </summary>
        public SlideDialog(UserLoginForm poOwner, float pfStep)
		{
			InitializeComponent();
			_oOwner = poOwner;
			_fRatio = 0.0f;
			SlideStep = pfStep;
			if (poOwner != null)
				Owner = poOwner.Owner;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.NewUser = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pw1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.adminpw = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pw2 = new System.Windows.Forms.TextBox();
            this.btnAddUser = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cbUserType = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.change_confirmpw = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.change_oldpw = new System.Windows.Forms.TextBox();
            this.change_newpw = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.change_user = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button_CleanHistory = new System.Windows.Forms.Button();
            this.del_user = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.del_adminpw = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label_alluser = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // NewUser
            // 
            this.NewUser.Location = new System.Drawing.Point(120, 16);
            this.NewUser.Name = "NewUser";
            this.NewUser.Size = new System.Drawing.Size(160, 20);
            this.NewUser.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(48, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "New User:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(24, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 24);
            this.label2.TabIndex = 2;
            this.label2.Text = "New Password:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pw1
            // 
            this.pw1.Location = new System.Drawing.Point(120, 40);
            this.pw1.Name = "pw1";
            this.pw1.PasswordChar = '*';
            this.pw1.Size = new System.Drawing.Size(160, 20);
            this.pw1.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 24);
            this.label3.TabIndex = 8;
            this.label3.Text = "Admin Password:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // adminpw
            // 
            this.adminpw.Location = new System.Drawing.Point(120, 112);
            this.adminpw.Name = "adminpw";
            this.adminpw.PasswordChar = '*';
            this.adminpw.Size = new System.Drawing.Size(160, 20);
            this.adminpw.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(16, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 24);
            this.label4.TabIndex = 4;
            this.label4.Text = "Confirm Password:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pw2
            // 
            this.pw2.Location = new System.Drawing.Point(120, 64);
            this.pw2.Name = "pw2";
            this.pw2.PasswordChar = '*';
            this.pw2.Size = new System.Drawing.Size(160, 20);
            this.pw2.TabIndex = 5;
            // 
            // btnAddUser
            // 
            this.btnAddUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAddUser.Location = new System.Drawing.Point(120, 136);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Size = new System.Drawing.Size(160, 32);
            this.btnAddUser.TabIndex = 10;
            this.btnAddUser.Text = "Add User";
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(320, 208);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cbUserType);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.pw1);
            this.tabPage1.Controls.Add(this.pw2);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.adminpw);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.NewUser);
            this.tabPage1.Controls.Add(this.btnAddUser);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(312, 182);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Add User";
            // 
            // cbUserType
            // 
            this.cbUserType.Location = new System.Drawing.Point(120, 88);
            this.cbUserType.Name = "cbUserType";
            this.cbUserType.Size = new System.Drawing.Size(121, 21);
            this.cbUserType.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(24, 88);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 20);
            this.label10.TabIndex = 6;
            this.label10.Text = "Type:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.change_confirmpw);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.change_oldpw);
            this.tabPage2.Controls.Add(this.change_newpw);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.change_user);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(312, 182);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Change Password";
            // 
            // change_confirmpw
            // 
            this.change_confirmpw.Location = new System.Drawing.Point(120, 88);
            this.change_confirmpw.Name = "change_confirmpw";
            this.change_confirmpw.PasswordChar = '*';
            this.change_confirmpw.Size = new System.Drawing.Size(160, 20);
            this.change_confirmpw.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(0, 88);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 24);
            this.label7.TabIndex = 6;
            this.label7.Text = "Confirm Password:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(48, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 24);
            this.label5.TabIndex = 0;
            this.label5.Text = "User Name:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(24, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 24);
            this.label6.TabIndex = 2;
            this.label6.Text = "Old Password:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // change_oldpw
            // 
            this.change_oldpw.Location = new System.Drawing.Point(120, 39);
            this.change_oldpw.Name = "change_oldpw";
            this.change_oldpw.PasswordChar = '*';
            this.change_oldpw.Size = new System.Drawing.Size(160, 20);
            this.change_oldpw.TabIndex = 3;
            // 
            // change_newpw
            // 
            this.change_newpw.Location = new System.Drawing.Point(120, 64);
            this.change_newpw.Name = "change_newpw";
            this.change_newpw.PasswordChar = '*';
            this.change_newpw.Size = new System.Drawing.Size(160, 20);
            this.change_newpw.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(16, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 24);
            this.label8.TabIndex = 4;
            this.label8.Text = "New Password:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // change_user
            // 
            this.change_user.Location = new System.Drawing.Point(120, 15);
            this.change_user.Name = "change_user";
            this.change_user.Size = new System.Drawing.Size(160, 20);
            this.change_user.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(120, 136);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(160, 32);
            this.button2.TabIndex = 8;
            this.button2.Text = "Change";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button_CleanHistory);
            this.tabPage3.Controls.Add(this.del_user);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.del_adminpw);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Controls.Add(this.button3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(312, 182);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Delete User";
            // 
            // button_CleanHistory
            // 
            this.button_CleanHistory.Location = new System.Drawing.Point(196, 58);
            this.button_CleanHistory.Name = "button_CleanHistory";
            this.button_CleanHistory.Size = new System.Drawing.Size(93, 32);
            this.button_CleanHistory.TabIndex = 2;
            this.button_CleanHistory.Text = "Clean History";
            this.button_CleanHistory.UseVisualStyleBackColor = true;
            this.button_CleanHistory.Click += new System.EventHandler(this.button_CleanHistory_Click);
            // 
            // del_user
            // 
            this.del_user.Location = new System.Drawing.Point(68, 118);
            this.del_user.Name = "del_user";
            this.del_user.Size = new System.Drawing.Size(122, 20);
            this.del_user.TabIndex = 4;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(15, 114);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 24);
            this.label9.TabIndex = 3;
            this.label9.Text = "User:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // del_adminpw
            // 
            this.del_adminpw.Location = new System.Drawing.Point(129, 11);
            this.del_adminpw.Name = "del_adminpw";
            this.del_adminpw.PasswordChar = '*';
            this.del_adminpw.Size = new System.Drawing.Size(160, 20);
            this.del_adminpw.TabIndex = 1;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(3, 11);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(96, 24);
            this.label12.TabIndex = 0;
            this.label12.Text = "Admin Password:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button3.Location = new System.Drawing.Point(196, 111);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(93, 32);
            this.button3.TabIndex = 5;
            this.button3.Text = "Delete User";
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label_alluser
            // 
            this.label_alluser.Location = new System.Drawing.Point(8, 224);
            this.label_alluser.Name = "label_alluser";
            this.label_alluser.Size = new System.Drawing.Size(304, 184);
            this.label_alluser.TabIndex = 13;
            // 
            // SlideDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(320, 206);
            this.ControlBox = false;
            this.Controls.Add(this.label_alluser);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SlideDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion
		/// <summary>
		/// Use this method to start the slide motion (in ou out) 
		/// according to the slide direction
		/// </summary>
		public void Slide()
		{
			if (!_bExpand)
				Show();
			_oOwner.BringToFront();
			_bExpand = !_bExpand;
			timer1.Start();
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (_bExpand)
			{
				_fRatio += _fStep;
				_oOffset += _oStep;
				if (_fRatio >= 1)
				{
					timer1.Stop();
					this.BringToFront();
				}
			}
			else
			{
				_fRatio -= _fStep;
				_oOffset -= _oStep;
				if (_fRatio <= 0)
					timer1.Stop();
			}
			SetLocation();
		}
		private void SetLocation()
		{
			Location = _oOrigin + _oOffset.ToSize();
		}

		private void SlideDialog_Move(object sender, System.EventArgs e)
		{
			SetSlideLocation();
			SetLocation();
		}

		private void SlideDialog_Resize(object sender, System.EventArgs e)
		{
			SetSlideLocation();
			SetLocation();
		}
		private void SlideDialog_Closed(object sender, System.EventArgs e)
		{
			Close();
		}

		private void SetSlideLocation()
		{
			if (_oOwner != null)
			{
				_oOrigin = new Point();
				Point pp= _oOwner.Location;
				switch (_eSlideDirection)
				{
					case SLIDE_DIRECTION.BOTTOM:
						_oOrigin.X = pp.X;
						_oOrigin.Y = pp.Y + _oOwner.Height - Height;
						Width = _oOwner.Width;
						_oStep = new SizeF(0, Height * _fStep);
						break;
					case SLIDE_DIRECTION.LEFT:
						_oOrigin = pp;
						_oStep = new SizeF(- Width * _fStep, 0);
						Height = _oOwner.Height;
						break;
					case SLIDE_DIRECTION.TOP:
						_oOrigin = pp;
						Width = _oOwner.Width;
						_oStep = new SizeF(0, - Height * _fStep);
						break;
					case SLIDE_DIRECTION.RIGHT:
						_oOrigin.X = pp.X + _oOwner.Width - Width;
						_oOrigin.Y = pp.Y;
						_oStep = new SizeF(Width * _fStep, 0);
						Height = _oOwner.Height;
						break;
				}
			}
		}
		protected override void OnLoad(System.EventArgs e)
		{
			SetSlideLocation();
			SetLocation();
			if (_oOwner != null)
			{
				_oOwner.LocationChanged += new System.EventHandler(this.SlideDialog_Move);
				_oOwner.Resize += new System.EventHandler(this.SlideDialog_Resize);
				_oOwner.Closed += new System.EventHandler(this.SlideDialog_Closed);
			}
		}
		public void setUserType(string[] str)
		{
			this.cbUserType.Items.Clear();
			this.cbUserType.Items.Add("");
			this.cbUserType.Items.AddRange(str);
		}
        private UserXml db;
        public void setUserDB(UserXml useDB)
        {
            db = useDB;
            foreach (UserInfo info in db.GetAllUserInfo())
            {
                string s = string.Format("User={0},PWD={1},Type={2}", info.username, info.password, info.type);
                this.AppendLabelText(s);
            }
        }

		private void clear()
		{
			this.pw1.Text="";
			this.pw2.Text="";
			this.adminpw.Text="";
			this.NewUser.Text="";
			this.change_user.Text="";
			this.change_confirmpw.Text="";
			this.change_newpw.Text="";
			this.change_oldpw.Text=""; 
			this.del_adminpw.Text=""; 
			this.del_user.Text=""; 
			this.cbUserType.SelectedIndex=0;
		}

		private void btnAddUser_Click(object sender, System.EventArgs e)
		{
			// add user
            if (this.pw1.Text.Length < 6)
            {
                MessageBox.Show(_oOwner, "Passowrd needs to be at least 6 characters!");
                this.pw1.Text = "";
                this.pw2.Text = "";
                this.adminpw.Text = "";
                return;
            }
			if(this.pw1.Text != this.pw2.Text)
			{
				MessageBox.Show(_oOwner,"Confirm passowrd is not match!");
				this.pw1.Text="";
				this.pw2.Text="";
				this.adminpw.Text="";
				return;
			}
			if(this.NewUser.Text == "")
			{
				MessageBox.Show(_oOwner,"Enter a New User Name !");
				return;
			}
            if (this.cbUserType.SelectedIndex <= 0 || this.cbUserType.SelectedItem == null) 
			{
				MessageBox.Show(_oOwner,"Please Select User Login Type !");
				return;
            }
           
            bool success = CheckAndAddUser(this.NewUser.Text, this.pw1.Text, this.adminpw.Text, this.cbUserType.SelectedItem.ToString());

            if (!success)
                return;

            this.BringToFront();
            this.clear();
		}
        private bool CheckAndAddUser(string username, string password, string adminPassowrd, string loginType)
        {
            this._oOwner.ConnectFileUtilityInstance();
            if (this._oOwner.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataKey.ApplicationOwner] = this._oOwner.AppOwnerName;
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.AddUser;
                commandTable[UserDataKey.UserName] = username;
                commandTable[UserDataKey.Password] = password;
                commandTable[UserDataKey.LoginType] = loginType;
                commandTable[UserDataKey.AdminPassword] = adminPassowrd;

                bool b = this._oOwner.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }

                if (message != "")
                {
                    MessageBox.Show(this, message);
                    return false;
                }

                return true;
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
                return false;
            }
        }

		private void button2_Click(object sender, System.EventArgs e)
		{
			// Change password
            if (this.change_newpw.Text.Length < 6)
            {
                MessageBox.Show(_oOwner, "Passowrd needs to be at least 6 characters!");
                this.change_confirmpw.Text = "";
                this.change_newpw.Text = "";
                this.change_oldpw.Text = "";
                return;
            }
			if(this.change_newpw.Text!=this.change_confirmpw.Text)
			{
				MessageBox.Show(_oOwner,"Confirm passowrd is not match!");
				this.change_confirmpw.Text="";
				this.change_newpw.Text="";
				this.change_oldpw.Text="";
				return;
            }

            ChangePassword(this.change_user.Text, this.change_oldpw.Text, this.change_newpw.Text);
            this.BringToFront();
            this.clear();
		}
        private void ChangePassword(string username, string oldpassword, string newPassowrd)
        {
            this._oOwner.ConnectFileUtilityInstance();
            if (this._oOwner.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataKey.ApplicationOwner] = this._oOwner.AppOwnerName;
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.ChangePassword;
                commandTable[UserDataKey.UserName] = username;
                commandTable[UserDataKey.Password] = oldpassword;
                commandTable[UserDataKey.NewPassword] = newPassowrd;

                bool b = this._oOwner.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                }
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
            }
        }

		private void button3_Click(object sender, System.EventArgs e)
		{
            DeleteUser(this.del_user.Text, this.del_adminpw.Text);
            this.BringToFront();

			this.clear();
		}
        private void DeleteUser(string username, string adminPassowrd)
        {
            this._oOwner.ConnectFileUtilityInstance();
            if (this._oOwner.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[UserDataKey.ApplicationOwner] = this._oOwner.AppOwnerName;
                commandTable[UserDataQueryCommand.Command] = UserDataQueryCommand.RemoveUser;
                commandTable[UserDataKey.UserName] = username;
                commandTable[UserDataKey.AdminPassword] = adminPassowrd;

                bool b = this._oOwner.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                }
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
            }
        }
		public void AppendLabelText(string text)
		{
			this.label_alluser.Text += "\n";
			this.label_alluser.Text += text;
		}
        private void button_CleanHistory_Click(object sender, EventArgs e)
        {
            this._oOwner.ConnectFileUtilityInstance();
            if (this._oOwner.FU != null)
            {
                Hashtable commandTable = new Hashtable();
                Hashtable returnTable = new Hashtable();
                commandTable[FileUtilities.UserDataKey.ApplicationOwner] = this._oOwner.AppOwnerName;
                commandTable[FileUtilities.UserDataQueryCommand.Command] = FileUtilities.UserDataQueryCommand.VerifyUserType;
                commandTable[FileUtilities.UserDataKey.UserName] = "admin";
                commandTable[FileUtilities.UserDataKey.Password] = this.del_adminpw.Text;

                bool b = this._oOwner.FU.UserDataQuery(commandTable, ref returnTable);
                string message = "";
                if (returnTable[FileUtilities.UserDataKey.ReturnMessage] != null)
                {
                    message = returnTable[FileUtilities.UserDataKey.ReturnMessage].ToString();
                }
                if (message != "")
                {
                    MessageBox.Show(this, message);
                }
                if (returnTable[FileUtilities.UserDataKey.LoginType] != null)
                {
                    string type = returnTable[FileUtilities.UserDataKey.LoginType].ToString();
                    if (type == "Admin" || type == "SWRD")
                    {
                        cleanHistory();
                    }
                }
            }
            else
            {
                MessageBox.Show(this, "Cannot connect to Login control center!");
            }
        }

        public bool SaveSettingInRegistry(string name, string data)
        {
            try
            {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("Software", true);
                RegistryKey EliteGUIKey = softwareKey.CreateSubKey(ConstVC.Register.UserLoginHistory);
                EliteGUIKey.SetValue(name, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void cleanHistory()
        {
            try
            {
                this.SaveSettingInRegistry("loginUserId", "");
            }
            catch (Exception)
            {
            }
        }
	}
}
