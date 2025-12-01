using System;
using System.Windows.Forms;
using NuevoAPPwindowsforms.Models;

namespace NuevoAPPwindowsforms.Forms
{
    public class EmpleadosForm : Form
    {
        private readonly AppData _data;
        private Button btnEditar_Empleado;
        private Button btnNuevo_Empleado;
        public EmpleadosForm(AppData data = null)
        {
            _data = data ?? new AppData();
            this.Text = "Gestión de Empleados";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
        }

        private void InitializeUI()
        {
            var label = new Label
            {
                Text = "GESTIONAR EMPLEADOS",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 14)
            };
            this.Controls.Add(label);
            btnNuevo_Empleado = new Button { Text = "Nuevo Empleado", Left = 50, Top = 100, Width = 150, Height = 40 };
            btnNuevo_Empleado.Click += BtnNuevo_Empleado;
            this.Controls.Add(btnNuevo_Empleado);
            btnEditar_Empleado = new Button { Text = "Editar Empleado", Left = 250, Top = 100, Width = 150, Height = 40 };
            btnEditar_Empleado.Click += BtnEditar_Empleado;
            this.Controls.Add(btnEditar_Empleado);
            // Aquí puedes agregar más controles para la gestión de empleados
        }
        private void BtnNuevo_Empleado(object sender, EventArgs e)
        {
            var form = new EmpleadosRegistro(_data);
            form.ShowDialog();
            //MessageBox.Show("Funcionalidad para agregar un nuevo empleado.");
        }
        private void BtnEditar_Empleado(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad para editar un empleado existente.");
        }
    }
}
