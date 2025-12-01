namespace NuevoAPPwindowsforms.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NumeroTelefono { get; set; } = string.Empty;
        public int Edad { get; set; }
        public byte[]? HuellaDactilar { get; set; }
    }
}
