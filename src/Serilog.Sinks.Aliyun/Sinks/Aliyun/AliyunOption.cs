namespace Serilog.Sinks.Aliyun;

/// <summary>
/// 阿里云 SLS 日志服务配置选项
/// </summary>
public class AliyunOption
{
    /// <summary>
    /// Access Key ID，建议通过环境变量或 UserSecrets 配置
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// Access Key Secret，建议通过环境变量或 UserSecrets 配置
    /// </summary>
    public string? AccessKeySecret { get; set; }

    /// <summary>
    /// 服务入口域名，例如 cn-shanghai.log.aliyuncs.com
    /// <br/>
    /// SDK 内部会自动拼接 HTTPS 协议前缀，无需手动添加
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// 日志项目名称（Project）
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// 日志库名称（Logstore）
    /// </summary>
    public string? Logstore { get; set; }

    /// <summary>
    /// HTTP 读写超时时间（毫秒）
    /// 默认值：10000
    /// </summary>
    public int ReadWriteTimeout { get; set; } = 10000;

    /// <summary>
    /// 是否启用 Sink
    /// 默认值：true
    /// 当配置缺失时自动置为 false
    /// </summary>
    public bool Enabled { get; set; } = true;
}
