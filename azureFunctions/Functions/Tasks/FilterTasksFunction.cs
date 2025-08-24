using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Web;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;
using TaskManagement.Models;

using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement
{
    public class FilterTasksFunction
    {
        private readonly ITaskService _taskService;
        private readonly JwtValidator _jwtValidator;

        public FilterTasksFunction(
            ITaskService taskService,
            JwtValidator jwtValidator)
        {
            _taskService = taskService;
            _jwtValidator = jwtValidator;
        }

        [Function("filter")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task/filter")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("FilterTasksFunction");
            logger.LogInformation("Filter Tasks function processed a request.");

            try
            {
                // 1. Validar Authorization Header
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

                // 2. Extraer parámetros de filtro de query string
                var query = HttpUtility.ParseQueryString(req.Url.Query);
                TaskStatus? status = null;
                TaskPriority? priority = null;
                DateTime? dueDateFrom = null;
                DateTime? dueDateTo = null;
                string category = null;

                // Parsear status
                if (!string.IsNullOrEmpty(query["status"]) && 
                    Enum.TryParse<TaskStatus>(query["status"], true, out var parsedStatus))
                {
                    status = parsedStatus;
                }

                // Parsear priority
                if (!string.IsNullOrEmpty(query["priority"]) && 
                    Enum.TryParse<TaskPriority>(query["priority"], true, out var parsedPriority))
                {
                    priority = parsedPriority;
                }

                // Parsear dueDateFrom
                if (!string.IsNullOrEmpty(query["dueDateFrom"]) && 
                    DateTime.TryParse(query["dueDateFrom"], out var parsedDueDateFrom))
                {
                    dueDateFrom = parsedDueDateFrom;
                }

                // Parsear dueDateTo
                if (!string.IsNullOrEmpty(query["dueDateTo"]) && 
                    DateTime.TryParse(query["dueDateTo"], out var parsedDueDateTo))
                {
                    dueDateTo = parsedDueDateTo;
                }

                // Obtener category
                category = query["category"];

                // 3. Filtrar tareas
                var filteredTasks = await _taskService.FilterTasksAsync(userId, status, priority, category, dueDateFrom, dueDateTo);

                // 4. Respuesta exitosa
                logger.LogInformation($"Se filtraron las tareas exitosamente para el usuario {userId}. Filtros aplicados: Status={status}, Priority={priority}, Category={category}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = filteredTasks, 
                    message = "Tareas filtradas exitosamente" 
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al filtrar las tareas");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Error interno del servidor" }));
                return errorResponse;
            }
        }
    }
}
