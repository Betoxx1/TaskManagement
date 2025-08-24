using System;
using System.IO;
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
    public class CreateTaskFunction
    {
        private readonly ITaskService _taskService;
        private readonly JwtValidator _jwtValidator;

        public CreateTaskFunction(
            ITaskService taskService,
            JwtValidator jwtValidator)
        {
            _taskService = taskService;
            _jwtValidator = jwtValidator;
        }

        [Function("create")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "task/create")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateTaskFunction");
            logger.LogInformation("Create Task function processed a request.");

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

                // 2. Leer y validar el body
                string requestBody = await req.ReadAsStringAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("{\"error\": \"Body de la solicitud es requerido\"}");
                    return badRequestResponse;
                }

                var createTaskDto = JsonSerializer.Deserialize<CreateTaskDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (createTaskDto == null)
                {
                    var invalidFormatResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await invalidFormatResponse.WriteStringAsync("{\"error\": \"Formato de datos inválido\"}");
                    return invalidFormatResponse;
                }

                // 3. Validaciones de negocio
                if (string.IsNullOrWhiteSpace(createTaskDto.Title))
                {
                    var titleRequiredResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await titleRequiredResponse.WriteStringAsync("{\"error\": \"El título es requerido\"}");
                    return titleRequiredResponse;
                }

                // 4. Crear la tarea
                var createdTask = await _taskService.CreateTaskAsync(createTaskDto, userId);

                // 5. Respuesta exitosa
                logger.LogInformation($"Tarea creada exitosamente con ID: {createdTask.Id}");
                
                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = createdTask, 
                    message = "Tarea creada exitosamente" 
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
                logger.LogError(ex, "Error inesperado al crear la tarea");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("{\"error\": \"Error interno del servidor\"}");
                return errorResponse;
            }
        }
    }
}
