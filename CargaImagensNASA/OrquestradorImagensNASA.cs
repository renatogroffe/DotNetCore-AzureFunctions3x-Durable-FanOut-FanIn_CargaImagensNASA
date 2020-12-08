using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using CargaImagensNASA.Models;

namespace CargaImagensNASA
{
    public static class OrquestradorImagensNASA
    {
        [FunctionName("OrquestradorImagensNASA")]
        public static async Task<ResultadoExecucao> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var parametrosExecucao = context.GetInput<ParametrosExecucao>();

            var info = await context.CallActivityAsync<InfoImagemNASA>(
                "ObterDadosImagemPorData", parametrosExecucao);
            
            var tasks = new Task<string>[3];
            tasks[0] = context.CallActivityAsync<string>("UploadImagemToStorage",
                new ParametrosUploadImagem() { UtilizarImagemHD = true, InformacoesImagemNASA = info } );
            tasks[1] = context.CallActivityAsync<string>("UploadImagemToStorage",
                new ParametrosUploadImagem() { UtilizarImagemHD = false, InformacoesImagemNASA = info } );
            tasks[2] = context.CallActivityAsync<string>("NotificacaoImagemSlack", info);

            await Task.WhenAll(tasks);
            
            return new ResultadoExecucao()
            {
                CodDataReferencia = parametrosExecucao.CodDataReferencia,
                Inicio = parametrosExecucao.Inicio,
                Termino = DateTime.Now,
                AcoesParalelasExecutadas = tasks.Select(t => t.Result).ToArray()
            };
        }
    }
}