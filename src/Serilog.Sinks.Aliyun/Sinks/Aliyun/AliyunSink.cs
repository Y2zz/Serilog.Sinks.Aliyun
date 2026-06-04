using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.Aliyun;

public class AliyunSink : ILogEventSink
{
    private readonly AliyunOption _option;
    private readonly ILogServiceClient? _client;

    public AliyunSink (AliyunOption option)
    {
        _option = option;

        if (!option.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(option.AccessKeyId))
        {
            throw new ArgumentNullException(nameof(option.AccessKeyId));
        }

        if (string.IsNullOrWhiteSpace(option.AccessKeySecret))
        {
            throw new ArgumentNullException(nameof(option.AccessKeySecret));
        }

        if (string.IsNullOrWhiteSpace(option.Domain))
        {
            throw new ArgumentNullException(nameof(option.Domain));
        }

        if (string.IsNullOrWhiteSpace(option.Project))
        {
            throw new ArgumentNullException(nameof(option.Project));
        }

        if (string.IsNullOrWhiteSpace(option.Logstore))
        {
            throw new ArgumentNullException(nameof(option.Logstore));
        }

        _client = LogServiceClientBuilders.HttpBuilder
            .Endpoint($"https://{option.Domain}", option.Project)
            .Credential(option.AccessKeyId, option.AccessKeySecret)
            .Build();
    }

    public void Emit (LogEvent logEvent)
    {
        if (_client == null)
        {
            return;
        }

        try
        {
            var logInfo = new LogInfo
            {
                Time = logEvent.Timestamp,
                Contents = new Dictionary<string, string>
                {
                    ["Level"] = logEvent.Level.ToString(),
                    ["Message"] = logEvent.RenderMessage()
                }
            };

            if (logEvent.Exception != null)
            {
                logInfo.Contents["Exception"] = logEvent.Exception.ToString();
            }

            var logGroup = new LogGroupInfo
            {
                Topic = "",
                Source = "",
                Logs = new List<LogInfo> { logInfo }
            };

            var response = _client.PostLogStoreLogsAsync(_option.Logstore, logGroup)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!response.IsSuccess)
            {
                SelfLog.WriteLine(
                    "[AliyunSink] 发送日志失败, 错误码: {0}, 错误消息: {1}",
                    response.Error.ErrorCode,
                    response.Error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine(
                "[AliyunSink] 发送日志异常: {0}", ex.Message);
            throw;
        }
    }
}
