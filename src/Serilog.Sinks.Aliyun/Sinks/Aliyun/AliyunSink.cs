using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;
using Nito.AsyncEx;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.Aliyun;

public class AliyunSink : ILogEventSink
{
    public AliyunSink(AliyunOption option)
    {
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

        if (string.IsNullOrWhiteSpace(option.Project))
        {
            throw new ArgumentNullException(option.Project);
        }

        if (string.IsNullOrWhiteSpace(option.Logstore))
        {
            throw new ArgumentNullException(option.Logstore);
        }

        Option = option;

        Client = LogServiceClientBuilders.HttpBuilder
            .Endpoint(option.Domain, option.Project)
            .Credential(option.AccessKeyId, option.AccessKeySecret)
            .Build();
    }

    private HttpLogServiceClient Client { get; }
    private AliyunOption Option { get; }

    public void Emit(LogEvent logEvent)
    {
        var request = new PostLogsRequest(Option.Logstore, new LogGroupInfo()
        {
            Logs = new List<LogInfo>
            {
                new()
                {
                    Time = DateTime.Now,
                    Contents = new Dictionary<string, string>
                    {
                        { "Level", logEvent.Level.ToString() },
                        { "Message", logEvent.RenderMessage() }
                    }
                }
            }
        });

        // send
        var response = AsyncContext.Run(async ()=> await Client.PostLogStoreLogsAsync(request));
        if (!response.IsSuccess)
        {
            throw new Exception(response.Error.ErrorMessage);
        }
    }
}