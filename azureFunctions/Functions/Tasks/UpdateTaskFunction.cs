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
using TaskManagement.DTOs;

namespace TaskManagement
{
    public class UpdateTaskFunction
    {
        private readonly ITaskService _taskService;
        private readonly JwtValidator _jwtValidator;

        public UpdateTaskFunction(
            ITaskService taskService,
            JwtValidator jwtValidator)
        {
            _taskService = taskService;
            _jwtValidator = jwtValidator;
        }

        [Function("update")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "task/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateTaskFunction");
            logger.LogInformation("Update Task function processed a request.");

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

                // 3. Leer y validar el body
                string requestBody = await req.ReadAsStringAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Body de la solicitud es requerido" }));
                    return badRequestResponse;
                }

                var updateTaskDto = JsonSerializer.Deserialize<UpdateTaskDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (updateTaskDto == null)
                {
                    var invalidFormatResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await invalidFormatResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Formato de datos inválido" }));
                    return invalidFormatResponse;
                }

                // 4. Actualizar la tarea
                var updatedTask = await _taskService.UpdateTaskAsync(taskId, updateTaskDto, userId);

                if (updatedTask == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = $"No se encontró la tarea con ID {taskId} o no pertenece al usuario actual" }));
                    return notFoundResponse;
                }

                // 5. Respuesta exitosa
                logger.LogInformation($"Tarea {taskId} actualizada exitosamente para usuario {userId}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = updatedTask, 
                    message = "Tarea actualizada exitosamente" 
                }));
                return response;
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning($"Error de validación: {ex.Message}");
                var validationErrorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await validationErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return validationErrorResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al actualizar la tarea");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Error interno del servidor" }));
                return errorResponse;
            }
        }
    }
}
