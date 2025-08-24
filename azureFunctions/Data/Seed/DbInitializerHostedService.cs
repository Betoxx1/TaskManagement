using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;
using TaskPriority = TaskManagement.Models.TaskPriority;

namespace TaskManagement.Data.Seed
{
    public class DbInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DbInitializerHostedService> _logger;

        public DbInitializerHostedService(IServiceProvider serviceProvider, ILogger<DbInitializerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database initialization...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Check for pending migrations
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                        pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                    
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Database migrations applied successfully.");
                }
                else
                {
                    _logger.LogInformation("No pending migrations found.");
                }

                // Seed initial data if needed
                await SeedInitialDataAsync(dbContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Database initialization service stopped.");
            return Task.CompletedTask;
        }

        private async Task SeedInitialDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
        {
            try
            {
                // Check if we already have users (avoid re-seeding)
                if (await dbContext.Users.AnyAsync(cancellationToken))
                {
                    _logger.LogInformation("Database already contains users. Skipping seed.");
                    return;
                }

                _logger.LogInformation("Seeding initial data...");

                // Create admin user
                var adminUser = new UserModel
                {
                    Id = "admin-001",
                    Name = "System Administrator",
                    Email = "admin@taskmanagement.com",
                    Role = "Admin",
                    Department = "IT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ProfilePictureUrl = ""
                };

                // Create demo user
                var demoUser = new UserModel
                {
                    Id = "demo-001",
                    Name = "Demo User",
                    Email = "demo@taskmanagement.com",
                    Role = "User",
                    Department = "General",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ProfilePictureUrl = ""
                };

                dbContext.Users.AddRange(adminUser, demoUser);

                // Create some sample tasks (let EF Core auto-generate Ids)
                var sampleTasks = new List<TaskModel>
                {
                    new TaskModel
                    {
                        Title = "Welcome Task",
                        Description = "Complete your profile setup and explore the task management features.",
                        Status = TaskStatus.Pending,
                        Priority = TaskPriority.Medium,
                        UserId = demoUser.Id,
                        Category = "Onboarding",
                        Tags = "",
                        CreatedAt = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(3)
                    },
                    new TaskModel
                    {
                        Title = "System Configuration Review",
                        Description = "Review and update system configuration settings for optimal performance.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        UserId = adminUser.Id,
                        Category = "Maintenance",
                        Tags = "",
                        CreatedAt = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(1)
                    }
                };

                dbContext.Tasks.AddRange(sampleTasks);

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Initial data seeded successfully. Created {UserCount} users and {TaskCount} tasks.", 
                    2, sampleTasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding initial data.");
                throw;
            }
        }
    }
}