using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TaskManagement.Utils
{
    public static class ResponseBuilder
    {
        public static IActionResult Success<T>(T data, string message = "Operación exitosa")
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            return new OkObjectResult(response);
        }

        public static IActionResult Success(string message = "Operación exitosa")
        {
            var response = new ApiResponse
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new OkObjectResult(response);
        }

        public static IActionResult Created<T>(T data, string message = "Recurso creado exitosamente")
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            return new ObjectResult(response) { StatusCode = StatusCodes.Status201Created };
        }

        public static IActionResult BadRequest(string message = "Solicitud inválida", object errors = null)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };

            return new BadRequestObjectResult(response);
        }

        public static IActionResult NotFound(string message = "Recurso no encontrado")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new NotFoundObjectResult(response);
        }

        public static IActionResult Unauthorized(string message = "No autorizado")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new UnauthorizedObjectResult(response);
        }

        public static IActionResult Forbidden(string message = "Acceso prohibido")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden };
        }

        public static IActionResult InternalServerError(string message = "Error interno del servidor")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        public static IActionResult Conflict(string message = "Conflicto en la solicitud")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return new ConflictObjectResult(response);
        }

        public static IActionResult ValidationError(object validationErrors, string message = "Errores de validación")
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = validationErrors,
                Timestamp = DateTime.UtcNow
            };

            return new BadRequestObjectResult(response);
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }

    public class PaginatedResponse<T> : ApiResponse<T>
    {
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
    }
} 