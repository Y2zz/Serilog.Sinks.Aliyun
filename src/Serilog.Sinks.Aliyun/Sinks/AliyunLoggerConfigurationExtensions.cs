using Serilog.Configuration;
using Serilog.Sinks.Aliyun;

namespace Serilog.Sinks;

public static class AliyunSlsLoggerConfigurationExtensions
{
    public static LoggerConfiguration AliyunLog(this LoggerSinkConfiguration sinkConfiguration,
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
}