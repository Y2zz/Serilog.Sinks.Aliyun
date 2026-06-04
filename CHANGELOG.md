# 1.2.0 发行日志

## 新特性

- **配置文件绑定**：支持从 `appsettings.json` 等 `IConfiguration` 源读取 SLS 配置，无需硬编码 ([AliyunLoggerConfigurationExtensions.cs](src/Serilog.Sinks.Aliyun/Sinks/AliyunLoggerConfigurationExtensions.cs))
- **机密配置管理**：支持 UserSecrets + 环境变量多层回退，敏感信息（AccessKeyId / AccessKeySecret）无需写入配置文件
- **调试能力增强**：集成 `SelfLog`，当日志写入失败时输出详细错误信息（错误码、错误消息、异常堆栈）

##  Breaking Changes

- **移除自研 HTTP 封装，回归官方 SDK**：放弃 `protobuf-net` 手写序列化/GZip/签名实现，改用官方 [`aliyun-log-dotnetcore-sdk`](https://www.nuget.org/packages/aliyun-log-dotnetcore-sdk) 1.1.2
- **依赖变更**：
  - 移除 `protobuf-net` 3.2.8
  - 新增 `aliyun-log-dotnetcore-sdk` 1.1.2
- **配置节名称**：默认配置节从 `AliyunOption` 改为 `AliyunSLS`

## Bug 修复

- **HTTPS 连接修复**：SDK 默认使用 HTTP（端口 80）连接阿里云，现显式指定 `https://` 前缀，正确使用 HTTPS（端口 443）
- **Enabled=false NPE**：修复 `AliyunOption.Enabled = false` 时 Sink 构造函数提前返回导致的空引用异常
- **Async 异常可见性**：解决 `Serilog.Sinks.Async` 静默吞噬 `Emit()` 异常的问题，通过 `SelfLog.WriteLine` 暴露错误详情

## 文件变更

| 文件 | 变更 |
|---|---|
| `README.md` | 新增使用说明 |
| `src/Serilog.Sinks.Aliyun.csproj` | `aliyun-log-dotnetcore-sdk` 1.1.2，移除 `protobuf-net` |
| `AliyunSink.cs` | 重写为 `ILogServiceClient.PostLogStoreLogsAsync` |
| `AliyunOption.cs` | 属性细化 |
| `AliyunLoggerConfigurationExtensions.cs` | 新增 `IConfiguration` 重载 + `GetSecretValue()` |
| `appsettings.json` | 配置节改为 `AliyunSLS` |
| `Program.cs` | 添加 SelfLog、UserSecrets、多环境支持 |
| `Sample.csproj` | 添加配置包，多目标框架 |

## 依赖

- .NET Standard 2.0 / 2.1
- `aliyun-log-dotnetcore-sdk` >= 1.1.2
- `Serilog` >= 4.3.1
- `Microsoft.Extensions.Configuration.Abstractions` >= 6.0.0
