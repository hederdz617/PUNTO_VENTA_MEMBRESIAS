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
            int clienteIdEncontrado = -1;
            // Buscar en base de datos la huella y obtener el ClienteId
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ClienteId, Template FROM Huella";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int clienteId = reader.GetInt32(0);
                        byte[] templateBytes = (byte[])reader[1];
                        var templateDB = new DPFP.Template(new System.IO.MemoryStream(templateBytes));
                        ver.Verify(featureSet, templateDB, ref res);
                        if (res.Verified)
                        {
                            match = true;
                            clienteIdEncontrado = clienteId;
                            MessageBox.Show($"Huella verificada. Cliente ID: {clienteId}\nFAR: {res.FARAchieved}", "Verificación exitosa");
                            break;
                        }
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
            Data.ClienteId = clienteIdEncontrado;
            Data.Update();
            if (match)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
