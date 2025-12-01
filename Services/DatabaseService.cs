using System;
using System.Data.SQLite;
using System.IO;

namespace NuevoAPPwindowsforms.Services
{
    public static class DatabaseService
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientes.db");
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                // Crear tablas si no existen
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Cliente (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT,
    Apellido TEXT,
    Correo TEXT,
    Edad INTEGER,
    Telefono TEXT
);
CREATE TABLE IF NOT EXISTS Huella (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER,
    Dedo TEXT,
    Template BLOB,
    FOREIGN KEY (ClienteId) REFERENCES Cliente(Id)
);
CREATE TABLE IF NOT EXISTS Ventas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Id_Cliente INTEGER,
    Fecha TEXT,
    Total_Venta REAL,
    FOREIGN KEY (Id_Cliente) REFERENCES Cliente(Id)
);
CREATE TABLE IF NOT EXISTS Detalles_venta (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Id_Venta INTEGER,
    Producto TEXT,
    Precio REAL,
    Fecha_Compra TEXT,
    FOREIGN KEY (Id_Venta) REFERENCES Ventas(Id)
);
CREATE TABLE IF NOT EXISTS Membresias (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Id_Cliente INTEGER NOT NULL,
    Fecha_Inicio TEXT NOT NULL,
    Fecha_Fin TEXT NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (Id_Cliente) REFERENCES Cliente(Id)
);
CREATE TABLE IF NOT EXISTS Empleado (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT,
    Apellido TEXT,
    Correo TEXT,
    Edad INTEGER,
    Telefono TEXT
);
";
                cmd.ExecuteNonQuery();
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public static int InsertEmpleado(string nombre, string apellido, string correo, int edad, string telefono)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Empleado (Nombre, Apellido, Correo, Edad, Telefono) VALUES (@nombre, @apellido, @correo, @edad, @telefono); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@edad", edad);
                cmd.Parameters.AddWithValue("@telefono", telefono);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
        public static int InsertCliente(string nombre, string apellido, string correo, int edad, string telefono)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Cliente (Nombre, Apellido, Correo, Edad, Telefono) VALUES (@nombre, @apellido, @correo, @edad, @telefono); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@edad", edad);
                cmd.Parameters.AddWithValue("@telefono", telefono);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void InsertHuella(int clienteId, string dedo, byte[] template)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Huella (ClienteId, Dedo, Template) VALUES (@clienteId, @dedo, @template);";
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.Parameters.AddWithValue("@dedo", dedo);
                cmd.Parameters.AddWithValue("@template", template);
                cmd.ExecuteNonQuery();
            }
        }

        public static int InsertVenta(int idCliente, DateTime fecha, decimal totalVenta)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Ventas (Id_Cliente, Fecha, Total_Venta) VALUES (@idCliente, @fecha, @total); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@total", totalVenta);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void InsertDetalleVenta(int idVenta, string producto, decimal precio, DateTime fechaCompra)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Detalles_venta (Id_Venta, Producto, Precio, Fecha_Compra) VALUES (@idVenta, @producto, @precio, @fechaCompra);";
                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                cmd.Parameters.AddWithValue("@producto", producto);
                cmd.Parameters.AddWithValue("@precio", precio);
                cmd.Parameters.AddWithValue("@fechaCompra", fechaCompra.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertMembresia(int idCliente, DateTime fechaInicio, string producto)
        {
            // Buscar membresía activa
            DateTime? fechaFinActual = null;
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Fecha_Fin FROM Membresias WHERE Id_Cliente = @idCliente AND Activo = 1 AND Fecha_Fin >= @hoy ORDER BY Fecha_Fin DESC LIMIT 1;";
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@hoy", DateTime.Now.ToString("yyyy-MM-dd"));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fechaFinActual = DateTime.Parse(reader["Fecha_Fin"].ToString());
                    }
                }
            }
            // Calcular nueva fecha de inicio y fin según el producto
            DateTime fechaInicioNueva = fechaInicio;
            if (fechaFinActual != null && fechaFinActual > fechaInicio)
            {
                fechaInicioNueva = fechaFinActual.Value.AddDays(1); // día siguiente a la fecha fin actual
            }
            DateTime fechaFinNueva = fechaInicioNueva;
            if (producto == "Mensualidad")
            {
                fechaFinNueva = fechaInicioNueva.AddMonths(1).AddDays(-1);
            }
            else if (producto == "Quincena")
            {
                fechaFinNueva = fechaInicioNueva.AddDays(14); // 15 días contando el actual
            }
            else if (producto == "Visita")
            {
                fechaFinNueva = fechaInicio; // Solo el día actual
            }
            else // Por defecto, mensualidad
            {
                fechaFinNueva = fechaInicioNueva.AddMonths(1).AddDays(-1);
            }
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Membresias (Id_Cliente, Fecha_Inicio, Fecha_Fin, Activo) VALUES (@idCliente, @fechaInicio, @fechaFin, 1);";
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicioNueva.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fechaFin", fechaFinNueva.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
            }
        }

        public static (DateTime? FechaFin, bool Activo) GetMembresiaActiva(int idCliente)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT Fecha_Fin, Activo FROM Membresias WHERE Id_Cliente = @idCliente AND Fecha_Fin >= @hoy AND Activo = 1 ORDER BY Fecha_Fin DESC LIMIT 1;";
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@hoy", DateTime.Now.ToString("yyyy-MM-dd"));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime fechaFin = DateTime.Parse(reader["Fecha_Fin"].ToString());
                        bool activo = Convert.ToInt32(reader["Activo"]) == 1;
                        return (fechaFin, activo);
                    }
                }
            }
            return (null, false);
        }

        public static void DesactivarMembresiasExpiradas()
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Membresias SET Activo = 0 WHERE Fecha_Fin < @hoy;";
                cmd.Parameters.AddWithValue("@hoy", DateTime.Now.ToString("yyyy-MM-dd"));
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarHuellasPorCliente(int clienteId)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Huella WHERE ClienteId = @clienteId;";
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
