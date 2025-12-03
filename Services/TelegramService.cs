using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NuevoAPPwindowsforms.Services
{
    public static class TelegramService
    {
        private static readonly string BotToken = "8348012030:AAFfVKfro83hoW4cBiaME5XrMDhwzolK_Nc"; // Reemplaza por tu token real
        private static readonly string ApiUrl = $"https://api.telegram.org/bot{BotToken}/sendMessage";

        // Agrega aquí los chat_id de cada grupo
        public static readonly string ChatIdRegistro = "-5011401284";
        public static readonly string ChatIdVenta = "-5011401284";
        private static readonly string ChatId = "-487675243"; // Reemplaza por tu chat_id real

        public static async Task EnviarMensajeAsync(string mensaje)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent($"chat_id={ChatId}&text={Uri.EscapeDataString(mensaje)}", Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    await client.PostAsync(ApiUrl, content);
                }
                catch (Exception ex)
                {
                    // Puedes loguear el error si lo deseas
                }
            }
        }

        public static async Task EnviarMensajeAGrupoAsync(string mensaje, string chatId)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent($"chat_id={chatId}&text={Uri.EscapeDataString(mensaje)}", Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    await client.PostAsync(ApiUrl, content);
                }
                catch (Exception ex)
                {
                    // Puedes loguear el error si lo deseas
                }
            }
        }
    }
}
