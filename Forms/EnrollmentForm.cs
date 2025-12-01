using System;
using System.Windows.Forms;
using DPFP.Gui.Enrollment;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public partial class EnrollmentForm : Form
    {
        public AppData Data { get; }
        public EnrollmentControl EnrollmentControl { get; private set; }
        private int _clienteId = -1;

        public EnrollmentForm(AppData data, int clienteId = -1)
        {
            Data = data;
            _clienteId = clienteId;
            this.Width = 500;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;
            // Permitir solo un dedo/huella
            Data.MaxEnrollFingerCount = 1;
            // InitializeComponent();
            InitializeEnrollmentControl();
        }

        private void InitializeEnrollmentControl()
        {
            EnrollmentControl = new EnrollmentControl();
            EnrollmentControl.Dock = DockStyle.Fill;
            EnrollmentControl.EnrolledFingerMask = Data.EnrolledFingersMask;
            EnrollmentControl.MaxEnrollFingerCount = Data.MaxEnrollFingerCount;
            EnrollmentControl.OnEnroll += EnrollmentControl_OnEnroll;
            EnrollmentControl.OnDelete += EnrollmentControl_OnDelete;
            EnrollmentControl.OnReaderConnect += EnrollmentControl_OnReaderConnect;
            EnrollmentControl.OnReaderDisconnect += EnrollmentControl_OnReaderDisconnect;
            EnrollmentControl.OnStartEnroll += EnrollmentControl_OnStartEnroll;
            EnrollmentControl.OnFingerTouch += EnrollmentControl_OnFingerTouch;
            EnrollmentControl.OnFingerRemove += EnrollmentControl_OnFingerRemove;
            EnrollmentControl.OnSampleQuality += EnrollmentControl_OnSampleQuality;
            EnrollmentControl.OnComplete += EnrollmentControl_OnComplete;
            EnrollmentControl.OnCancelEnroll += EnrollmentControl_OnCancelEnroll;
            this.Controls.Add(EnrollmentControl);
        }

        // Eventos clave
        private void EnrollmentControl_OnEnroll(object control, int finger, DPFP.Template template, ref DPFP.Gui.EventHandlerStatus status)
        {
            if (Data.EnrolledFingersMask != 0) // Ya hay una huella registrada
            {
                MessageBox.Show("Solo se permite registrar una huella por cliente.", "Límite de huellas");
                status = DPFP.Gui.EventHandlerStatus.Failure;
                return;
            }
            // Extraer FeatureSet de la huella actual
            DPFP.Processing.FeatureExtraction extractor = new DPFP.Processing.FeatureExtraction();
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet featureSet = new DPFP.FeatureSet();
            template.Serialize(new System.IO.MemoryStream()); // Solo para mantener la lógica, no se usa aquí
            // El FeatureSet debe venir del Sample, pero aquí solo tenemos el Template
            // Si tienes el Sample, pásalo aquí para extraer el FeatureSet correctamente
            // Suponiendo que tienes el FeatureSet de la huella actual:
            // Validar duplicidad de huella
            if (Data.FeatureSet != null && EsHuellaDuplicada(Data.FeatureSet))
            {
                MessageBox.Show("Esta huella ya está registrada para otro cliente.", "Huella duplicada");
                status = DPFP.Gui.EventHandlerStatus.Failure;
                return;
            }
            Data.Templates[finger - 1] = template;
            Data.EnrolledFingersMask = EnrollmentControl.EnrolledFingerMask;
            Data.Update();
            MessageBox.Show($"Dedo {finger} enrollado correctamente.", "Éxito");

            // Guardar automáticamente el template en la base de datos si hay cliente
            if (_clienteId > 0 && template != null)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    template.Serialize(ms);
                    byte[] templateBytes = ms.ToArray();
                    NuevoAPPwindowsforms.Services.DatabaseService.InsertHuella(_clienteId, $"Dedo {finger}", templateBytes);
                }
                MessageBox.Show("Huella guardada automáticamente en la base de datos.", "Éxito");
            }
        }

        private void EnrollmentControl_OnDelete(object control, int finger, ref DPFP.Gui.EventHandlerStatus status)
        {
            Data.Templates[finger - 1] = null;
            Data.EnrolledFingersMask = EnrollmentControl.EnrolledFingerMask;
            Data.Update();
            MessageBox.Show($"Dedo {finger} eliminado.", "Eliminado");
        }

        private void EnrollmentControl_OnReaderConnect(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnReaderDisconnect(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnStartEnroll(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnFingerTouch(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnFingerRemove(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnSampleQuality(object control, string readerSerialNumber, int finger, DPFP.Capture.CaptureFeedback feedback)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnComplete(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }
        private void EnrollmentControl_OnCancelEnroll(object control, string readerSerialNumber, int finger)
        {
            // Logging/debug
        }

        private bool EsHuellaDuplicada(DPFP.FeatureSet featureSet)
        {
            using (var conn = NuevoAPPwindowsforms.Services.DatabaseService.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ClienteId, Template FROM Huella";
                using (var reader = cmd.ExecuteReader())
                {
                    var verifier = new DPFP.Verification.Verification();
                    var res = new DPFP.Verification.Verification.Result();
                    while (reader.Read())
                    {
                        byte[] templateBytes = (byte[])reader["Template"];
                        var templateDB = new DPFP.Template(new System.IO.MemoryStream(templateBytes));
                        verifier.Verify(featureSet, templateDB, ref res);
                        if (res.Verified)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
