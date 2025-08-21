using Microsoft.Extensions.Configuration;

namespace TaskManagement.Config
{
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public AzureAdSettings AzureAd { get; set; } = new();
        public EmailSettings Email { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
        public FeatureFlags FeatureFlags { get; set; } = new();

        public static AppSettings LoadFromConfiguration(IConfiguration configuration)
        {
            var settings = new AppSettings();
            configuration.Bind(settings);
            return settings;
        }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public bool EnableRetryOnFailure { get; set; } = true;
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelay { get; set; } = 30;
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class AzureAdSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string Instance { get; set; } = "https://login.microsoftonline.com/";
        public string Domain { get; set; }
        public string CallbackPath { get; set; } = "/signin-oidc";
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool EnableSsl { get; set; } = true;
    }

    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Information";
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = false;
        public string LogFilePath { get; set; } = "logs/app.log";
        public bool EnableApplicationInsights { get; set; } = false;
        public string ApplicationInsightsConnectionString { get; set; }
    }

    public class FeatureFlags
    {
        public bool EnableTaskNotifications { get; set; } = true;
        public bool EnableTaskAttachments { get; set; } = false;
        public bool EnableTaskComments { get; set; } = false;
        public bool EnableAdvancedFiltering { get; set; } = true;
        public bool EnableTaskTemplates { get; set; } = false;
        public bool EnableBulkOperations { get; set; } = false;
        public bool EnableTaskAnalytics { get; set; } = true;
    }

    public static class ConfigurationExtensions
    {
        public static AppSettings GetAppSettings(this IConfiguration configuration)
        {
            return AppSettings.LoadFromConfiguration(configuration);
        }

        public static T GetSection<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            var section = new T();
            configuration.GetSection(sectionName).Bind(section);
            return section;
        }
    }
} 