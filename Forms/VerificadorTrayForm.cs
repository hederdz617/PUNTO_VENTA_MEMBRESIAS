using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using DPFP;
using DPFP.Capture;
using DPFP.Verification;

namespace NuevoAPPwindowsforms.Forms
{
    public partial class VerificadorTrayForm : Form, DPFP.Capture.EventHandler
    {
        private NotifyIcon trayIcon;
        private Capture capture;
        private Verification verifier;
        private string dbPath;
        private string connStr;
        private SerialPort arduinoPort;
        private string _puertoSerial;
        private int _baudrate;

        public VerificadorTrayForm()
        {
            var config = NuevoAPPwindowsforms.Services.SerialConfigService.Load();
            _puertoSerial = config.PuertoSerial;
            _baudrate = config.Baudrate;
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientes.db");
            connStr = $"Data Source={dbPath};Version=3;";
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "Verificador de Huellas"
            };
            trayIcon.ShowBalloonTip(3000, "Verificador", "El verificador de huellas está activo en segundo plano.", ToolTipIcon.Info);
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Text = "Verificador de Huellas";
            this.Size = new Size(400, 200);
            var infoLabel = new Label() { Text = "Coloca un dedo en el lector para verificar.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(FontFamily.GenericSansSerif, 12) };
            this.Controls.Add(infoLabel);
            trayIcon.DoubleClick += (s, e) => {
                this.Invoke((Action)(() => {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.Show();
                    this.BringToFront();
                }));
            };
            this.Resize += (s, e) => {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.ShowInTaskbar = false;
                    this.Hide();
                }
            };
            this.Load += (s, e) => {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            };
            verifier = new Verification();
            capture = new Capture();
            capture.EventHandler = this;
            capture.StartCapture();

            // Inicializar puerto serie para el torniquete
            arduinoPort = new SerialPort(_puertoSerial, _baudrate);
            try
            {
                arduinoPort.Open();
            }
            catch (Exception ex)
            {
                trayIcon.ShowBalloonTip(3000, "Error Arduino", $"No se pudo abrir {_puertoSerial}: {ex.Message}", ToolTipIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            capture.StopCapture();
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                try { arduinoPort.Close(); } catch { }
            }
            base.OnFormClosing(e);
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
        {
            var features = ExtraerFeatures(Sample, DPFP.Processing.DataPurpose.Verification);
            if (features == null)
            {
                trayIcon.ShowBalloonTip(2000, "Verificador", "Huella de baja calidad.", ToolTipIcon.Warning);
                return;
            }
            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT h.ClienteId, h.Dedo, h.Template, c.Nombre, c.Apellido FROM Huella h JOIN Cliente c ON h.ClienteId = c.Id";
                using (var reader = cmd.ExecuteReader())
                {
                    bool encontrado = false;
                    var res = new Verification.Result();
                    int clienteId = -1;
                    string nombre = "", apellido = "", dedo = "";
                    while (reader.Read())
                    {
                        clienteId = reader.GetInt32(0);
                        dedo = reader.GetString(1);
                        byte[] templateGuardado = (byte[])reader[2];
                        nombre = reader[3].ToString();
                        apellido = reader[4].ToString();
                        var templateDB = new DPFP.Template(new MemoryStream(templateGuardado));
                        verifier.Verify(features, templateDB, ref res);
                        if (res.Verified)
                        {
                            // Verificar membresía activa
                            if (TieneMembresiaActiva(conn, clienteId, out DateTime fechaFin))
                            {
                                trayIcon.ShowBalloonTip(3000, "Acceso permitido", $"{nombre} {apellido}\nMembresía activa hasta: {fechaFin:yyyy-MM-dd}", ToolTipIcon.Info);
                                // Control de torniquete
                                if (arduinoPort != null && arduinoPort.IsOpen)
                                {
                                    try { arduinoPort.WriteLine("OPEN"); } catch { }
                                }
                            }
                            else
                            {
                                trayIcon.ShowBalloonTip(3000, "Acceso denegado", $"{nombre} {apellido}\nMembresía inactiva", ToolTipIcon.Warning);
                            }
                            encontrado = true;
                            break;
                        }
                    }
                    if (encontrado)
                        return;
                }
                // Buscar en empleados si no se encontró en clientes
                var cmdEmp = conn.CreateCommand();
                cmdEmp.CommandText = "SELECT h.EmpleadoId, h.Dedo, h.Template, e.Nombre, e.Apellido FROM HuellaEmpleado h JOIN Empleado e ON h.EmpleadoId = e.Id";
                using (var readerEmp = cmdEmp.ExecuteReader())
                {
                    var resEmp = new Verification.Result();
                    int empleadoId = -1;
                    string nombreEmp = "", apellidoEmp = "", dedoEmp = "";
                    while (readerEmp.Read())
                    {
                        empleadoId = readerEmp.GetInt32(0);
                        dedoEmp = readerEmp.GetString(1);
                        byte[] templateGuardado = (byte[])readerEmp[2];
                        nombreEmp = readerEmp[3].ToString();
                        apellidoEmp = readerEmp[4].ToString();
                        var templateDB = new DPFP.Template(new MemoryStream(templateGuardado));
                        verifier.Verify(features, templateDB, ref resEmp);
                        if (resEmp.Verified)
                        {
                            trayIcon.ShowBalloonTip(3000, "Acceso permitido (Empleado)", $"{nombreEmp} {apellidoEmp}", ToolTipIcon.Info);
                            // Enviar mensaje a Telegram
                            string mensaje = $"Empleado ingresó: {nombreEmp} {apellidoEmp}\nFecha y hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                            _ = NuevoAPPwindowsforms.Services.TelegramService.EnviarMensajeAsync(mensaje);
                            // Control de torniquete
                            if (arduinoPort != null && arduinoPort.IsOpen)
                            {
                                try { arduinoPort.WriteLine("OPEN"); } catch { }
                            }
                            return;
                        }
                    }
                }
                trayIcon.ShowBalloonTip(2000, "Verificador", "No se encontró coincidencia.", ToolTipIcon.Warning);
            }
        }

        private bool TieneMembresiaActiva(SQLiteConnection conn, int clienteId, out DateTime fechaFin)
        {
            fechaFin = DateTime.MinValue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Fecha_Fin FROM Membresias WHERE Id_Cliente = @idCliente AND Activo = 1 AND Fecha_Fin >= @hoy ORDER BY Fecha_Fin DESC LIMIT 1;";
            cmd.Parameters.AddWithValue("@idCliente", clienteId);
            cmd.Parameters.AddWithValue("@hoy", DateTime.Now.ToString("yyyy-MM-dd"));
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    fechaFin = DateTime.Parse(reader["Fecha_Fin"].ToString());
                    return true;
                }
            }
            return false;
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback) { }

        private FeatureSet ExtraerFeatures(Sample sample, DPFP.Processing.DataPurpose purpose)
        {
            var extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            FeatureSet features = new FeatureSet();
            extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);
            return feedback == DPFP.Capture.CaptureFeedback.Good ? features : null;
        }
    }
}
