using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;

namespace TaskManagement
{
    public class GetTasksFunction
    {
        private readonly ITaskService _taskService;
        private readonly JwtValidator _jwtValidator;

        public GetTasksFunction(
            ITaskService taskService,
            JwtValidator jwtValidator)
        {
            _taskService = taskService;
            _jwtValidator = jwtValidator;
        }

        [Function("get")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetTasksFunction");
            logger.LogInformation("Get Tasks function processed a request.");

            try
            {
                // 1. Validar Authorization Header
                if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync("{\"error\": \"Token de autorización requerido\"}");
                    return unauthorizedResponse;
                }

                var token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "");
                var userId = _jwtValidator.GetUserIdFromToken(token);
                
                if (string.IsNullOrEmpty(userId))
                {
                    var invalidTokenResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await invalidTokenResponse.WriteStringAsync("{\"error\": \"Token inválido\"}");
                    return invalidTokenResponse;
                }

                // 2. Obtener todas las tareas del usuario
                var tasks = await _taskService.GetAllTasksAsync(userId);

                // 3. Respuesta exitosa
                logger.LogInformation($"Se obtuvieron las tareas exitosamente para el usuario {userId}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = tasks, 
                    message = "Tareas obtenidas exitosamente" 
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al obtener las tareas");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("{\"error\": \"Error interno del servidor\"}");
                return errorResponse;
            }
        }
    }
}
