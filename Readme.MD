# Pek.Mail - 邮件操作类库

`Pek.Mail` 是基于 DH 框架的 .NET 邮件发送类库，支持 **MailKit**（推荐）和 **System.Net.Mail.SmtpClient** 两种发送引擎，提供简单发信、富文本正文、附件、抄送密送、邮件队列及多账号路由等特性。

- **目标框架**：net8.0 / net9.0 / net10.0
- **NuGet 包**：`Pek.Mail`（核心）、`Pek.Mail.Extensions`（Web 扩展）
- **开源地址**：<https://github.com/PeiKeSmart/Pek.AspNetCore>

---

## 目录

1. [安装](#1-安装)
2. [配置](#2-配置)
3. [服务注册](#3-服务注册)
4. [基本发送](#4-基本发送)
5. [EmailBox 富文本发送](#5-emailbox-富文本发送)
6. [MailMessage 高级发送](#6-mailmessage-高级发送)
7. [附件支持](#7-附件支持)
8. [多账号路由](#8-多账号路由)
9. [邮件队列](#9-邮件队列)
10. [接口说明](#10-接口说明)
11. [常见问题](#11-常见问题)

---

## 1. 安装

```bash
dotnet add package Pek.Mail
```

如需 Web API 扩展（测试发送、验证码等接口）：

```bash
dotnet add package Pek.Mail.Extensions
```

---

## 2. 配置

`Pek.Mail` 使用 NewLife 配置体系 `Config<MailSettings>`，配置文件默认路径为 `Config/Mail.config`（或 `appsettings.json` 均可）。

### 2.1 Config/Mail.config 示例

```xml
<?xml version="1.0" encoding="utf-8"?>
<MailSettings>
  <UseDefaultCredentials>false</UseDefaultCredentials>
  <Domain></Domain>
  <SleepInterval>3000</SleepInterval>
  <SecureSocketOption>Auto</SecureSocketOption>
  <Data>
    <item>
      <Code>唯一标识（自动生成）</Code>
      <Host>smtp.exmail.qq.com</Host>
      <Port>465</Port>
      <IsSSL>true</IsSSL>
      <UserName>noreply@example.com</UserName>
      <Password>your_password</Password>
      <From>noreply@example.com</From>
      <FromName>系统通知</FromName>
      <IsEnabled>true</IsEnabled>
      <IsDefault>true</IsDefault>
      <EmailSuffix></EmailSuffix>
    </item>
  </Data>
</MailSettings>
```

### 2.2 配置字段说明

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `UseDefaultCredentials` | bool | false | 是否使用系统默认凭据（Windows 域账号） |
| `Domain` | string | - | 域名，Windows 域认证时填写 |
| `SleepInterval` | int | 3000 | 队列处理睡眠间隔（毫秒） |
| `SecureSocketOption` | enum | null | MailKit 安全套接字选项（Auto/None/SslOnConnect/StartTls） |
| `Data[].Host` | string | - | SMTP 服务器地址 |
| `Data[].Port` | int | 25 | SMTP 服务器端口 |
| `Data[].IsSSL` | bool | false | 是否启用 SSL/TLS |
| `Data[].UserName` | string | - | 认证用户名 |
| `Data[].Password` | string | - | 认证密码 |
| `Data[].From` | string | - | 发件人邮件地址 |
| `Data[].FromName` | string | - | 发件人昵称 |
| `Data[].IsEnabled` | bool | false | 是否启用该账号 |
| `Data[].IsDefault` | bool | false | 是否为默认账号 |
| `Data[].EmailSuffix` | string | - | 收件人邮箱后缀路由（见[第8节](#8-多账号路由)） |

---

## 3. 服务注册

框架通过 `DHStartup` 自动完成注册，无需手动调用。若需在非 DH 框架项目中使用，可手动注册：

```csharp
// Program.cs
builder.Services.AddScoped<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>();
builder.Services.AddScoped<IMailKitEmailSender, MailKitEmailSender>();

// 若需要 System.Net.Mail.SmtpClient 实现
builder.Services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
```

> **推荐使用 `IMailKitEmailSender`**，基于 MailKit 实现，支持异步和更完善的 SSL/TLS 协议。

---

## 4. 基本发送

### 4.1 同步发送

```csharp
public class NotificationService(IMailKitEmailSender mailSender)
{
    private readonly IMailKitEmailSender _mailSender = mailSender;

    // 最简单的三参数重载（使用配置中的默认发件人）
    public void SendWelcome(String toEmail)
    {
        _mailSender.Send(
            to: toEmail,
            subject: "欢迎注册",
            body: "<h1>欢迎加入！</h1><p>感谢您的注册。</p>",
            isBodyHtml: true
        );
    }

    // 指定发件人
    public void SendWithSender(String toEmail)
    {
        _mailSender.Send(
            from: "noreply@example.com",
            to: toEmail,
            subject: "系统通知",
            body: "您有一条新消息。",
            isBodyHtml: false
        );
    }
}
```

### 4.2 异步发送（推荐）

```csharp
public async Task SendWelcomeAsync(String toEmail)
{
    await _mailSender.SendAsync(
        to: toEmail,
        subject: "欢迎注册",
        body: "<h1>欢迎加入！</h1>",
        isBodyHtml: true
    );
}
```

---

## 5. EmailBox 富文本发送

`EmailBox` 是推荐的邮件对象，支持多收件人、抄送、密送和附件。

### 5.1 多收件人 + 抄送 + 密送

```csharp
public async Task<String> SendOrderConfirmAsync()
{
    var box = new EmailBox
    {
        Subject = "订单确认",
        Body = """
            <html>
            <body>
                <h2>您的订单已确认</h2>
                <p>订单号：<strong>ORD-20260226-001</strong></p>
                <p>感谢您的购买！</p>
            </body>
            </html>
            """,
        IsBodyHtml = true,
        To = ["customer@example.com", "buyer@partner.com"],
        Cc = ["manager@example.com"],
        Bcc = ["log@example.com"]
    };

    return await _mailSender.SendAsync(box);
}
```

### 5.2 带附件发送（详见[第7节](#7-附件支持)）

```csharp
var box = new EmailBox
{
    Subject = "月度报表",
    Body = "<p>请查收本月报表，详见附件。</p>",
    IsBodyHtml = true,
    To = ["boss@example.com"]
};

// 添加物理文件附件
box.Attachments.Add(new PhysicalFileAttachment(@"C:\Reports\report_202602.pdf"));

return await _mailSender.SendAsync(box);
```

---

## 6. MailMessage 高级发送

直接操作 `System.Net.Mail.MailMessage`，适用于需要精细控制（优先级、自定义头等）的场景。

### 6.1 使用默认配置发送

```csharp
using var mail = new MailMessage();
mail.To.Add("recipient@example.com");
mail.Subject = "重要通知";
mail.Body = "<p>这是一封重要通知邮件。</p>";
mail.IsBodyHtml = true;
mail.Priority = MailPriority.High;

// normalize=true 时自动从配置读取发件人地址、设置 UTF-8 编码
var result = _mailSender.Send(mail, normalize: true);
```

### 6.2 临时指定 SMTP 参数发送

```csharp
using var mail = new MailMessage();
mail.To.Add("recipient@example.com");
mail.Subject = "临时账号发送";
mail.Body = "使用临时 SMTP 账号发送。";

var result = _mailSender.Send(
    mail: mail,
    Host: "smtp.163.com",
    Port: 465,
    UserName: "temp@163.com",
    Password: "temp_password",
    EnableSsl: true
);
```

---

## 7. 附件支持

### 7.1 物理文件附件

从磁盘读取文件作为附件。

```csharp
using Pek.Mail.Attachments;

var box = new EmailBox
{
    Subject = "文件传输",
    Body = "<p>请查收附件。</p>",
    To = ["receiver@example.com"]
};

// 文件不存在时构造函数会抛出 FileNotFoundException
box.Attachments.Add(new PhysicalFileAttachment(@"D:\Documents\contract.docx"));
box.Attachments.Add(new PhysicalFileAttachment(@"D:\Documents\appendix.pdf"));

await _mailSender.SendAsync(box);
```

### 7.2 内存流附件

适用于动态生成内容（如 Excel 报表、PDF 等）作为附件。

```csharp
using Pek.Mail.Attachments;

// 模拟动态生成 CSV 内容
var csvContent = "姓名,金额,日期\n张三,1000.00,2026-02-26";
var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
var stream = new MemoryStream(bytes);

var box = new EmailBox
{
    Subject = "数据导出",
    Body = "<p>请查收导出数据。</p>",
    To = ["analyst@example.com"]
};

// MemoryStreamAttachment 会在邮件发送后释放流
box.Attachments.Add(new MemoryStreamAttachment(stream, "data_export.csv"));

await _mailSender.SendAsync(box);
```

---

## 8. 多账号路由

当业务需要多个发件邮箱时（如：国内用企业邮箱，海外用 Gmail），可在配置中添加多条 `Data` 记录，通过 `EmailSuffix` 字段自动路由。

### 8.1 配置多账号

```xml
<Data>
  <!-- 默认账号：发给国内用户 -->
  <item>
    <Code>account-001</Code>
    <Host>smtp.exmail.qq.com</Host>
    <Port>465</Port>
    <IsSSL>true</IsSSL>
    <UserName>noreply@company.cn</UserName>
    <Password>cn_password</Password>
    <From>noreply@company.cn</From>
    <FromName>系统通知</FromName>
    <IsEnabled>true</IsEnabled>
    <IsDefault>true</IsDefault>
    <EmailSuffix></EmailSuffix>
  </item>

  <!-- 专用账号：发给 Gmail 用户 -->
  <item>
    <Code>account-002</Code>
    <Host>smtp.gmail.com</Host>
    <Port>587</Port>
    <IsSSL>false</IsSSL>
    <UserName>noreply@company.com</UserName>
    <Password>gmail_app_password</Password>
    <From>noreply@company.com</From>
    <FromName>System Notification</FromName>
    <IsEnabled>true</IsEnabled>
    <IsDefault>false</IsDefault>
    <!-- 当收件人邮箱包含 @gmail.com 时使用此账号 -->
    <EmailSuffix>@gmail.com</EmailSuffix>
  </item>
</Data>
```

### 8.2 代码获取指定账号配置

```csharp
// 获取默认账号
var defaultConfig = MailSettings.Current.FindDefault();

// 按 Code 获取指定账号
var specificConfig = MailSettings.Current.FindByCode("account-002");
```

---

## 9. 邮件队列

邮件队列用于异步批量发送，将邮件先放入队列，由后台服务统一处理，避免阻塞业务流程。

### 9.1 注册队列服务

```csharp
// Program.cs
builder.Services.AddScoped<IMailQueueProvider, YourMailQueueProvider>(); // 实现自定义队列提供程序
builder.Services.AddScoped<IMailQueueService, MailQueueService>();
```

### 9.2 使用队列发送

```csharp
public class OrderService(IMailQueueService mailQueue)
{
    private readonly IMailQueueService _mailQueue = mailQueue;

    public void PlaceOrder(String customerEmail, String orderId)
    {
        // 业务处理...

        // 将确认邮件放入队列，立即返回，不阻塞订单流程
        _mailQueue.Enqueue(new EmailBox
        {
            Subject = $"订单确认 - {orderId}",
            Body = $"<p>您的订单 <strong>{orderId}</strong> 已提交，我们将尽快处理。</p>",
            IsBodyHtml = true,
            To = [customerEmail]
        });
    }
}
```

---

## 10. 接口说明

### IEmailSender

| 方法 | 说明 |
|------|------|
| `Send(to, subject, body, isBodyHtml)` | 简单同步发送 |
| `SendAsync(to, subject, body, isBodyHtml)` | 简单异步发送 |
| `Send(from, to, subject, body, isBodyHtml)` | 指定发件人同步发送 |
| `SendAsync(from, to, subject, body, isBodyHtml)` | 指定发件人异步发送 |
| `Send(EmailBox)` | 通过 EmailBox 同步发送，返回发送结果 |
| `SendAsync(EmailBox)` | 通过 EmailBox 异步发送，返回发送结果 |
| `Send(MailMessage, normalize)` | 通过 MailMessage 同步发送 |
| `Send(MailMessage, Host, Port, UserName, Password, EnableSsl, normalize)` | 临时 SMTP 参数同步发送 |
| `SendAsync(MailMessage, normalize)` | 通过 MailMessage 异步发送 |

### IMailQueueService

| 方法 | 说明 |
|------|------|
| `Enqueue(EmailBox)` | 将邮件加入发送队列 |

### 附件接口 IAttachment

| 实现类 | 说明 |
|--------|------|
| `PhysicalFileAttachment` | 从磁盘文件路径创建附件 |
| `MemoryStreamAttachment` | 从内存流创建附件 |

---

## 11. 常见问题

### Q1：发送失败返回什么？

- `MailKitEmailSender`：成功返回服务器响应字符串，失败返回异常消息。
- `SmtpEmailSender`：成功返回 `"OK"`，失败返回 `"Fail"`（同步）或 `"Smtp方法无返回"`（异步）。

### Q2：如何使用腾讯企业邮箱？

```xml
<Host>smtp.exmail.qq.com</Host>
<Port>465</Port>
<IsSSL>true</IsSSL>
```

### Q3：如何使用 163 邮箱？

```xml
<Host>smtp.163.com</Host>
<Port>465</Port>
<IsSSL>true</IsSSL>
```

> 注意：163 邮箱需要在邮箱设置中开启 SMTP 服务并使用**授权码**作为密码。

### Q4：如何使用 Gmail？

```xml
<Host>smtp.gmail.com</Host>
<Port>587</Port>
<IsSSL>false</IsSSL>
<SecureSocketOption>StartTls</SecureSocketOption>
```

> 注意：Gmail 需要使用**应用专用密码**，需在 Google 账号安全设置中生成。

### Q5：`normalize=true` 有什么作用？

调用 `Send(MailMessage, normalize: true)` 时，框架会自动：
1. 从 `MailSettings` 配置读取默认发件人邮箱和昵称，填充 `mail.From`
2. 将邮件主题和正文编码设为 UTF-8，避免中文乱码

### Q6：`SecureSocketOption` 应如何设置？

| 值 | 适用场景 |
|----|---------|
| `Auto`（默认） | 自动检测，优先 SSL/TLS，其次 StartTLS |
| `None` | 不加密，端口通常为 25 |
| `SslOnConnect` | 直接 SSL 连接，端口通常为 465 |
| `StartTls` | 先建立明文连接再升级，端口通常为 587 |

---

## 变更历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 4.18 | 2026-02-26 | 当前版本，支持 net8/net9/net10 |