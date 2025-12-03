using System;
using System.Windows.Forms;

namespace NuevoAPPwindowsforms.Forms
{
    public class ConexionSerialForm : Form
    {
        private TextBox txtPuerto;
        private TextBox txtBaudrate;
        private Label lblPuerto;
        private Label lblBaudrate;
        private Button btnAceptar;

        public string PuertoSerial { get; private set; } = "COM9";
        public int Baudrate { get; private set; } = 9600;

        public ConexionSerialForm()
        {
            this.Text = "Configuración de Conexión Serial";
            this.Width = 320;
            this.Height = 200;
            var config = NuevoAPPwindowsforms.Services.SerialConfigService.Load();
            PuertoSerial = config.PuertoSerial;
            Baudrate = config.Baudrate;
            InitializeUI();
        }

        private void InitializeUI()
        {
            lblPuerto = new Label { Text = "Puerto Serial (ej. COM9):", Left = 20, Top = 20, Width = 150 };
            txtPuerto = new TextBox { Left = 170, Top = 20, Width = 100, Text = PuertoSerial };
            lblBaudrate = new Label { Text = "Baudrate (ej. 9600):", Left = 20, Top = 60, Width = 150 };
            txtBaudrate = new TextBox { Left = 170, Top = 60, Width = 100, Text = Baudrate.ToString() };
            btnAceptar = new Button { Text = "Aceptar", Left = 100, Top = 110, Width = 90 };
            btnAceptar.Click += BtnAceptar_Click;

            this.Controls.Add(lblPuerto);
            this.Controls.Add(txtPuerto);
            this.Controls.Add(lblBaudrate);
            this.Controls.Add(txtBaudrate);
            this.Controls.Add(btnAceptar);
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            PuertoSerial = txtPuerto.Text.Trim();
            int.TryParse(txtBaudrate.Text.Trim(), out int baud);
            Baudrate = baud > 0 ? baud : 9600;
            // Guardar en archivo
            var config = new NuevoAPPwindowsforms.Services.SerialConfig { PuertoSerial = PuertoSerial, Baudrate = Baudrate };
            NuevoAPPwindowsforms.Services.SerialConfigService.Save(config);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
