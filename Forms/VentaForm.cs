using System;
using System.Data;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;
using System.Data.SQLite;

namespace NuevoAPPwindowsforms.Forms
{
    public class VentaForm : Form
    {
        private TextBox txtBuscar;
        private ComboBox cmbClientes;
        private Button btnSeleccionar;
        private int _clienteSeleccionadoId = -1;
        private ListBox lstCarrito;
        private Button btnAgregarAlCarrito;
        private Button btnEliminarDelCarrito;
        private Label lblTotal;
        private decimal total = 0;
        private Label lblClienteSeleccionado;
        private Label lblTelefonoCliente;
        private ComboBox cmbProducto;
        private Button btnVenta;
        private Button btnRevisarActivo;
        private DateTimePicker dtpFechaInicio;

        public VentaForm()
        {
            this.Text = "Venta";
            this.Width = 800;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
            CargarClientes("");
            btnVenta.Enabled = false;
            if (dtpFechaInicio != null) dtpFechaInicio.Enabled = false;
        }

        private void InitializeUI()
        {
            Label lblBuscar = new Label { Text = "Buscar cliente:", Left = 20, Top = 20, Width = 100 };
            txtBuscar = new TextBox { Left = 130, Top = 18, Width = 180 };
            txtBuscar.TextChanged += (s, e) => CargarClientes(txtBuscar.Text.Trim());

            cmbClientes = new ComboBox { Left = 20, Top = 60, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList }; // Menú desplegable
            cmbClientes.SelectedIndexChanged += (s, e) => {
                if (cmbClientes.SelectedItem is ClienteItem item)
                {
                    _clienteSeleccionadoId = item.Id;
                }
            };

            btnSeleccionar = new Button { Text = "Seleccionar", Left = 140, Top = 100, Width = 100 };
            btnSeleccionar.Click += BtnSeleccionar_Click;

            // Carrito de compras
            Label lblCarrito = new Label { Text = "Carrito de compras:", Left = 400, Top = 120, Width = 150 };
            lstCarrito = new ListBox { Left = 400, Top = 150, Width = 350, Height = 80 };
            btnAgregarAlCarrito = new Button { Text = "Agregar producto", Left = 400, Top = 240, Width = 150 };
            btnAgregarAlCarrito.Click += BtnAgregarAlCarrito_Click;
            lblTotal = new Label { Text = "Total: $0.00", Left = 400, Top = 200, Width = 200 };

            // Cliente seleccionado
            lblClienteSeleccionado = new Label { Text = "Cliente: (ninguno)", Left = 400, Top = 0, Width = 350, Height = 32, Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold) };
            lblTelefonoCliente = new Label { Text = "Teléfono: ", Left = 400, Top = 38, Width = 350, Height = 28, Font = new System.Drawing.Font("Segoe UI", 11) };


            // ComboBox de productos
            Label lblProducto = new Label { Text = "Producto:", Left = 570, Top = 270, Width = 70 };
            cmbProducto = new ComboBox { Left = 640, Top = 270, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProducto.Items.AddRange(new string[] { "Mensualidad", "Quincena", "Semana", "Visita" });
            cmbProducto.SelectedIndex = 0;

            btnEliminarDelCarrito = new Button { Text = "Eliminar producto", Left = 560, Top = 320, Width = 150 };
            btnEliminarDelCarrito.Click += BtnEliminarDelCarrito_Click;

            btnVenta = new Button {
                Text = "Venta",
                Left = 400,
                Top = lblTelefonoCliente.Top + lblTelefonoCliente.Height + 20,
                Width = 180,
                Height = 45,
                Enabled = false,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold)
            };
            btnVenta.Click += BtnVenta_Click;

            btnRevisarActivo = new Button { Text = "Revisar Activo", Left = 140, Top = 140, Width = 100 };
            btnRevisarActivo.Click += BtnRevisarActivo_Click;

            // Botón para buscar por huella
            var btnBuscarPorHuella = new Button { Text = "Buscar por huella", Left = 320, Top = 100, Width = 120 };
            btnBuscarPorHuella.Click += BtnBuscarPorHuella_Click;

            this.Controls.Add(lblBuscar);
            this.Controls.Add(txtBuscar);
            this.Controls.Add(cmbClientes);
            this.Controls.Add(btnSeleccionar);
            this.Controls.Add(lblCarrito);
            this.Controls.Add(lstCarrito);
            this.Controls.Add(btnAgregarAlCarrito);
            this.Controls.Add(btnEliminarDelCarrito);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblClienteSeleccionado);
            this.Controls.Add(lblTelefonoCliente);
            this.Controls.Add(lblProducto);
            this.Controls.Add(cmbProducto);
            this.Controls.Add(btnVenta);
            this.Controls.Add(btnRevisarActivo);
            this.Controls.Add(btnBuscarPorHuella);
            // Colocar el calendario debajo del botón Agregar producto
            Label lblFechaInicio = new Label { Text = "Fecha inicio:", Left = btnAgregarAlCarrito.Left, Top = btnAgregarAlCarrito.Top + btnAgregarAlCarrito.Height + 10, Width = 100 };
            dtpFechaInicio = new DateTimePicker { Left = lblFechaInicio.Left + lblFechaInicio.Width + 10, Top = lblFechaInicio.Top - 2, Width = 120, Format = DateTimePickerFormat.Short };
            dtpFechaInicio.Value = DateTime.Now;
            this.Controls.Add(lblFechaInicio);
            this.Controls.Add(dtpFechaInicio);
        }

        private class ClienteItem
        {
            public int Id { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public override string ToString() => NombreCompleto;
        }

        private void CargarClientes(string filtro)
        {
            cmbClientes.Items.Clear();
            using (var conn = Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Nombre || ' ' || Apellido AS NombreCompleto FROM Cliente WHERE Nombre LIKE @filtro OR Apellido LIKE @filtro ORDER BY Nombre, Apellido";
                cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
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
                _clienteSeleccionadoId = item.Id;
                lblClienteSeleccionado.Text = $"Cliente: {item.NombreCompleto}";
                // Buscar teléfono del cliente
                string telefono = "";
                using (var conn = Services.DatabaseService.GetConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Telefono FROM Cliente WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", _clienteSeleccionadoId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            telefono = reader["Telefono"].ToString();
                        }
                    }
                }
                lblTelefonoCliente.Text = $"Teléfono: {telefono}";
                btnVenta.Enabled = true;
                // Verificar membresía activa
                var membresia = Services.DatabaseService.GetMembresiaActiva(_clienteSeleccionadoId);
                if (dtpFechaInicio != null)
                {
                    if (membresia.FechaFin != null && membresia.Activo)
                    {
                        dtpFechaInicio.Enabled = false;
                        MessageBox.Show($"El cliente ya tiene una membresía activa hasta: {membresia.FechaFin:yyyy-MM-dd}", "Membresía activa");
                    }
                    else
                    {
                        dtpFechaInicio.Enabled = true;
                    }
                }
                // Aquí puedes continuar con la lógica de venta
            }
            else
            {
                btnVenta.Enabled = false;
                lblTelefonoCliente.Text = "Teléfono: ";
                if (dtpFechaInicio != null) dtpFechaInicio.Enabled = false;
                MessageBox.Show("Seleccione un cliente de la lista.", "Atención");
            }
        }

        private void BtnAgregarAlCarrito_Click(object sender, EventArgs e)
        {
            if (lstCarrito.Items.Count >= 1)
            {
                MessageBox.Show("Solo se permite un producto por venta.", "Carrito");
                return;
            }
            string producto = cmbProducto.SelectedItem?.ToString() ?? "Producto";
            decimal precio = 0;
            switch (producto)
            {
                case "Mensualidad": precio = 400; break;
                case "Quincena": precio = 250; break;
                case "Semana": precio = 150; break;
                case "Visita": precio = 50; break;
                default: precio = 100; break;
            }
            lstCarrito.Items.Add($"{producto} - ${precio:F2}");
            total += precio;
            lblTotal.Text = $"Total: ${total:F2}";
        }

        private void BtnEliminarDelCarrito_Click(object sender, EventArgs e)
        {
            if (lstCarrito.SelectedItem != null)
            {
                string item = lstCarrito.SelectedItem.ToString();
                // Extraer el precio del string
                int idx = item.LastIndexOf("$");
                decimal precio = 0;
                if (idx >= 0 && decimal.TryParse(item.Substring(idx + 1), out precio))
                {
                    total -= precio;
                    if (total < 0) total = 0;
                }
                lstCarrito.Items.Remove(lstCarrito.SelectedItem);
                lblTotal.Text = $"Total: ${total:F2}";
            }
            else
            {
                MessageBox.Show("Seleccione un producto del carrito para eliminar.", "Atención");
            }
        }

        private void BtnVenta_Click(object sender, EventArgs e)
        {
            if (_clienteSeleccionadoId <= 0)
            {
                MessageBox.Show("Seleccione un cliente antes de realizar la venta.", "Atención");
                return;
            }
            if (lstCarrito.Items.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto al carrito.", "Atención");
                return;
            }
            try
            {
                // Determinar la fecha de inicio a usar
                DateTime fechaInicio;
                var membresiaActiva = Services.DatabaseService.GetMembresiaActiva(_clienteSeleccionadoId);
                if (membresiaActiva.FechaFin != null && membresiaActiva.Activo)
                {
                    // Si tiene membresía activa, sumar desde la fecha fin actual
                    fechaInicio = membresiaActiva.FechaFin.Value.AddDays(1);
                }
                else if (dtpFechaInicio != null && dtpFechaInicio.Enabled)
                {
                    // Si no tiene membresía activa, usar la fecha seleccionada en el calendario
                    fechaInicio = dtpFechaInicio.Value.Date;
                }
                else
                {
                    // Si el calendario está deshabilitado y no hay membresía activa, usar la fecha actual
                    fechaInicio = DateTime.Now.Date;
                }
                var fechaVenta = DateTime.Now;
                int idVenta = Services.DatabaseService.InsertVenta(_clienteSeleccionadoId, fechaVenta, total);
                foreach (var item in lstCarrito.Items)
                {
                    string itemStr = item.ToString();
                    int idx = itemStr.LastIndexOf("-");
                    string producto = itemStr;
                    decimal precio = 0;
                    if (idx > 0)
                    {
                        producto = itemStr.Substring(0, idx).Trim();
                        string precioStr = itemStr.Substring(idx + 1).Replace("$", "").Trim();
                        decimal.TryParse(precioStr, out precio);
                    }
                    Services.DatabaseService.InsertDetalleVenta(idVenta, producto, precio, fechaVenta);
                }
                // Insertar membresía automáticamente al realizar la venta, usando la fecha de inicio seleccionada
                string productoMembresia = "";
                if (lstCarrito.Items.Count > 0)
                {
                    string itemStr = lstCarrito.Items[0].ToString();
                    int idx = itemStr.IndexOf("-");
                    if (idx > 0)
                        productoMembresia = itemStr.Substring(0, idx).Trim();
                }
                // Calcular fecha fin según el producto
                DateTime fechaFin = fechaInicio;
                switch (productoMembresia)
                {
                    case "Mensualidad":
                        fechaFin = fechaInicio.AddMonths(1);
                        break;
                    case "Quincena":
                        fechaFin = fechaInicio.AddDays(15);
                        break;
                    case "Semana":
                        fechaFin = fechaInicio.AddDays(7);
                        break;
                    case "Visita":
                        fechaFin = fechaInicio.AddDays(1);
                        break;
                    default:
                        fechaFin = fechaInicio.AddMonths(1);
                        break;
                }
                // Insertar membresía con fecha de inicio y fin
                Services.DatabaseService.InsertMembresia(_clienteSeleccionadoId, fechaInicio, productoMembresia, fechaFin);
                // Consultar membresía activa y mostrar detalles
                var membresia = Services.DatabaseService.GetMembresiaActiva(_clienteSeleccionadoId);
                if (membresia.FechaFin != null && membresia.Activo)
                {
                    MessageBox.Show($"Membresía activa. Fecha de finalización: {membresia.FechaFin:yyyy-MM-dd}", "Membresía");
                }
                else
                {
                    MessageBox.Show("El cliente no tiene membresía activa.", "Membresía");
                }
                MessageBox.Show($"Venta registrada con éxito. ID Venta: {idVenta}", "Venta");
                // Enviar mensaje a Telegram
                string nombreCliente = "";
                using (var conn = Services.DatabaseService.GetConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Nombre, Apellido FROM Cliente WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", _clienteSeleccionadoId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nombreCliente = reader["Nombre"] + " " + reader["Apellido"];
                        }
                    }
                }
                // Construir lista de productos vendidos
                var productosList = new System.Collections.Generic.List<string>();
                foreach (var item in lstCarrito.Items)
                {
                    productosList.Add(item.ToString());
                }
                string productos = string.Join(", ", productosList);
                string mensaje = $"Venta registrada:\nCliente: {nombreCliente}\nProductos: {productos}\nTotal: ${total:F2}\nFecha inicio: {fechaInicio:yyyy-MM-dd}\nFecha fin: {fechaFin:yyyy-MM-dd}\nFecha y hora venta: {fechaVenta:yyyy-MM-dd HH:mm:ss}";
                _ = NuevoAPPwindowsforms.Services.TelegramService.EnviarMensajeAGrupoAsync(mensaje, NuevoAPPwindowsforms.Services.TelegramService.ChatIdVenta);
                lstCarrito.Items.Clear();
                total = 0;
                lblTotal.Text = "Total: $0.00";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error");
            }
        }

        private void BtnRevisarActivo_Click(object sender, EventArgs e)
        {
            if (cmbClientes.SelectedItem is ClienteItem item)
            {
                var membresia = Services.DatabaseService.GetMembresiaActiva(item.Id);
                if (membresia.FechaFin != null && membresia.Activo)
                {
                    MessageBox.Show($"Membresía activa. Fecha de finalización: {membresia.FechaFin:yyyy-MM-dd}", "Membresía");
                }
                else
                {
                    MessageBox.Show("El cliente no tiene membresía activa.", "Membresía");
                }
            }
            else
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Atención");
            }
        }

        private void BtnBuscarPorHuella_Click(object sender, EventArgs e)
        {
            var data = new NuevoAPPwindowsforms.Models.AppData();
            var form = new VerificationForm(data);
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Buscar cliente por huella (asumiendo que VerificationForm pone el ID en data.ClienteId)
                if (data.ClienteId > 0)
                {
                    _clienteSeleccionadoId = data.ClienteId;
                    // Buscar nombre completo
                    using (var conn = Services.DatabaseService.GetConnection())
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT Nombre || ' ' || Apellido AS NombreCompleto FROM Cliente WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@id", _clienteSeleccionadoId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblClienteSeleccionado.Text = $"Cliente: {reader["NombreCompleto"]}";
                                btnVenta.Enabled = true;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró cliente para la huella.", "Búsqueda por huella");
                }
            }
        }
    }
}
