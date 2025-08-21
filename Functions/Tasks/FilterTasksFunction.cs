using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;
using TaskManagement.Factories;
using TaskManagement.Repositories.Implementations;
using TaskManagement.Services.Implementations;
using TaskManagement.Models;

using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement
{
    public static class FilterTasksFunction
    {
        [FunctionName("filter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "task/filter")] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Filter Tasks function processed a request.");

            try
            {
                // 1. Configuración
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // 2. Validar Authorization Header
                if (!req.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    return ResponseBuilder.Unauthorized("Token de autorización requerido");
                }

                var token = authHeader.ToString().Replace("Bearer ", "");
                var jwtValidator = new JwtValidator(config);
                var userId = jwtValidator.GetUserIdFromToken(token);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseBuilder.Unauthorized("Token inválido");
                }

                // 3. Extraer parámetros de filtro de query string
                TaskStatus? status = null;
                TaskPriority? priority = null;
                DateTime? dueDateFrom = null;
                DateTime? dueDateTo = null;
                string category = null;

                // Parsear status
                if (req.Query.TryGetValue("status", out var statusQuery) && 
                    Enum.TryParse<TaskStatus>(statusQuery, true, out var parsedStatus))
                {
                    status = parsedStatus;
                }

                // Parsear priority
                if (req.Query.TryGetValue("priority", out var priorityQuery) && 
                    Enum.TryParse<TaskPriority>(priorityQuery, true, out var parsedPriority))
                {
                    priority = parsedPriority;
                }

                // Parsear dueDateFrom
                if (req.Query.TryGetValue("dueDateFrom", out var dueDateFromQuery) && 
                    DateTime.TryParse(dueDateFromQuery, out var parsedDueDateFrom))
                {
                    dueDateFrom = parsedDueDateFrom;
                }

                // Parsear dueDateTo
                if (req.Query.TryGetValue("dueDateTo", out var dueDateToQuery) && 
                    DateTime.TryParse(dueDateToQuery, out var parsedDueDateTo))
                {
                    dueDateTo = parsedDueDateTo;
                }

                // Obtener category
                if (req.Query.TryGetValue("category", out var categoryQuery))
                {
                    category = categoryQuery.ToString();
                }

                // 4. Inicializar servicios
                var connectionFactory = new SqlConnectionFactory(config);
                var taskRepository = new TaskRepository(connectionFactory);
                var userRepository = new UserRepository(connectionFactory);
                var taskService = new TaskService(taskRepository, userRepository);

                // 5. Filtrar tareas
                var filteredTasks = await taskService.FilterTasksAsync(userId, status, priority, category, dueDateFrom, dueDateTo);

                // 6. Respuesta exitosa
                log.LogInformation($"Se filtraron las tareas exitosamente para el usuario {userId}. Filtros aplicados: Status={status}, Priority={priority}, Category={category}");
                return ResponseBuilder.Success(filteredTasks, "Tareas filtradas exitosamente");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado al filtrar las tareas");
                return ResponseBuilder.InternalServerError("Error interno del servidor");
            }
        }
    }
}
