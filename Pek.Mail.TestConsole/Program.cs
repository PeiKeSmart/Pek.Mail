using System.Net.Mail;

using NewLife.Log;

using Pek.Mail.Attachments;
using Pek.Mail.Core;
using Pek.Mail.MailKit;
using Pek.Mail.Smtp;

namespace Pek.Mail.TestConsole;

internal class Program
{
    private static async Task Main(String[] args)
    {
        XTrace.UseConsole();

        XTrace.WriteLine("========================================");
        XTrace.WriteLine("  Pek.Mail 邮件发送控制台测试工具");
        XTrace.WriteLine("========================================");

        EnsureConfigReady();
        ShowConfig();

        while (true)
        {
            PrintMenu();
            Console.Write("请输入编号: ");
            Console.Out.Flush();
            var key = Console.ReadLine()?.Trim();

            switch (key)
            {
                case "1": await RunAsync("MailKit - 简单字符串发送",    () => Test_MailKit_SimpleAsync()); break;
                case "2": await RunAsync("MailKit - EmailBox 多收件人", () => Test_MailKit_EmailBoxAsync()); break;
                case "3": await RunAsync("MailKit - 内存流附件",        () => Test_MailKit_MemoryAttachmentAsync()); break;
                case "4": await RunAsync("MailKit - 物理文件附件",      () => Test_MailKit_PhysicalAttachmentAsync()); break;
                case "5": await RunAsync("MailKit - 高优先级邮件",      () => Test_MailKit_HighPriorityAsync()); break;
                case "6": await RunAsync("Smtp - 简单字符串发送",       () => Test_Smtp_SimpleAsync()); break;
                case "7": await RunAsync("Smtp - 临时 SMTP 参数发送",   () => Task.FromResult(Test_Smtp_ExplicitParams())); break;
                case "8": await RunAsync("全部测试（依次执行）",         () => RunAllAsync()); break;
                case "0": ShowConfig(); break;
                case "q":
                case "Q":
                    XTrace.WriteLine("已退出。");
                    return;
                default:
                    XTrace.WriteLine("无效输入，请重新选择。");
                    break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 首次运行引导
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 首次运行时配置文件尚不存在，访问 MailSettings.Current 会自动生成默认配置文件。
    /// 检测到默认占位值时提示用户先编辑配置再继续。
    /// </summary>
    private static void EnsureConfigReady()
    {
        var cfg = MailSettings.Current.FindDefault();

        var isPlaceholder = String.IsNullOrEmpty(cfg.Host)
            || cfg.UserName == "your_account@example.com";

        if (!isPlaceholder) return;

        XTrace.WriteLine("检测到配置文件为初始默认值，请先编辑 SMTP 配置后再测试。");
        XTrace.WriteLine("需要修改的关键字段：");
        XTrace.WriteLine("  Host     - SMTP 服务器地址（如 smtp.exmail.qq.com）");
        XTrace.WriteLine("  Port     - 端口（465=SSL，587=StartTLS，25=明文）");
        XTrace.WriteLine("  IsSSL    - 是否 SSL（true/false）");
        XTrace.WriteLine("  UserName - 邮箱账号");
        XTrace.WriteLine("  Password - 密码或授权码");
        XTrace.WriteLine("  From     - 发件人地址");
        XTrace.WriteLine("  FromName - 发件人昵称");

        Console.Write("配置文件已生成，编辑完成后按回车继续，或输入 q 退出: ");
        Console.Out.Flush();
        var input = Console.ReadLine()?.Trim();
        if (input?.ToLower() == "q")
        {
            XTrace.WriteLine("已退出，请编辑配置文件后重新运行。");
            Environment.Exit(0);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 测试方法
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>MailKit：最简字符串发送</summary>
    private static async Task Test_MailKit_SimpleAsync()
    {
        var to = PromptRecipient();
        var sender = BuildMailKitSender();

        await sender.SendAsync(
            to:         to,
            subject:    "[Pek.Mail测试] 简单字符串发送",
            body:       "<h2>发送成功 ✓</h2><p>这是由 <b>Pek.Mail MailKit</b> 发出的简单测试邮件。</p>",
            isBodyHtml: true
        );
        XTrace.WriteLine("发送完成，未抛出异常即为成功。");
    }

    /// <summary>MailKit：EmailBox 多收件人 + 抄送</summary>
    private static async Task Test_MailKit_EmailBoxAsync()
    {
        var to = PromptRecipient();
        var sender = BuildMailKitSender();

        var box = new EmailBox
        {
            Subject    = "[Pek.Mail测试] EmailBox 发送",
            Body       = "<h2>EmailBox 测试 ✓</h2><p>支持多收件人、抄送、密送。</p>",
            IsBodyHtml = true,
            To         = [to],
            Cc         = [],
            Bcc        = []
        };

        var result = await sender.SendAsync(box);
        XTrace.WriteLine("发送结果：{0}", result);
    }

    /// <summary>MailKit：内存流附件</summary>
    private static async Task Test_MailKit_MemoryAttachmentAsync()
    {
        var to = PromptRecipient();
        var sender = BuildMailKitSender();

        var csv    = "姓名,金额,日期\n张三,1000.00,2026-02-26\n李四,2000.00,2026-02-26";
        var bytes  = System.Text.Encoding.UTF8.GetBytes(csv);
        var stream = new MemoryStream(bytes);

        var box = new EmailBox
        {
            Subject    = "[Pek.Mail测试] 内存流附件",
            Body       = "<p>请查收附件中的测试数据（export.csv）。</p>",
            IsBodyHtml = true,
            To         = [to]
        };
        box.Attachments.Add(new MemoryStreamAttachment(stream, "export.csv"));

        var result = await sender.SendAsync(box);
        XTrace.WriteLine("发送结果：{0}", result);
    }

    /// <summary>MailKit：物理文件附件（自动创建临时文件）</summary>
    private static async Task Test_MailKit_PhysicalAttachmentAsync()
    {
        var to  = PromptRecipient();
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, $"Pek.Mail 物理附件测试\r\n生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        try
        {
            var sender = BuildMailKitSender();
            var box = new EmailBox
            {
                Subject    = "[Pek.Mail测试] 物理文件附件",
                Body       = "<p>请查收附件（临时 txt 文件）。</p>",
                IsBodyHtml = true,
                To         = [to]
            };
            box.Attachments.Add(new PhysicalFileAttachment(tmp));

            var result = await sender.SendAsync(box);
            XTrace.WriteLine("发送结果：{0}", result);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    /// <summary>MailKit：高优先级 MailMessage</summary>
    private static async Task Test_MailKit_HighPriorityAsync()
    {
        var to     = PromptRecipient();
        var sender = BuildMailKitSender();

        using var mail = new MailMessage();
        mail.To.Add(to);
        mail.Subject    = "[Pek.Mail测试] 高优先级邮件";
        mail.Body       = "<p>这是一封 <strong>高优先级</strong> 测试邮件。</p>";
        mail.IsBodyHtml = true;
        mail.Priority   = MailPriority.High;

        var result = await sender.SendAsync(mail, normalize: true);
        XTrace.WriteLine("发送结果：{0}", result);
    }

    /// <summary>Smtp：简单字符串发送</summary>
    private static async Task Test_Smtp_SimpleAsync()
    {
        var to     = PromptRecipient();
        var sender = new SmtpEmailSender();

        await sender.SendAsync(
            to:         to,
            subject:    "[Pek.Mail测试] Smtp 简单发送",
            body:       "<h2>Smtp 发送成功 ✓</h2><p>由 <b>SmtpEmailSender</b> 发出。</p>",
            isBodyHtml: true
        );
        XTrace.WriteLine("发送完成，未抛出异常即为成功。");
    }

    /// <summary>Smtp：临时参数覆盖发送（在运行时手动输入 SMTP 参数）</summary>
    private static String Test_Smtp_ExplicitParams()
    {
        var to = PromptRecipient();
        var def = MailSettings.Current.FindDefault();

        Prompt("  临时 SMTP 服务器 [回车使用配置中的值]: ");
        var host = Console.ReadLine()?.Trim();
        if (String.IsNullOrEmpty(host)) host = def.Host!;

        Prompt("  端口 [回车使用配置中的值]: ");
        var portStr = Console.ReadLine()?.Trim();
        var port    = String.IsNullOrEmpty(portStr) ? def.Port : Int32.Parse(portStr);

        Prompt("  用户名 [回车使用配置中的值]: ");
        var user = Console.ReadLine()?.Trim();
        if (String.IsNullOrEmpty(user)) user = def.UserName!;

        Prompt("  密码 [回车使用配置中的值]: ");
        var pass = Console.ReadLine()?.Trim();
        if (String.IsNullOrEmpty(pass)) pass = def.Password!;

        var sender = new SmtpEmailSender();
        using var mail = new MailMessage();
        mail.To.Add(to);
        mail.Subject    = "[Pek.Mail测试] Smtp 临时参数发送";
        mail.Body       = "使用临时 SMTP 参数发送的测试邮件。";
        mail.IsBodyHtml = false;

        var result = sender.Send(mail, host, port, user, pass, true);
        XTrace.WriteLine("发送结果：{0}", result);
        return result;
    }

    /// <summary>依次运行全部测试（共用一次收件人输入）</summary>
    private static async Task RunAllAsync()
    {
        var to = PromptRecipient();
        _cachedRecipient = to;
        try
        {
            await RunAsync("1/5 MailKit 简单发送",   () => Test_MailKit_SimpleAsync());
            await RunAsync("2/5 MailKit EmailBox",   () => Test_MailKit_EmailBoxAsync());
            await RunAsync("3/5 MailKit 内存流附件", () => Test_MailKit_MemoryAttachmentAsync());
            await RunAsync("4/5 MailKit 高优先级",   () => Test_MailKit_HighPriorityAsync());
            await RunAsync("5/5 Smtp 简单发送",      () => Test_Smtp_SimpleAsync());
        }
        finally
        {
            _cachedRecipient = null;
        }
        XTrace.WriteLine("全部测试完成，请检查收件箱。");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 辅助
    // ─────────────────────────────────────────────────────────────────────

    private static String? _cachedRecipient;

    private static MailKitEmailSender BuildMailKitSender() => new(new DefaultMailKitSmtpBuilder());

    private static String PromptRecipient()
    {
        if (_cachedRecipient != null) return _cachedRecipient;

        while (true)
        {
            Console.WriteLine();
            Console.Write("  收件人邮箱地址: ");
            Console.Out.Flush();
            var addr = Console.ReadLine()?.Trim();
            if (!String.IsNullOrEmpty(addr) && addr.Contains('@'))
                return addr;
            XTrace.WriteLine("地址格式不正确，请重新输入。");
        }
    }

    private static async Task RunAsync(String name, Func<Task> action)
    {
        XTrace.WriteLine("开始测试：{0}", name);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }

    private static void ShowConfig()
    {
        var cfg = MailSettings.Current.FindDefault();
        XTrace.WriteLine("当前 SMTP 配置：{0}:{1}  SSL={2}  用户={3}  发件人={4}({5})",
            cfg.Host, cfg.Port, cfg.IsSSL, cfg.UserName, cfg.From, cfg.FromName);
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("1  MailKit - 简单字符串发送");
        Console.WriteLine("2  MailKit - EmailBox（多收件人 + 抄送）");
        Console.WriteLine("3  MailKit - 内存流附件");
        Console.WriteLine("4  MailKit - 物理文件附件（自动创建临时文件）");
        Console.WriteLine("5  MailKit - 高优先级 MailMessage");
        Console.WriteLine("6  Smtp   - 简单字符串发送");
        Console.WriteLine("7  Smtp   - 运行时手动输入 SMTP 参数");
        Console.WriteLine("8  运行全部测试（1~5，共用收件人）");
        Console.WriteLine("0  重新显示当前配置");
        Console.WriteLine("q  退出");
        Console.WriteLine();
    }

    private static void Prompt(String text)
    {
        Console.Write(text);
        Console.Out.Flush();
    }
}
