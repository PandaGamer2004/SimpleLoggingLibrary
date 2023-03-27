using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LoggingLibrary.MicrosoftDI;

public static class MicrosoftDependencyInjectionExtension  
{
    private const string DefaultLogFilePath = "logger.configuration.json";

    private const int DefaultWritesThresholdToReplication = 100;
    public static IHostBuilder AddLogging(this IHostBuilder hostBuilder,
        Action<LoggingConfiguration> configureLogging)
    {
        var loggingConfiguration = new LoggingConfiguration
        {
            LogsFilePath = DefaultLogFilePath,
            LogEntriesWritesForReplicationThreshold = DefaultWritesThresholdToReplication 
        };
        configureLogging(loggingConfiguration);
        hostBuilder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddJsonFile(loggingConfiguration.LogsFilePath, optional:false);
        }).ConfigureServices((hostContext, serviceCollection) =>
        {
            IConfiguration appConfiguration = hostContext.Configuration;
        });

        return hostBuilder;
    }
}