# Serilog.Sinks.Aliyun

Serilog 接入阿里云日志服务（SLS）的 Sink 组件。

基于官方 [`aliyun-log-dotnetcore-sdk`](https://www.nuget.org/packages/aliyun-log-dotnetcore-sdk) 实现。

## 安装

```shell
dotnet add package Serilog.Sinks.Aliyun
```

## 配置项

| 属性 | 说明 | 必填 |
|---|---|---|
| `AccessKeyId` | Access Key ID | 是 |
| `AccessKeySecret` | Access Key Secret | 是 |
| `Endpoint` | 服务入口，如 `cn-shanghai.log.aliyuncs.com` | 是 |
| `Project` | 阿里云 SLS 项目名称 | 是 |
| `Logstore` | LogStore 名称 | 是 |
| `ReadWriteTimeout` | 超时时间（毫秒），默认 10000 | 否 |
| `Enabled` | 是否启用，默认 true | 否 |

### 获取正确的 Endpoint 和 Project

1. 登录 [阿里云 SLS 控制台](https://sls.console.aliyun.com/)
2. 进入你的项目，在 **概览** 页找到 **服务入口**
3. `Endpoint` 填入服务入口（区域域名，如 `cn-shanghai.log.aliyuncs.com`）
4. `Project` 填入控制台中的项目名称

> 注意：项目名必须与你在阿里云 SLS 创建的项目名称完全一致，否则会连接失败。

## 使用方式

### 方式一：直接传入配置对象

```csharp
using Serilog;
using Serilog.Sinks.Aliyun;

var option = new AliyunOption
{
    AccessKeyId = Environment.GetEnvironmentVariable("ALIYUN_ACCESS_KEY_ID"),
    AccessKeySecret = Environment.GetEnvironmentVariable("ALIYUN_ACCESS_KEY_SECRET"),
    Endpoint = "cn-shanghai.log.aliyuncs.com",
    Project = "your-project",
    Logstore = "your-logstore"
};

Log.Logger = new LoggerConfiguration()
    .WriteTo.AliyunLog(option)
    .CreateLogger();

Log.Information("这是一条日志");
```

### 方式二：从配置文件读取（推荐）

`appsettings.json`：

```json
{
  "AliyunSLS": {
    "Endpoint": "",
    "Project": "",
    "Logstore": "",
    "ReadWriteTimeout": 10000,
    "Enabled": true
  }
}
```

> 敏感字段（AccessKeyId / AccessKeySecret / Endpoint / Project / Logstore）建议留空，通过 UserSecrets 或环境变量注入。

```csharp
using Microsoft.Extensions.Configuration;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .AddUserSecrets<Program>(true)
    .AddEnvironmentVariables("ALIYUN_")
    .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo.AliyunLog(configuration)
    .CreateLogger();

Log.Information("这是一条日志");
```

配置节名称默认为 `AliyunSLS`，可通过 `sectionName` 参数修改：

```csharp
.WriteTo.AliyunLog(configuration, sectionName: "MySection")
```

### 多 Logstore 配置

支持向多个 Logstore 写入，只需定义不同的配置节：

```json
{
  "AliyunSLS": { "Endpoint": "", "Project": "", "Logstore": "app-logs", ... },
  "AliyunSLS_Error": { "Endpoint": "", "Project": "", "Logstore": "error-logs", ... }
}
```

```csharp
.WriteTo.AliyunLog(configuration)                                   // 默认 AliyunSLS
.WriteTo.AliyunLog(configuration, sectionName: "AliyunSLS_Error")   // 错误日志独立 Logstore
```

## 机密配置管理

支持多层配置回退机制，优先级从高到低：

| 优先级 | 来源 | 示例 |
|---|---|---|
| 1（最高） | 环境变量（库内置回退） | `ALIYUN_ENDPOINT=xxx` |
| 2 | `IConfiguration` 链（appsettings.json / UserSecrets / 环境变量） | |
| 3（最低） | `IConfiguration` 链中的 appsettings.json | `"Endpoint": ""` |

### UserSecrets（开发环境）

```shell
dotnet user-secrets init
dotnet user-secrets set "AliyunSLS:AccessKeyId" "your-access-key-id"
dotnet user-secrets set "AliyunSLS:AccessKeySecret" "your-access-key-secret"
dotnet user-secrets set "AliyunSLS:Endpoint" "cn-shanghai.log.aliyuncs.com"
dotnet user-secrets set "AliyunSLS:Project" "your-project"
dotnet user-secrets set "AliyunSLS:Logstore" "your-logstore"
```

### 环境变量（CI/CD）

库内置环境变量回退（默认前缀 `ALIYUN_`），与 .NET 配置链独立：

```shell
export ALIYUN_ACCESS_KEY_ID=your-access-key-id
export ALIYUN_ACCESS_KEY_SECRET=your-access-key-secret
export ALIYUN_ENDPOINT=cn-shanghai.log.aliyuncs.com
export ALIYUN_PROJECT=your-project
export ALIYUN_LOGSTORE=your-logstore
```

也可通过扩展方法自定义前缀：

```csharp
.WriteTo.AliyunLog(configuration, environmentVariablePrefix: "MYAPP_")
```

## 调试

如果日志写入失败，建议启用 SelfLog 查看详细错误信息：

```csharp
Serilog.Debugging.SelfLog.Enable(Console.Error);
```

配合 `Serilog.Sinks.Async` 使用时，`Emit` 中的异常默认被静默捕获，必须通过 SelfLog 才能看到。

## 常见问题

### Connection refused

**问题**：连接被拒绝，错误包含 `Connection refused (xxx.log.aliyuncs.com:443)`

**检查**：
1. 确认 Project 名称与阿里云 SLS 控制台完全一致
2. 确认 Endpoint（服务入口）与项目所在区域匹配
3. 用 `nslookup {project}.{endpoint}` 检查 DNS 是否解析到有效 IP
   - 如果返回 `0.0.0.0`，说明项目名或区域错误

### 修改 appsettings.json 后未生效

输出目录存在旧缓存文件，请执行 `dotnet clean && dotnet build` 强制同步。

## 依赖

- [`aliyun-log-dotnetcore-sdk`](https://www.nuget.org/packages/aliyun-log-dotnetcore-sdk) — 阿里云 SLS 官方 SDK
- [`Microsoft.Extensions.Configuration.Abstractions`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Abstractions) — 配置抽象
- [`Microsoft.Extensions.Configuration.Binder`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder) — 配置绑定
- [`Serilog`](https://www.nuget.org/packages/Serilog) — Serilog 核心库

## 项目示例

| 目录 | 场景 | 运行方式 |
|---|---|---|
| [`example/01-DirectOption`](./example/01-DirectOption) | 直接传参 | `dotnet run --project example/01-DirectOption` |
| [`example/02-IConfiguration`](./example/02-IConfiguration) | IConfiguration 绑定（推荐） | `dotnet run --project example/02-IConfiguration` |
| [`example/03-MultiLogstore`](./example/03-MultiLogstore) | 多 Logstore + 环境变量覆盖 | `dotnet run --project example/03-MultiLogstore` |
