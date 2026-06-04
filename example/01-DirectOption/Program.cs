using Serilog;
using Serilog.Debugging;
using Serilog.Sinks;
using Serilog.Sinks.Aliyun;

namespace DirectOption;

public class Program
{
    public static void Main(string[] args)
    {
        SelfLog.Enable(Console.Error);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Async(x => x.Console())

            // ─── 直接传入 AliyunOption ───────────────────────────────────
            // 适用于测试、快速验证或配置完全由代码管理的场景。
            // 注意：AccessKey 建议从环境变量读取，避免硬编码泄密。
            // -----------------------------------------------------------
            .WriteTo.Async(c => c.AliyunLog(new AliyunOption
            {
                Endpoint = "cn-shanghai.log.aliyuncs.com",
                Project = Environment.GetEnvironmentVariable("ALIYUN_SLS_PROJECT"),
                Logstore = Environment.GetEnvironmentVariable("ALIYUN_SLS_LOGSTORE"),
                AccessKeyId = Environment.GetEnvironmentVariable("ALIYUN_SLS_ACCESS_KEY_ID"),
                AccessKeySecret = Environment.GetEnvironmentVariable("ALIYUN_SLS_ACCESS_KEY_SECRET"),
                ReadWriteTimeout = 15000,
            }))

            .CreateLogger();

        Log.Logger.Information("直接传参模式 — 这是一条测试日志");
        Log.Logger.Information("结构化数据: User={UserId}, Action={Action}", 10086, "登录");

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
