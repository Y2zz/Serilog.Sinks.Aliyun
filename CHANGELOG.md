# 1.3.0 发行日志

## Breaking Changes

- **移除 `Domain` 属性**：配置项统一为 `Endpoint`，与阿里云 SLS 官方术语一致
  - `appsettings.json` 中 `"Domain"` 需改为 `"Endpoint"`
  - 代码中 `option.Domain` 需改为 `option.Endpoint`
- **环境变量全大写下划线命名**：从驼峰改为全大写下划线格式
  - `ALIYUN_AccessKeyId` → `ALIYUN_ACCESS_KEY_ID`
  - `ALIYUN_Domain` → `ALIYUN_ENDPOINT`
  - 详见下方环境变量对照表
- **移除无参 `AliyunLog()` 重载**：不再内部自行构建 Configuration，改由调用方传入 `IConfiguration`

## 新特性

- **配置绑定重写**：使用 `IConfigurationSection.Bind()` 替代逐字段手动映射，代码更简洁、兼容性更好
- **富日志内容**：每条日志自动包含 `MessageTemplate`（模板原文）、`ExceptionType` / `ExceptionMessage` / `ExceptionStackTrace`（异常逐字段展开），以及所有 `LogEvent.Properties`
- **超时配置生效**：`ReadWriteTimeout` 通过 `RequestTimeout()` 正确传递给 SDK Client，之前为摆设
- **多 Logstore 示例**：新增 `03-MultiLogstore` 示例，演示多配置节并行写入
- **安全写入**：`Emit` 不再向上抛异常（符合 Serilog 最佳实践），失败时写 SelfLog

## 环境变量对照

| v1.2.x（旧） | v1.3.0（新） |
|---|---|
| `ALIYUN_AccessKeyId` | `ALIYUN_ACCESS_KEY_ID` |
| `ALIYUN_AccessKeySecret` | `ALIYUN_ACCESS_KEY_SECRET` |
| `ALIYUN_Domain` | `ALIYUN_ENDPOINT` |
| `ALIYUN_Project` | `ALIYUN_PROJECT` |
| `ALIYUN_Logstore` | `ALIYUN_LOGSTORE` |
| `ALIYUN_ReadWriteTimeout` | `ALIYUN_READ_WRITE_TIMEOUT` |
| `ALIYUN_Enabled` | `ALIYUN_ENABLED` |

## 依赖变更

- 新增 `Microsoft.Extensions.Configuration.Binder` 6.0.0

## 项目结构调整

- 移除旧 `example/Sample`，拆分为三个独立示例：
  - `example/01-DirectOption` — 直接传参
  - `example/02-IConfiguration` — IConfiguration 绑定（推荐）
  - `example/03-MultiLogstore` — 多 Logstore + 环境变量覆盖

---

# 1.2.0 发行日志

## 新特性

- **配置文件绑定**：支持从 `appsettings.json` 等 `IConfiguration` 源读取 SLS 配置，无需硬编码
- **机密配置管理**：支持 UserSecrets + 环境变量多层回退，敏感信息（AccessKeyId / AccessKeySecret）无需写入配置文件
- **调试能力增强**：集成 `SelfLog`，当日志写入失败时输出详细错误信息（错误码、错误消息、异常堆栈）

## Breaking Changes

- **移除自研 HTTP 封装，回归官方 SDK**：放弃 `protobuf-net` 手写序列化/GZip/签名实现，改用官方 `aliyun-log-dotnetcore-sdk` 1.1.2
- **依赖变更**：
  - 移除 `protobuf-net` 3.2.8
  - 新增 `aliyun-log-dotnetcore-sdk` 1.1.2
- **配置节名称**：默认配置节从 `AliyunOption` 改为 `AliyunSLS`

## Bug 修复

- **HTTPS 连接修复**：SDK 默认使用 HTTP（端口 80）连接阿里云，现显式指定 `https://` 前缀，正确使用 HTTPS（端口 443）
- **Enabled=false NPE**：修复 `AliyunOption.Enabled = false` 时 Sink 构造函数提前返回导致的空引用异常
- **Async 异常可见性**：解决 `Serilog.Sinks.Async` 静默吞噬 `Emit()` 异常的问题，通过 `SelfLog.WriteLine` 暴露错误详情

## 依赖

- .NET Standard 2.0 / 2.1
- `aliyun-log-dotnetcore-sdk` >= 1.1.2
- `Serilog` >= 4.3.1
- `Microsoft.Extensions.Configuration.Abstractions` >= 6.0.0
