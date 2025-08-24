using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Config;
using TaskManagement.Data;
using TaskManagement.Data.Seed;
using TaskManagement.Repositories.Implementations;
using TaskManagement.Repositories.Interfaces;
using TaskManagement.Services.Implementations;
using TaskManagement.Services.Interfaces;
using TaskManagement.Utils;

namespace TaskManagement
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // Load local.settings.json for local development
                    config.SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                          .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // Register configuration
                    services.AddSingleton<IConfiguration>(configuration);
                    services.Configure<AppSettings>(configuration.GetSection("Values"));

                    // Resolve connection string with robust fallback logic
                    var connectionString = GetConnectionString(configuration);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException(
                            "Connection string 'DefaultConnection' not found. Please ensure it's configured in " +
                            "ConnectionStrings:DefaultConnection, Values:ConnectionStrings:DefaultConnection, or " +
                            "Values:ConnectionStrings__DefaultConnection");
                    }

                    // Determine database provider (default to SQL Server)
                    var dbProvider = configuration.GetValue<string>("Values:DbProvider") ?? "SqlServer";
                    
                    // Register Entity Framework
                    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
                    {
                        var logger = serviceProvider.GetService<ILogger<Program>>();
                        
                        switch (dbProvider.ToLower())
                        {
                            case "sqlite":
                                logger?.LogInformation("Using SQLite provider for development");
                                options.UseSqlite(connectionString);
                                break;
                            case "sqlserver":
                            default:
                                logger?.LogInformation("Using SQL Server provider");
                                options.UseSqlServer(connectionString, sqlOptions =>
                                {
                                    sqlOptions.EnableRetryOnFailure(
                                        maxRetryCount: 3,
                                        maxRetryDelay: TimeSpan.FromSeconds(30),
                                        errorNumbersToAdd: null);
                                });
                                break;
                        }

                        // Enable sensitive data logging in development
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            options.EnableSensitiveDataLogging();
                            options.EnableDetailedErrors();
                        }
                    });

                    // Register DbContextFactory for scenarios requiring multiple contexts
                    services.AddDbContextFactory<AppDbContext>();

                    // Register database initialization service
                    services.AddHostedService<DbInitializerHostedService>();
                    
                    // Register repositories (now using EF Core)
                    services.AddScoped<ITaskRepository, TaskRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    
                    // Register services
                    services.AddScoped<ITaskService, TaskService>();
                    
                    // Register utilities
                    services.AddScoped<JwtValidator>();

                    // Add logging
                    services.AddLogging();
                })
                .Build();

            host.Run();
        }

        private static string? GetConnectionString(IConfiguration configuration)
        {
            // Try multiple configuration paths for connection string
            return configuration.GetConnectionString("DefaultConnection") ??
                   configuration.GetValue<string>("ConnectionStrings:DefaultConnection") ??
                   configuration.GetValue<string>("Values:ConnectionStrings:DefaultConnection") ??
                   configuration.GetValue<string>("Values:ConnectionStrings__DefaultConnection");
        }
    }
}
