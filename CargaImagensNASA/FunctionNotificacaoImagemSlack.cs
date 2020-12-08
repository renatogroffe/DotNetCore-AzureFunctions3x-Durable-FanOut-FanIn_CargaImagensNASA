using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using CargaImagensNASA.Models;
using CargaImagensNASA.HttpClients;

namespace CargaImagensNASA
{
    public static class FunctionNotificacaoImagemSlack
    {
        [FunctionName("NotificacaoImagemSlack")]
        public static async Task<string> NotificacaoImagemSlack(
            [ActivityTrigger] InfoImagemNASA infoImagem,
            ILogger log)
        {
            log.LogInformation(
                $"{nameof(NotificacaoImagemSlack)} - Iniciando a execução...");

            if (infoImagem.Media_type == "image")
            {
                await CanalSlackClient.PostNotificacaoImagemAsync(infoImagem);
                
                string resultadoNotificacao =
                    $"{nameof(NotificacaoImagemSlack)} - Notificação para o Slack - Imagem: " +
                    infoImagem.Url;
                log.LogInformation(resultadoNotificacao);
                return resultadoNotificacao;
            }
            else
            {
                string alerta =
                    $"{nameof(NotificacaoImagemSlack)} - Não foi enviada notificação para o Slack - " +
                    $"O link não corresponde a uma imagem: {infoImagem.Url}";
                log.LogInformation(alerta);
                return alerta;
           }
        }
    }
}