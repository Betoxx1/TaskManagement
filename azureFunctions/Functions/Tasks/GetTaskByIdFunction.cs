using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;

namespace TaskManagement
{
    public class GetTaskByIdFunction
    {
        private readonly ITaskService _taskService;
        private readonly JwtValidator _jwtValidator;

        public GetTaskByIdFunction(
            ITaskService taskService,
            JwtValidator jwtValidator)
        {
            _taskService = taskService;
            _jwtValidator = jwtValidator;
        }

        [Function("getById")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetTaskByIdFunction");
            logger.LogInformation("Get Task By ID function processed a request.");

            try
            {
                // 1. Validar y convertir ID
                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int taskId))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "ID de tarea inválido. Debe ser un número entero." }));
                    return badRequestResponse;
                }

                // 2. Validar Authorization Header
                if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Token de autorización requerido" }));
                    return unauthorizedResponse;
                }

                var token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "");
                var userId = _jwtValidator.GetUserIdFromToken(token);
                
                if (string.IsNullOrEmpty(userId))
                {
                    var invalidTokenResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await invalidTokenResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Token inválido" }));
                    return invalidTokenResponse;
                }

                // 3. Obtener la tarea
                var task = await _taskService.GetTaskByIdAsync(taskId, userId);

                if (task == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = $"No se encontró la tarea con ID {taskId} o no pertenece al usuario actual" }));
                    return notFoundResponse;
                }

                // 4. Respuesta exitosa
                logger.LogInformation($"Tarea {taskId} obtenida exitosamente para usuario {userId}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = task, 
                    message = "Tarea obtenida exitosamente" 
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al obtener la tarea");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Error interno del servidor" }));
                return errorResponse;
            }
        }
    }
}
