using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CargaImagensNASA.Models;

namespace CargaImagensNASA.HttpClients
{
    public static class CanalSlackClient
    {
        public static async Task PostNotificacaoImagemAsync(InfoImagemNASA infoImagem)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(
                    Environment.GetEnvironmentVariable("UrlLogicAppNotificacao"));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var requestMessage =
                      new HttpRequestMessage(HttpMethod.Post, String.Empty);

                requestMessage.Content = new StringContent(
                    JsonSerializer.Serialize(new ParametrosSlack()
                    {
                        data = infoImagem.Date,
                        titulo = infoImagem.Title,
                        detalhes = infoImagem.Explanation,
                        urlImagem = infoImagem.Url
                    }), Encoding.UTF8, "application/json");

                var respLogicApp = await client.SendAsync(requestMessage);
                respLogicApp.EnsureSuccessStatusCode();
            }
        }
    }
}