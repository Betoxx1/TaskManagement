using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TaskManagement
{
    public class AppHealtFunction
    {
        [Function("health")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AppHealtFunction");
            logger.LogInformation("Health check endpoint accessed.");

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

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = healthResponse, 
                    message = "Health check exitoso" 
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en health check");
                
                var errorResponse = new
                {
                    status = "ERROR",
                    message = "Error en el sistema",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    error = ex.Message
                };

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = false, 
                    data = errorResponse, 
                    message = "Error en health check" 
                }));
                return response;
            }
        }
    }
}
