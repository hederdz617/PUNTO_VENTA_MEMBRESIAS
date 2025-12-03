using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public partial class MainForm : Form
    {
        private readonly AppData _data;
        private VerificadorTrayForm verificadorTray;
        public static MainForm Instance { get; private set; }
        private string _puertoSerial;
        private int _baudrate;
        public MainForm()
        {
            Instance = this;
            _data = new AppData();
            var config = NuevoAPPwindowsforms.Services.SerialConfigService.Load();
            _puertoSerial = config.PuertoSerial;
            _baudrate = config.Baudrate;
            this.Text = "Menú Principal";
            this.Width = 350;
            this.Height = 280;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeMenuUI();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            verificadorTray = new VerificadorTrayForm();
            verificadorTray.Show();
            verificadorTray.Hide(); // Mantenerlo en bandeja, no visible
        }

        public void OcultarVerificadorTray()
        {
            if (verificadorTray != null && !verificadorTray.IsDisposed)
                verificadorTray.Close();
        }

        public void MostrarVerificadorTray()
        {
            if (verificadorTray == null || verificadorTray.IsDisposed)
            {
                verificadorTray = new VerificadorTrayForm();
                verificadorTray.Show();
                verificadorTray.Hide();
            }

        }

        private void InitializeMenuUI()
        {
            var btnRegistro = new Button { Text = "Registro", Left = 50, Top = 50, Width = 100, Height = 50 };
            var btnVenta = new Button { Text = "Venta", Left = 180, Top = 50, Width = 100, Height = 50 };
            var btnEditarCliente = new Button { Text = "Editar Cliente", Left = 180, Top = 120, Width = 100, Height = 50 };
            var btnEmpleados = new Button { Text = "Empleados", Left = 50, Top = 120, Width = 100, Height = 50 };
            var btnConfiguracion = new Button { Text = "Configuracion", Left = 50, Top = 190, Width = 100, Height = 50 };
            btnRegistro.Font = btnVenta.Font = btnEditarCliente.Font = btnEmpleados.Font = btnConfiguracion.Font = new System.Drawing.Font("Segoe UI", 14);
            btnRegistro.Click += (s, e) => {
                var form = new RegistroForm(_data);
                form.ShowDialog();
            };
            btnVenta.Click += (s, e) => {
                var form = new VentaForm();
                form.ShowDialog();
            };
            btnEditarCliente.Click += (s, e) => {
                var input = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la contraseña para editar clientes:", "Autenticación", "");
                if (input != "contraseña123") // Cambiar 
                {
                    MessageBox.Show("Contraseña incorrecta.", "Acceso denegado");
                    return;
                }
                var seleccionarForm = new SeleccionarClienteForm();
                if (seleccionarForm.ShowDialog() == DialogResult.OK && seleccionarForm.ClienteIdSeleccionado > 0)
                {
                    var form = new EditarClienteForm(seleccionarForm.ClienteIdSeleccionado);
                    form.ShowDialog();
                }
            };
            btnEmpleados.Click += (s, e) => {
                var form = new EmpleadosForm();
                form.ShowDialog();
            };
            btnConfiguracion.Click += (s, e) => {
                var form = new ConfiguracionForm(_data);
                form.ShowDialog();
                // Actualizar configuración después de cerrar
                if (form is ConfiguracionForm conf)
                {
                    _puertoSerial = conf.PuertoSerial;
                    _baudrate = conf.Baudrate;
                }
            };
            this.Controls.Add(btnRegistro);
            this.Controls.Add(btnVenta);
            this.Controls.Add(btnEditarCliente);
            this.Controls.Add(btnEmpleados);
            this.Controls.Add(btnConfiguracion);
        }
    }
}
