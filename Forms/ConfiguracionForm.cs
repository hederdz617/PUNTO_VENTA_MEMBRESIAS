using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public partial class ConfiguracionForm : Form
    {
        private AppData _data;

        public ConfiguracionForm(AppData data)
        {
            InitializeComponent();
            _data = data;
        }

        private TextBox txtPuerto;
        private TextBox txtBaudrate;
        private Label lblPuerto;
        private Label lblBaudrate;
        private Button btnGuardar;
        private Button btnConexion;

        private string _puertoSerial = "COM9";
        private int _baudrate = 9600;
        public string PuertoSerial => _puertoSerial;
        public int Baudrate => _baudrate;

        private void InitializeComponent()
        {
            this.Text = "Configuración";
            this.Width = 400;
            this.Height = 300;

            btnConexion = new Button { Text = "Conexión", Left = 120, Top = 40, Width = 120 };
            btnConexion.Click += BtnConexion_Click;
            btnGuardar = new Button { Text = "Guardar", Left = 120, Top = 120, Width = 90 };
            btnGuardar.Click += btnGuardar_Click;

            this.Controls.Add(btnConexion);
            this.Controls.Add(btnGuardar);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Los valores ya están en _puertoSerial y _baudrate
            MessageBox.Show("Configuración guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void BtnConexion_Click(object sender, EventArgs e)
        {
            var form = new ConexionSerialForm();
            form.ShowDialog();
            // Si quieres actualizar los valores en ConfiguracionForm después de cerrar la ventana, puedes hacerlo aquí
            _puertoSerial = form.PuertoSerial;
            _baudrate = form.Baudrate;
        }
    }
}