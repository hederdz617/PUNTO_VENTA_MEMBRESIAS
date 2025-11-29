using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public class EditarClienteForm : Form
    {
        private TextBox txtNombre, txtApellido, txtCorreo, txtEdad, txtTelefono;
        private Button btnGuardar, btnEditarHuella, btnVerificarHuella;
        private int _clienteId;

        public EditarClienteForm(int clienteId)
        {
            _clienteId = clienteId;
            this.Text = "Editar Cliente";
            this.Width = 400;
            this.Height = 350;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
            CargarDatosCliente();
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

        private void CargarDatosCliente()
        {
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Nombre, Apellido, Correo, Edad, Telefono FROM Cliente WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", _clienteId);
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
                    cmd.CommandText = "UPDATE Cliente SET Nombre = @nombre, Apellido = @apellido, Correo = @correo, Edad = @edad, Telefono = @telefono WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@edad", edad);
                    cmd.Parameters.AddWithValue("@telefono", telefono);
                    cmd.Parameters.AddWithValue("@id", _clienteId);
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
            Services.DatabaseService.EliminarHuellasPorCliente(_clienteId);
            var data = new AppData();
            var form = new EnrollmentForm(data, _clienteId);
            form.ShowDialog();
        }

        private void BtnVerificarHuella_Click(object sender, EventArgs e)
        {
            // Cargar la huella del cliente desde la base de datos
            var data = new AppData();
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Template FROM Huella WHERE ClienteId = @clienteId LIMIT 1;";
                cmd.Parameters.AddWithValue("@clienteId", _clienteId);
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
            var form = new VerificationForm(data);
            form.ShowDialog();
        }
    }
}
