using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TaskManagement
{
    public static class AppHealtFunction
    {
        [FunctionName("health")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Health check endpoint accessed.");

            try
            {
                var healthResponse = new
                {
                    status = "OK",
                    message = "Sistema funcionando correctamente",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    version = "1.0.0",
                    service = "TaskManagement API",
                    environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development",
                    checks = new
                    {
                        api = "Funcionando",
                        database = "No configurada", // Se actualizará cuando implementes la BD
                        memory = "OK"
                    }
                };

                return new OkObjectResult(healthResponse);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error en health check");
                
                var errorResponse = new
                {
                    status = "ERROR",
                    message = "Error en el sistema",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    error = ex.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
        }
    }
}
