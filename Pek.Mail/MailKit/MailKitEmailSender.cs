using System.Net.Mail;
using System.Text;

using NewLife.Log;

using Pek.Mail.Core;

using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Pek.Mail.MailKit;

/// <summary>
/// MailKit 电子邮件发送器
/// </summary>
/// <remarks>
/// 初始化一个<see cref="MailKitEmailSender"/>类型的实例
/// </remarks>
/// <param name="smtpBuilder">SMTP 生成器</param>
public class MailKitEmailSender(IMailKitSmtpBuilder smtpBuilder) : EmailSenderBase(), IMailKitEmailSender
{
    /// <summary>
    /// SMTP 生成器
    /// </summary>
    private readonly IMailKitSmtpBuilder _smtpBuilder = smtpBuilder;

    /// <summary>
    /// 发送邮件。依次尝试所有已启用账号，默认账号优先；某账号发送失败时自动切换下一个，全部失败则抛出 <see cref="AggregateException"/>
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override String SendEmail(MailMessage mail)
    {
        var accounts = MailSettings.Current.FindAllEnabled();
        if (accounts.Count == 0)
            throw new InvalidOperationException("没有找到可用的邮箱配置，请检查 Mail.config 中是否存在 IsEnabled=true 的邮箱账号");

        List<Exception>? errors = null;
        foreach (var account in accounts)
        {
            try
            {
                mail.From = new MailAddress(account.From!, account.FromName, Encoding.UTF8);
                mail.ReplyToList.Clear();
                mail.ReplyToList.Add(new MailAddress(account.From!));

                using var client = BuildSmtpClient(account.Host!, account.Port, account.UserName!, account.Password!, account.IsSSL);
                var message = mail.ToMimeMessage();
                var mes = client.Send(message);
                client.Disconnect(true);
                return mes;
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                errors ??= [];
                errors.Add(ex);
            }
        }

        throw new AggregateException($"所有 {accounts.Count} 个启用的邮箱账号均发送失败", errors!);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    protected override String SendEmail(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl)
    {
        using var client = BuildSmtpClient(Host, Port, UserName, Password, EnableSsl);
        var message = mail.ToMimeMessage();
        var mes = client.Send(message);
        client.Disconnect(true);

        return mes;
    }

    /// <summary>
    /// 异步发送邮件。依次尝试所有已启用账号，默认账号优先；某账号发送失败时自动切换下一个，全部失败则抛出 <see cref="AggregateException"/>
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override async Task<String> SendEmailAsync(MailMessage mail)
    {
        var accounts = MailSettings.Current.FindAllEnabled();
        if (accounts.Count == 0)
            throw new InvalidOperationException("没有找到可用的邮箱配置，请检查 Mail.config 中是否存在 IsEnabled=true 的邮箱账号");

        List<Exception>? errors = null;
        foreach (var account in accounts)
        {
            mail.From = new MailAddress(account.From!, account.FromName, Encoding.UTF8);
            mail.ReplyToList.Clear();
            mail.ReplyToList.Add(new MailAddress(account.From!));

            using var client = BuildSmtpClient(account.Host!, account.Port, account.UserName!, account.Password!, account.IsSSL);
            var message = mail.ToMimeMessage();
            try
            {
                var result = await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                errors ??= [];
                errors.Add(ex);

                // 不能用 ConfigureAwait(false) 后继续复用同一个连接，直接断开即可
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        throw new AggregateException($"所有 {accounts.Count} 个启用的邮箱账号均发送失败", errors!);
    }

    /// <summary>
    /// 异步发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="EnableSsl">是否启用SSL</param>
    protected override async Task<String> SendEmailAsync(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl)
    {
        using var client = BuildSmtpClient(Host, Port, UserName, Password, EnableSsl);
        var message = mail.ToMimeMessage();
        try
        {
            var result = await client.SendAsync(message).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await client.DisconnectAsync(true).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <returns></returns>
    protected virtual SmtpClient BuildSmtpClient() => _smtpBuilder.Build();

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <returns></returns>
    protected virtual SmtpClient BuildSmtpClient(String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl) => _smtpBuilder.Build(Host, Port, UserName, Password, EnableSsl);

}