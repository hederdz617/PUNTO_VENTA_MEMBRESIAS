using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public class EditarEmpleadoForm : Form
    {
        private TextBox txtNombre, txtApellido, txtCorreo, txtEdad, txtTelefono;
        private Button btnGuardar, btnEditarHuella, btnVerificarHuella;
        private int _empleadoId;
        public EditarEmpleadoForm(int empleadoId)
        {
            _empleadoId = empleadoId;
            this.Text = "Editar Empleado";
            this.Width = 400;
            this.Height = 350;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
            CargarDatosEmpleado();
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

            btnGuardar = new Button { Text = "Guardar", Left = 120, Top = 240, Width = 90 };
            btnGuardar.Click += BtnGuardar_Click;

            btnEditarHuella = new Button { Text = "Editar Huella", Left = 230, Top = 240, Width = 90 };
            btnEditarHuella.Click += BtnEditarHuella_Click;

            btnVerificarHuella = new Button { Text = "Verificar Huella", Left = 120, Top = 280, Width = 120 };
            btnVerificarHuella.Click += BtnVerificarHuella_Click;

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
            this.Controls.Add(btnEditarHuella);
            this.Controls.Add(btnVerificarHuella);
        }

        private void CargarDatosEmpleado()
        {
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Nombre, Apellido, Correo, Edad, Telefono FROM Empleado WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", _empleadoId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtNombre.Text = reader["Nombre"].ToString();
                        txtApellido.Text = reader["Apellido"].ToString();
                        txtCorreo.Text = reader["Correo"].ToString();
                        txtEdad.Text = reader["Edad"].ToString();
                        txtTelefono.Text = reader["Telefono"].ToString();
                    }
                }
            }
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

            // Aquí puedes agregar validaciones similares al registro

            try
            {
                using (var conn = Services.DatabaseService.GetConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE Empleado SET Nombre = @nombre, Apellido = @apellido, Correo = @correo, Edad = @edad, Telefono = @telefono WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@edad", edad);
                    cmd.Parameters.AddWithValue("@telefono", telefono);
                    cmd.Parameters.AddWithValue("@id", _empleadoId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Datos actualizados correctamente.", "Éxito");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
            }
        }

        private void BtnEditarHuella_Click(object sender, EventArgs e)
        {
            // Eliminar huella anterior
            Services.DatabaseService.EliminarHuellasPorEmpleado(_empleadoId);
            var data = new AppData();
            NuevoAPPwindowsforms.Forms.MainForm.Instance?.OcultarVerificadorTray();
            var form = new EnrollmentForm(data, _empleadoId, true); // true = esEmpleado
            form.ShowDialog();
            NuevoAPPwindowsforms.Forms.MainForm.Instance?.MostrarVerificadorTray();
        }

        private void BtnVerificarHuella_Click(object sender, EventArgs e)
        {
            // Cargar la huella del empleado desde la base de datos
            var data = new AppData();
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Template FROM HuellaEmpleado WHERE EmpleadoId = @empleadoId LIMIT 1;";
                cmd.Parameters.AddWithValue("@empleadoId", _empleadoId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var templateBytes = (byte[])reader["Template"];
                        using (var ms = new System.IO.MemoryStream(templateBytes))
                        {
                            data.Templates[0] = new DPFP.Template(ms);
                        }
                    }
                }
            }
            NuevoAPPwindowsforms.Forms.MainForm.Instance?.OcultarVerificadorTray();
            var form = new VerificationForm(data);
            form.ShowDialog();
            NuevoAPPwindowsforms.Forms.MainForm.Instance?.MostrarVerificadorTray();
        }

        /*
        private bool EsHuellaDuplicada(DPFP.FeatureSet featureSet)
        {
            // Lógica comentada para evitar errores de compilación
            return false;
        }
        */
    }
}
