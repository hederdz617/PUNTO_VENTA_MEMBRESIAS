using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public class EmpleadosRegistro : Form
    {
        private readonly AppData _data;
        private TextBox txtNombre, txtApellido, txtCorreo, txtEdad, txtTelefono;
        private Button btnGuardar, btnEnrollar, btnVerificar;
        private int _ultimoEmpleadoId = -1;
        public EmpleadosRegistro(AppData data)
        {
            _data = data;
            // Inicializar base de datos al abrir el formulario
            Services.DatabaseService.InitializeDatabase();
            this.Text = "Registro de Empleados";
            this.Width = 400;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lblNombre = new Label { Text = "Nombre:", Left = 30, Top = 30, Width = 80 };
            txtNombre = new TextBox { Left = 120, Top = 30, Width = 200 };
            Label lblApellido = new Label { Text = "Apellido:", Left = 30, Top = 70, Width = 80 };
            txtApellido = new TextBox { Left = 120, Top = 70, Width = 200 };
            Label lblCorreo = new Label { Text = "Correo:", Left = 30, Top = 110, Width = 80 };
            txtCorreo = new TextBox { Left = 120, Top = 110, Width = 200 };
            Label lblEdad = new Label { Text = "Edad:", Left = 30, Top = 150, Width = 80 };
            txtEdad = new TextBox { Left = 120, Top = 150, Width = 200 };
            Label lblTelefono = new Label { Text = "Teléfono:", Left = 30, Top = 190, Width = 80 };
            txtTelefono = new TextBox { Left = 120, Top = 190, Width = 200 };

            btnGuardar = new Button { Text = "Guardar", Left = 120, Top = 230, Width = 90 };
            btnGuardar.Click += BtnGuardar_Click;

            btnEnrollar = new Button { Text = "Enrollar Huella", Left = 30, Top = 280, Width = 130, Enabled = false };
            btnEnrollar.Click += BtnEnrollar_Click;

            btnVerificar = new Button { Text = "Verificar Huella", Left = 190, Top = 280, Width = 130 };
            btnVerificar.Click += BtnVerificar_Click;

            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblApellido);
            this.Controls.Add(txtApellido);
            this.Controls.Add(lblCorreo);
            this.Controls.Add(txtCorreo);
            this.Controls.Add(lblEdad);
            this.Controls.Add(txtEdad);
            this.Controls.Add(lblTelefono);
            this.Controls.Add(txtTelefono);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnEnrollar);
            this.Controls.Add(btnVerificar);
        }

        private bool EsTextoValido(string texto)
        {
            // Solo letras mayúsculas/minúsculas sin acentos
            foreach (char c in texto)
            {
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                    return false;
            }
            return true;
        }

        private bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            if (!correo.Contains("@")) return false;
            char[] prohibidos = { ':', ',', '\'', '"' };
            foreach (char c in prohibidos)
            {
                if (correo.Contains(c)) return false;
            }
            return true;
        }

        private bool EsEdadValida(int edad)
        {
            return edad > 0 && edad <= 100;
        }

        private bool EsTelefonoValido(string telefono)
        {
            if (telefono.Length != 10) return false;
            foreach (char c in telefono)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string correo = txtCorreo.Text.Trim();
            string edadStr = txtEdad.Text.Trim();
            string telefono = txtTelefono.Text.Trim();
            int edad = 0;
            int.TryParse(edadStr, out edad);

            // Validación de nombre y apellido
            if (!EsTextoValido(nombre))
            {
                MessageBox.Show("El nombre solo debe contener letras sin acentos ni símbolos.", "Validación");
                return;
            }
            if (!EsTextoValido(apellido))
            {
                MessageBox.Show("El apellido solo debe contener letras sin acentos ni símbolos.", "Validación");
                return;
            }
            // Validación de correo
            if (!EsCorreoValido(correo))
            {
                MessageBox.Show("El correo debe contener un '@' y no debe contener los caracteres : , ' \"", "Validación");
                return;
            }
            // Validación de edad
            if (!EsEdadValida(edad))
            {
                MessageBox.Show("La edad debe ser un número positivo y no mayor a 100.", "Validación");
                return;
            }
            // Validación de teléfono
            if (!EsTelefonoValido(telefono))
            {
                MessageBox.Show("El teléfono debe contener exactamente 10 dígitos numéricos.", "Validación");
                return;
            }

            try
            {
                int empleadoId = Services.DatabaseService.InsertEmpleado(nombre, apellido, correo, edad, telefono);
                _ultimoEmpleadoId = empleadoId;
                btnEnrollar.Enabled = true;
                MessageBox.Show($"Empleado guardado con ID: {empleadoId}. Ahora puede enrollar la huella.", "Registro exitoso");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        private void BtnEnrollar_Click(object sender, EventArgs e)
        {
            if (_ultimoEmpleadoId <= 0)
            {
                MessageBox.Show("Primero debe guardar los datos del empleado.", "Atención");
                return;
            }
            MainForm.Instance?.OcultarVerificadorTray();
            var form = new EnrollmentForm(_data, _ultimoEmpleadoId, true);
            form.ShowDialog();
            MainForm.Instance?.MostrarVerificadorTray();
        }

        private void BtnVerificar_Click(object sender, EventArgs e)
        {
            MainForm.Instance?.OcultarVerificadorTray();
            var form = new VerificationForm(_data);
            form.ShowDialog();
            MainForm.Instance?.MostrarVerificadorTray();
        }
    }
}
