using System;
using System.Windows.Forms;
using DPFP.Gui.Verification;
using DPFP.Verification;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public partial class VerificationForm : Form
    {
        public AppData Data { get; }
        public VerificationControl VerificationControl { get; private set; }

        public VerificationForm(AppData data)
        {
            Data = data;
            // InitializeComponent();
            InitializeVerificationControl();
        }

        private void InitializeVerificationControl()
        {
            VerificationControl = new VerificationControl();
            VerificationControl.Dock = DockStyle.Fill;
            VerificationControl.Active = true;
            VerificationControl.OnComplete += VerificationControl_OnComplete;
            this.Controls.Add(VerificationControl);
        }

        private void VerificationControl_OnComplete(object control, DPFP.FeatureSet featureSet, ref DPFP.Gui.EventHandlerStatus status)
        {
            var ver = new Verification();
            var res = new Verification.Result();
            bool match = false;
            for (int i = 0; i < AppData.MaxFingers; i++)
            {
                if (Data.Templates[i] != null)
                {
                    ver.Verify(featureSet, Data.Templates[i], ref res);
                    if (res.Verified)
                    {
                        match = true;
                        MessageBox.Show($"Huella verificada. Dedo: {i + 1}\nFAR: {res.FARAchieved}", "Verificación exitosa");
                        break;
                    }
                }
            }
            if (!match)
            {
                status = DPFP.Gui.EventHandlerStatus.Failure;
                MessageBox.Show("No se encontró coincidencia.", "Verificación fallida");
            }
            Data.IsFeatureSetMatched = match;
            Data.FalseAcceptRate = res.FARAchieved;
            Data.Update();
        }
    }
}
