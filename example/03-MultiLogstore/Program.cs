using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks;

namespace MultiLogstore;

public class Program
{
    public static void Main(string[] args)
    {
        SelfLog.Enable(Console.Error);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables("ALIYUN_")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Async(x => x.Console())

            // ─── 常规日志 → AliyunSLS 节 ──────────────────────────────────
            .WriteTo.Async(c => c.AliyunLog(configuration))

            // ─── 错误日志 → AliyunSLS_Error 节（独立 Logstore）──────────
            // 仅 Error 及以上级别写入，不影响主日志库
            .WriteTo.Async(c => c.AliyunLog(
                configuration,
                sectionName: "AliyunSLS_Error"))

            // ─── 调试日志（已禁用，仅演示配置效果）───────────────────────
            // AliyunSLS_Debug 节中 Enabled=false，Sink 初始化时自动跳过
            .WriteTo.Async(c => c.AliyunLog(
                configuration,
                sectionName: "AliyunSLS_Debug"))

            .CreateLogger();

        Log.Logger.Information("常规日志 — 写入 AliyunSLS Logstore");
        Log.Logger.Error("错误日志 — 写入 AliyunSLS_Error Logstore");
        Log.Logger.Debug("调试日志 — 被 AliyunSLS_Debug 的 Enabled=false 丢弃");

        // 环境变量覆盖示例：运行前设置环境变量可临时切换目标
        // export ALIYUN_ENDPOINT=cn-beijing.log.aliyuncs.com
        // export ALIYUN_PROJECT=my-project-test
        // dotnet run --project example/03-MultiLogstore

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
