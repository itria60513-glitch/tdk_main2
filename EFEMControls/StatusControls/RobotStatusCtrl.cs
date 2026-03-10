using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using EFEMInterface;
using EFEM.DataCenter;


namespace EFEM.GUIControls.StatusControls
{
    public partial class RobotStatusCtrl : UserControl
    {

        #region Defines

        public enum KawasakiProductStatusCode
        {
            [HMIDescription("Ready")]
            Rdy = 0,
            [HMIDescription("Busy")]
            Bsy = 1,
            [HMIDescription("Servo OFF")]
            Off = 2,
            [HMIDescription("Teaching")]
            Tch = 3,
            [HMIDescription("Error")]
            Err = 4,
            [HMIDescription("Unknown")]
            Unknown = 5,
        }
       
        #endregion


        #region Fields and Properties

        private bool _dualhand = true;
        public bool IsDualHand
        {
            get { return _dualhand; }
            set 
            {
                _dualhand = value;
                SetTabPageVisibility(tHand, tHand.TabPages[1], value);
                //Switch figure

            }
        }

        
        #endregion


        #region Contructor

        public RobotStatusCtrl()
        {
            InitializeComponent();
        }

        #endregion


        #region Methods

        void SetTabPageVisibility(TabControl tc, TabPage tp, bool visibility)
        {
            //if tp is not visible and visibility is set to true
            if ((visibility == true) && (tc.TabPages.IndexOf(tp) <= -1))
            {
                tc.TabPages.Insert(tc.TabCount, tp);
                //guarantee tabcontrol visibility
                tc.Visible = true;
                tc.SelectTab(tp);
            }
            //if tp is visible and visibility is set to false
            else if ((visibility == false) && (tc.TabPages.IndexOf(tp) > -1))
            {
                tc.TabPages.Remove(tp);
                //no pages to show, hide tabcontrol
                if (tc.TabCount == 0)
                {
                    tc.Visible = false;
                }
            }
        }

        public bool SetValue(string name,object value)
        {
            if (value == null) return false;

            switch (name)
            {
                case "RobotStatus":
                    {
                        if (!Enum.IsDefined(typeof(KawasakiProductStatusCode), value))
                            value = KawasakiProductStatusCode.Unknown;

                         switch ((KawasakiProductStatusCode)value)
                        {
                            case KawasakiProductStatusCode.Rdy:
                                lbRobotStatus.Color = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(255)))), ((int)(((byte)(54)))));
                                lbRobotStatus.On = true;
                                tRobotStatus.Text = "Ready";
                                break;
                            case KawasakiProductStatusCode.Bsy: //Running
                                lbRobotStatus.Color = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(202)))), ((int)(((byte)(255)))));
                                lbRobotStatus.On = true;
                                tRobotStatus.Text = "Busy";
                                break;
                            case KawasakiProductStatusCode.Off:
                                lbRobotStatus.On = false;
                                tRobotStatus.Text = "Servo OFF";
                                break;
                            case KawasakiProductStatusCode.Tch:
                                lbRobotStatus.Color =System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(68)))), ((int)(((byte)(255)))));
                                lbRobotStatus.On = false;
                                tRobotStatus.Text = "Teaching";
                                break;
                            case KawasakiProductStatusCode.Err:
                                lbRobotStatus.Color = System.Drawing.Color.Red;
                                lbRobotStatus.Blink(500);
                                tRobotStatus.Text = "Error";
                                break;
                             case KawasakiProductStatusCode.Unknown:
                                tRobotStatus.Text = "<Unkown>";
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case "RobotSpeed":
                    tRobotSpeed.Text = value.ToString();
                    break;
                case"Arm1_X":
                    tX1pos.Text = value.ToString();
                    break;
                case "Arm1_Y":
                    tY1pos.Text = value.ToString();
                    break;
                case "Arm1_Z":
                    tZ1pos.Text = value.ToString();
                    break;
                case "Arm1_A":
                    tHA1.Text = value.ToString();
                    break;
                case "Arm1_Chuck":
                    if (value is bool)
                        bsH1Chuck.On = (bool)value;
                    else
                        bsH1Chuck.On = false;
                    break;
                case "Arm1_Wafer":
                    if (value is bool)
                        bsWaferOnH1.On = (bool)value;
                    else
                        bsWaferOnH1.On = false;
                    break;
                case "Arm2_X":
                    tX2pos.Text = value.ToString();
                    break;
                case "Arm2_Y":
                    tY2pos.Text = value.ToString();
                    break;
                case "Arm2_Z":
                    tZ2pos.Text = value.ToString();
                    break;
                case "Arm2_A":
                    tHA2.Text = value.ToString();
                    break;
                case "Arm2_Chuck":
                    if (value is bool)
                        bsH2Chuck.On = (bool)value;
                    else
                        bsH2Chuck.On = false;
                    break;
                case "Arm2_Wafer":
                    if (value is bool)
                        bsWaferOnH1.On = (bool)value;
                    else
                        bsWaferOnH2.On = false;
                    break;
                case "Robot_J2":
                    tJ2pos.Text = value.ToString();
                    break;
                case "Robot_J4":
                    tJ4pos.Text = value.ToString();
                    break;
                case "Robot_J6":
                    tJ6pos.Text = value.ToString();
                    break;
                case "Robot_J7":
                    tJ7pos.Text = value.ToString();
                    break;
                default:
                    break;
            }

            return true;
        }

        #endregion

    }

}
