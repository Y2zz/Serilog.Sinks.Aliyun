using Aliyun.Api.LOG;
using Aliyun.Api.LOG.Common.Utilities;
using Aliyun.Api.LOG.Data;
using Aliyun.Api.LOG.Request;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.Aliyun;

public class AliyunSink : ILogEventSink
{
    public AliyunSink(AliyunOption option)
    {
        if (string.IsNullOrWhiteSpace(option.AccessKeyId))
        {
            throw new ArgumentNullException(nameof(option.AccessKeyId));
        }

        if (string.IsNullOrWhiteSpace(option.AccessKeySecret))
        {
            throw new ArgumentNullException(nameof(option.AccessKeySecret));
        }

        if (string.IsNullOrWhiteSpace(option.Project))
        {
            throw new ArgumentNullException(option.Project);
        }

        if (string.IsNullOrWhiteSpace(option.Logstore))
        {
            throw new ArgumentNullException(option.Logstore);
        }

        Option = option;

        Client = new LogClient(option.Domain, option.AccessKeyId, option.AccessKeySecret)
        {
            ReadWriteTimeout = option.ReadWriteTimeout
        };
    }

    private LogClient Client { get; }
    private AliyunOption Option { get; }

    public void Emit(LogEvent logEvent)
    {
        var request = new PutLogsRequest
        {
            Project = Option.Project,
            Logstore = Option.Logstore,
            LogItems = new List<LogItem>()
        };

        var logItem = new LogItem
        {
            Time = DateUtils.TimeSpan(),
            Contents = new List<LogContent>
            {
                new(nameof(logEvent.Timestamp), logEvent.Timestamp.ToString("yyyy-MM-dd hh:mm:ss")),
                new(nameof(logEvent.Level), logEvent.Level.ToString()),
                new("Message", logEvent.RenderMessage()),
            }
        };

        // 仅异常时处理
        if (logEvent.Exception != null)
        {
            logItem.PushBack(nameof(logEvent.Exception.Source), logEvent.Exception.Source);
            logItem.PushBack(nameof(logEvent.Exception.Message), logEvent.Exception.Message);
            logItem.PushBack(nameof(logEvent.Exception.StackTrace), logEvent.Exception.StackTrace);
        }

        request.LogItems.Add(logItem);

        Client.PutLogs(request);
    }
}