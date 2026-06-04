using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.Aliyun;

/// <summary>
/// 阿里云 SLS Serilog Sink，将日志事件写入阿里云日志服务
/// </summary>
public class AliyunSink : ILogEventSink
{
    readonly AliyunOption _option;
    readonly ILogServiceClient? _client;

    public AliyunSink(AliyunOption option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));

        if (!option.Enabled)
        {
            SelfLog.WriteLine("[AliyunSink] Sink 已被禁用 (Enabled=false)");
            return;
        }

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(option.AccessKeyId)) missingFields.Add(nameof(option.AccessKeyId));
        if (string.IsNullOrWhiteSpace(option.AccessKeySecret)) missingFields.Add(nameof(option.AccessKeySecret));
        if (string.IsNullOrWhiteSpace(option.Endpoint)) missingFields.Add(nameof(option.Endpoint));
        if (string.IsNullOrWhiteSpace(option.Project)) missingFields.Add(nameof(option.Project));
        if (string.IsNullOrWhiteSpace(option.Logstore)) missingFields.Add(nameof(option.Logstore));

        if (missingFields.Count > 0)
        {
            SelfLog.WriteLine(
                "[AliyunSink] 以下配置缺失: {0}，Aliyun Sink 已禁用",
                string.Join(", ", missingFields));
            _option.Enabled = false;
            return;
        }

        _client = LogServiceClientBuilders.HttpBuilder
            .Endpoint($"https://{option.Endpoint}", option.Project)
            .Credential(option.AccessKeyId, option.AccessKeySecret)
            .RequestTimeout(option.ReadWriteTimeout)
            .Build();
    }

    /// <summary>
    /// 发送日志事件到阿里云 SLS
    /// </summary>
    public void Emit(LogEvent logEvent)
    {
        if (_client == null) return;

        try
        {
            var logInfo = BuildLogInfo(logEvent);

            var logGroup = new LogGroupInfo
            {
                Topic = string.Empty,
                Source = string.Empty,
                Logs = new List<LogInfo> { logInfo }
            };

            var response = _client.PostLogStoreLogsAsync(_option.Logstore, logGroup)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (!response.IsSuccess)
            {
                SelfLog.WriteLine(
                    "[AliyunSink] 发送日志失败, ErrorCode: {0}, ErrorMessage: {1}",
                    response.Error?.ErrorCode,
                    response.Error?.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine(
                "[AliyunSink] 发送日志异常: {0}", ex);
        }
    }

    static LogInfo BuildLogInfo(LogEvent logEvent)
    {
        var contents = new Dictionary<string, string>
        {
            ["Level"] = logEvent.Level.ToString(),
            ["Message"] = logEvent.RenderMessage(),
            ["MessageTemplate"] = logEvent.MessageTemplate.Text,
        };

        if (logEvent.Exception != null)
        {
            contents["ExceptionType"] = logEvent.Exception.GetType().FullName ?? "Unknown";
            contents["ExceptionMessage"] = logEvent.Exception.Message;
            contents["ExceptionStackTrace"] = logEvent.Exception.ToString();
        }

        foreach (var property in logEvent.Properties.Where(property => !contents.ContainsKey(property.Key)))
        {
            contents[property.Key] = property.Value?.ToString() ?? "null";
        }

        return new LogInfo
        {
            Time = logEvent.Timestamp,
            Contents = contents
        };
    }
}
