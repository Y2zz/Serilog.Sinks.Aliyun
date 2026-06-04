using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks;

namespace IConfiguration;

public class Program
{
    public static void Main (string[] args)
    {
        SelfLog.Enable(Console.Error);

        // 构建配置链，来源优先级：环境变量 > UserSecrets > appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<Program>(true)
            .AddEnvironmentVariables("ALIYUN_")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Async(x => x.Console())

            // ─── 通过 IConfiguration 绑定（推荐）──────────────────────────
            // 自动从配置源读取 AliyunSLS 节，环境变量可覆盖同名配置。
            // 若所有关键配置均缺失，Sink 会自动禁用并输出 SelfLog 提醒。
            // -----------------------------------------------------------
            .WriteTo.Async(c => c.AliyunLog(configuration))
            .CreateLogger();

        // 各级别日志
        Log.Logger.Verbose("Verbose 日志");
        Log.Logger.Debug("Debug 日志");
        Log.Logger.Information("Information 日志");
        Log.Logger.Warning("Warning 日志");
        Log.Logger.Error("Error 日志");
        Log.Logger.Fatal("Fatal 日志");

        // 结构化属性
        Log.Logger.Information("用户 {UserId} 执行 {Action} 耗时 {Duration}ms", 10086, "登录", 235);

        // 异常日志
        try
        {
            throw new InvalidOperationException("模拟异常");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "执行 {Operation} 时发生异常", "数据处理");
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
