using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;

namespace NuevoAPPwindowsforms.Forms
{
    public class SeleccionarClienteForm : Form
    {
        public int ClienteIdSeleccionado { get; private set; } = -1;
        private ComboBox cmbClientes;
        private Button btnSeleccionar;

        public SeleccionarClienteForm()
        {
            this.Text = "Seleccionar Cliente";
            this.Width = 350;
            this.Height = 150;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
            CargarClientes();
        }

        private void InitializeUI()
        {
            Label lblCliente = new Label { Text = "Cliente:", Left = 20, Top = 20, Width = 60 };
            cmbClientes = new ComboBox { Left = 90, Top = 18, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            btnSeleccionar = new Button { Text = "Seleccionar", Left = 110, Top = 60, Width = 100 };
            btnSeleccionar.Click += BtnSeleccionar_Click;
            this.Controls.Add(lblCliente);
            this.Controls.Add(cmbClientes);
            this.Controls.Add(btnSeleccionar);
        }

        private void CargarClientes()
        {
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Nombre || ' ' || Apellido AS NombreCompleto FROM Cliente ORDER BY Nombre, Apellido";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbClientes.Items.Add(new ClienteItem
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            NombreCompleto = reader["NombreCompleto"].ToString()
                        });
                    }
                }
            }
            if (cmbClientes.Items.Count > 0)
                cmbClientes.SelectedIndex = 0;
        }

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            if (cmbClientes.SelectedItem is ClienteItem item)
            {
                ClienteIdSeleccionado = item.Id;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Seleccione un cliente.", "Atención");
            }
        }

        private class ClienteItem
        {
            public int Id { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public override string ToString() => NombreCompleto;
        }
    }
}
