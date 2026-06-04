using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks;

namespace Sample;

public class Program
{
    public static void Main (string[] args)
    {
        // 开启 SelfLog，让被 Async 包装器吞掉的错误也能输出到控制台
        SelfLog.Enable(Console.Error);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<Program>(true) // 开发环境机密
            .AddEnvironmentVariables("ALIYUN_") // CI/CD 环境变量
            .Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Async(x => x.Console())

            // 方式一：通过 IConfiguration 配置链（推荐），支持 UserSecrets + 环境变量
            .WriteTo.Async(c => c.AliyunLog(configuration))

            // 方式二：通过环境变量前缀直接回退（不依赖配置链），例如设置 ALIYUN_ACCESS_KEY_ID=xxx
            // .WriteTo.Async(c => c.AliyunLog(configuration, environmentVariablePrefix: "ALIYUN_"))
            .CreateLogger();

        Log.Logger.Debug("这是一条日志");
        Console.ReadKey();
    }
}
