using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFEM.DataCenter;

namespace EFEM.GUIControls.StatusControls
{
    public partial class AlignerStatusCtrl : UserControl
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

        #region Construtor

        public AlignerStatusCtrl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        public bool SetValue(string name, object value)
        {

            if (value == null) return false;

            switch (name)
            {
                case "AlignerStatus":
                    {
                        if (!Enum.IsDefined(typeof(KawasakiProductStatusCode), value))
                            value = KawasakiProductStatusCode.Unknown;

                        switch ((KawasakiProductStatusCode)value)
                        {
                            case KawasakiProductStatusCode.Rdy:
                                lbAlignerStatus.Color = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(255)))), ((int)(((byte)(54)))));
                                lbAlignerStatus.On = true;
                                tAlignerStatus.Text = "Ready";
                                break;
                            case KawasakiProductStatusCode.Bsy: //Running
                                lbAlignerStatus.Color = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(202)))), ((int)(((byte)(255)))));
                                lbAlignerStatus.On = true;
                                tAlignerStatus.Text = "Busy";
                                break;
                            case KawasakiProductStatusCode.Off:
                                lbAlignerStatus.On = false;
                                tAlignerStatus.Text = "Servo OFF";
                                break;
                            case KawasakiProductStatusCode.Tch:
                                lbAlignerStatus.Color = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(68)))), ((int)(((byte)(255)))));
                                lbAlignerStatus.On = true;
                                tAlignerStatus.Text = "Teaching";
                                break;
                            case KawasakiProductStatusCode.Err:
                                lbAlignerStatus.Color = System.Drawing.Color.Red;
                                lbAlignerStatus.Blink(500);
                                tAlignerStatus.Text = "Error";
                                break;
                            case KawasakiProductStatusCode.Unknown:
                                tAlignerStatus.Text = "<Unknown>";
                                break;
                            default:
                                break;
                        }

                        break;
                    }
                case "AlignerSpeed":
                    tAlignerSpeed.Text = value.ToString();
                    break;
                case "Aligner_J1":
                    tJ1pos.Text = value.ToString();
                    break;
                case "Aligner_Chuck":
                    if (value is bool)
                        bsAlignerChuck.On = (bool)value;
                    else
                        bsAlignerChuck.On = false;
                    break;
                case "Aligner_Wafer":
                    if (value is bool)
                        bsWaferOnAligner.On = (bool)value;
                    else
                        bsWaferOnAligner.On = false;
                    break;
                default:
                    break;
            }

            return true;
        }

        #endregion


    }
}
