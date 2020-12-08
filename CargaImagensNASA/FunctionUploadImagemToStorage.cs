using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using CargaImagensNASA.Models;
using CargaImagensNASA.Data;
using CargaImagensNASA.Documents;

namespace CargaImagensNASA
{
    public static class FunctionUploadImagemToStorage
    {
        [FunctionName("UploadImagemToStorage")]
        public static async Task<string> UploadImagemToStorage(
            [ActivityTrigger] ParametrosUploadImagem parametrosUpload,
            ILogger log)
        {
            log.LogInformation(
                $"{nameof(UploadImagemToStorage)} - Iniciando a execução...");

            var infoImagem = parametrosUpload.InformacoesImagemNASA;
            string urlImagem =
                (parametrosUpload.UtilizarImagemHD? infoImagem.Hdurl : infoImagem.Url);
            string nomeArquivoImagem = Path.GetFileName(urlImagem);

            if (infoImagem.Media_type == "image")
            {
                var storageAccount = CloudStorageAccount.Parse(
                    Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("imagens-nasa-paralelismo");
                await container.CreateIfNotExistsAsync();
                var dataAtual = DateTime.Now;
                var blockBlob = container.GetBlockBlobReference(
                    $"{dataAtual:yyyyMMdd_HHmmss}-{infoImagem.Date}-{nomeArquivoImagem}");

                using var client = new HttpClient();
                using var stream = await client.GetStreamAsync(urlImagem);
                await blockBlob.UploadFromStreamAsync(stream);

                ImagemNASARepository.SaveInfoImagem(new ImagemNASADocument()
                {
                    Data = infoImagem.Date,
                    DataUpload = dataAtual.ToString("yyyy-MM-dd HH:mm:ss"),
                    Titulo = infoImagem.Title,
                    Detalhes = infoImagem.Explanation,
                    UrlImagem = urlImagem,
                    HD = parametrosUpload.UtilizarImagemHD,
                    BlobName = blockBlob.Name,
                    BlobContainer = container.Name,
                    Copyright = infoImagem.Copyright,
                    MediaType = infoImagem.Media_type,
                    ServiceVersion = infoImagem.Service_version
                });
                
                string resultadoUpload =
                    $"{nameof(UploadImagemToStorage)} - O arquivo {nomeArquivoImagem} " +
                    $"foi carregado no Blob Container {container.Name} com o nome {blockBlob.Name}";
                log.LogInformation(resultadoUpload);
                return resultadoUpload;
            }
            else
            {
                string alerta =
                    $"{nameof(UploadImagemToStorage)} - O arquivo {nomeArquivoImagem} " +
                    "não será carregado pois não corresponde a uma imagem";
                log.LogInformation(alerta);
                return alerta;
           }
        }
    }
}