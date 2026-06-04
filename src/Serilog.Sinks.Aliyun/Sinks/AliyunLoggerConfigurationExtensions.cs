using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Debugging;
using Serilog.Sinks.Aliyun;

namespace Serilog.Sinks;

/// <summary>
/// 提供写入阿里云 SLS 的 Serilog 扩展方法
/// </summary>
public static class AliyunSlsLoggerConfigurationExtensions
{
    const string DefaultSectionName = "AliyunSLS";
    const string DefaultEnvPrefix = "ALIYUN_";

    /// <summary>
    /// 通过 <see cref="IConfiguration"/> 配置 Aliyun SLS Sink
    /// <br/>
    /// 配置来源优先级：环境变量 > IConfiguration（appsettings.json / UserSecrets）
    /// </summary>
    /// <param name="sinkConfiguration">Serilog sink 配置</param>
    /// <param name="configuration">IConfiguration 实例</param>
    /// <param name="sectionName">配置节名称，默认 "AliyunSLS"</param>
    /// <param name="environmentVariablePrefix">环境变量前缀，默认 "ALIYUN_"，传 null 禁用环境变量覆盖</param>
    public static LoggerConfiguration AliyunLog(
        this LoggerSinkConfiguration sinkConfiguration,
        IConfiguration configuration,
        string sectionName = DefaultSectionName,
        string? environmentVariablePrefix = DefaultEnvPrefix)
    {
        if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var section = configuration.GetSection(sectionName);
        var options = new AliyunOption();
        section.Bind(options);

        // 环境变量覆盖（优先级最高）
        if (environmentVariablePrefix != null)
        {
            OverrideFromEnv(options, environmentVariablePrefix);
        }

        // 所有关键配置均缺失时主动禁用
        if (string.IsNullOrWhiteSpace(options.AccessKeyId)
            && string.IsNullOrWhiteSpace(options.AccessKeySecret)
            && string.IsNullOrWhiteSpace(options.Endpoint)
            && string.IsNullOrWhiteSpace(options.Project)
            && string.IsNullOrWhiteSpace(options.Logstore))
        {
            SelfLog.WriteLine(
                "[AliyunLog] 未检测到任何有效配置（环境变量/配置文件均缺失），Aliyun Sink 已禁用。"
                + "请设置环境变量 {0}ACCESS_KEY_ID 等或在 appsettings.json 中配置 {1} 节。",
                environmentVariablePrefix ?? "",
                sectionName);
            options.Enabled = false;
        }

        return sinkConfiguration.Sink(new AliyunSink(options));
    }

    /// <summary>
    /// 直接传入 <see cref="AliyunOption"/> 配置
    /// </summary>
    public static LoggerConfiguration AliyunLog(
        this LoggerSinkConfiguration sinkConfiguration,
        AliyunOption aliyunOption)
    {
        if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
        if (aliyunOption == null) throw new ArgumentNullException(nameof(aliyunOption));

        return sinkConfiguration.Sink(new AliyunSink(aliyunOption));
    }

    static void OverrideFromEnv(AliyunOption options, string prefix)
    {
        SetIfEnvExists(prefix + "ACCESS_KEY_ID", v => options.AccessKeyId = v);
        SetIfEnvExists(prefix + "ACCESS_KEY_SECRET", v => options.AccessKeySecret = v);
        SetIfEnvExists(prefix + "ENDPOINT", v => options.Endpoint = v);
        SetIfEnvExists(prefix + "PROJECT", v => options.Project = v);
        SetIfEnvExists(prefix + "LOGSTORE", v => options.Logstore = v);
        SetIfEnvExists(prefix + "READ_WRITE_TIMEOUT", v =>
        {
            if (int.TryParse(v, out var t)) options.ReadWriteTimeout = t;
        });
        SetIfEnvExists(prefix + "ENABLED", v =>
        {
            if (bool.TryParse(v, out var e)) options.Enabled = e;
        });

        static void SetIfEnvExists(string key, Action<string> setter)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value))
            {
                setter(value);
            }
        }
    }
}
