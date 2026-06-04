using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Sinks.Aliyun;

namespace Serilog.Sinks;

public static class AliyunSlsLoggerConfigurationExtensions
{
    public static LoggerConfiguration AliyunLog (this LoggerSinkConfiguration sinkConfiguration,
        AliyunOption aliyunOption
    )
    {
        if (aliyunOption == null)
        {
            throw new ArgumentNullException(nameof(aliyunOption));
        }

        var aliyunSlsSink = new AliyunSink(aliyunOption);
        return sinkConfiguration.Sink(aliyunSlsSink);
    }

    public static LoggerConfiguration AliyunLog (this LoggerSinkConfiguration sinkConfiguration,
        IConfiguration configuration,
        string sectionName = "AliyunSLS",
        string? environmentVariablePrefix = null)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        var aliyunOption = new AliyunOption
        {
            AccessKeyId = GetSecretValue(section, "AccessKeyId", environmentVariablePrefix),
            AccessKeySecret = GetSecretValue(section, "AccessKeySecret", environmentVariablePrefix),
            Domain = GetSecretValue(section, "Domain", environmentVariablePrefix),
            Project = GetSecretValue(section, "Project", environmentVariablePrefix),
            Logstore = GetSecretValue(section, "Logstore", environmentVariablePrefix)
        };

        if (int.TryParse(GetSecretValue(section, "ReadWriteTimeout", environmentVariablePrefix), out var timeout))
        {
            aliyunOption.ReadWriteTimeout = timeout;
        }

        if (bool.TryParse(GetSecretValue(section, "Enabled", environmentVariablePrefix), out var enabled))
        {
            aliyunOption.Enabled = enabled;
        }

        return sinkConfiguration.AliyunLog(aliyunOption);
    }

    private static string? GetSecretValue (IConfigurationSection section, string key, string? environmentVariablePrefix)
    {
        var value = section[key];

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (environmentVariablePrefix != null)
        {
            var envKey = $"{environmentVariablePrefix}{key}";
            var envValue = Environment.GetEnvironmentVariable(envKey);

            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
        }

        return null;
    }
}
