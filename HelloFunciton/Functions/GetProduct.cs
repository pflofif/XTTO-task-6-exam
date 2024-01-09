using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;

namespace HelloFunciton.Functions
{
    public static class GetProduct
    {
        private const string BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=bohdanvivcharstorage1;AccountKey=T46KZjqd6QRylNfpnGzp4pKUqtnVBek0ST1jw4zLqASg+Rq3EoVx20gAhvY7Kz4wqWUUkJa6MbgH+AStevbjMA==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "products";

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "product/{id}")] HttpRequest req,
            string id, ILogger log)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
                }

                BlobServiceClient client = new(BlobConnectionString);
                BlobContainerClient containerClient = client.GetBlobContainerClient(BlobContainerName);

                if((await containerClient.ExistsAsync()) == false)
                {
                    throw new Exception("Container is not exist");
                }

                BlobClient blobClient = containerClient.GetBlobClient($"product-{id}.json");

                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadAsync();
                    using var reader = new StreamReader(response.Value.Content);

                    string productData = await reader.ReadToEndAsync();
                    return new OkObjectResult(productData);
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
