using Serilog;
using Serilog.Sinks;
using Serilog.Sinks.Aliyun;

namespace Sample;

public class Program
{
    public static void Main(string[] args)
    {
        var aliyunOption = new AliyunOption
        {
            AccessKeyId = "",
            AccessKeySecret = "",
            Domain = "",
            Project = "",
            Logstore = ""
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Async(x=> x.Console())
            .WriteTo.Async(c=> c.AliyunLog(aliyunOption))
            .CreateLogger();
        
        Log.Logger.Debug("这是一条日志");
        Console.ReadKey();
    }
}

