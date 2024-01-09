using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using HelloFunciton.Models;
using Newtonsoft.Json;

namespace HelloFunciton.Functions
{
    public static class AddProduct
    {
        private const string BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=bohdanvivcharstorage1;AccountKey=T46KZjqd6QRylNfpnGzp4pKUqtnVBek0ST1jw4zLqASg+Rq3EoVx20gAhvY7Kz4wqWUUkJa6MbgH+AStevbjMA==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "products";

        [FunctionName("AddProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function to create a product processed a request.");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Product product = JsonConvert.DeserializeObject<Product>(requestBody);
                if (product == null || string.IsNullOrWhiteSpace(product.Id))
                {
                    return new BadRequestObjectResult("Invalid order data");
                }

                BlobServiceClient client = new(BlobConnectionString);
                BlobContainerClient containerClient = client.GetBlobContainerClient(BlobContainerName);
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient($"product-{product.Id}.json");

                await blobClient.UploadAsync(new BinaryData(requestBody), true);

                return new OkObjectResult($"product with id: {product.Id} added");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
