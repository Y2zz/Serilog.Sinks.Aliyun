namespace Serilog.Sinks.Aliyun;

public class AliyunOption
{
    /// <summary>
    /// Access Key ID
    /// </summary>
    public string AccessKeyId { get; set; }
    
    /// <summary>
    /// Access Key Secret
    /// </summary>
    public string AccessKeySecret { get; set; }
 
    /// <summary>
    /// 域
    /// </summary>
    public string Domain { get; set; }
    
    /// <summary>
    /// Project
    /// </summary>
    public string Project { get; set; }
    
    /// <summary>
    /// Logstore
    /// </summary>
    public string Logstore { get; set; }

    /// <summary>
    /// 读写超时
    /// 默认值：10000
    /// </summary>
    public int ReadWriteTimeout { get; set; } = 10000;
    
    /// <summary>
    /// 启用
    /// 默认值：true
    /// </summary>
    public bool Enabled { get; set; } = true;
}