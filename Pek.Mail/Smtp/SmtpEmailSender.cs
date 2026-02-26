using System.Net;
using System.Net.Mail;
using System.Text;

using NewLife.Log;

using Pek.Mail.Core;

namespace Pek.Mail.Smtp;

/// <summary>
/// 基于SMTP的电子邮件发送器
/// </summary>
public class SmtpEmailSender : EmailSenderBase, ISmtpEmailSender
{
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

                using var smtpClient = SmtpEmailSender.BuildClient(account.Host!, account.Port, account.UserName!, account.Password!, account.IsSSL);
                smtpClient.Send(mail);
                return "OK";
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
        using var smtpClient = SmtpEmailSender.BuildClient(Host, Port, UserName, Password, EnableSsl);
        smtpClient.Send(mail);
        return "OK";
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
            try
            {
                mail.From = new MailAddress(account.From!, account.FromName, Encoding.UTF8);
                mail.ReplyToList.Clear();
                mail.ReplyToList.Add(new MailAddress(account.From!));

                using var smtpClient = SmtpEmailSender.BuildClient(account.Host!, account.Port, account.UserName!, account.Password!, account.IsSSL);
                await smtpClient.SendMailAsync(mail).ConfigureAwait(false);
                return "Smtp方法无返回";
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
        using var smtpClient = SmtpEmailSender.BuildClient(Host, Port, UserName, Password, EnableSsl);
        await smtpClient.SendMailAsync(mail).ConfigureAwait(false);
        return "Smtp方法无返回";
    }

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <returns></returns>
    public SmtpClient BuildClient()
    {
        var config = MailSettings.Current.FindDefault();
        var host = config!.Host;
        var port = config.Port;

        var smtpClient = new SmtpClient(host, port);
        try
        {
            if (config.IsSSL)
            {
                smtpClient.EnableSsl = true;
            }

            if (MailSettings.Current.UseDefaultCredentials)
            {
                smtpClient.UseDefaultCredentials = true;
            }
            else
            {
                smtpClient.UseDefaultCredentials = false;
                var userName = config.UserName;
                if (userName?.IsEmpty() == false)
                {
                    var password = config.Password;
                    var domain = MailSettings.Current.Domain;
                    smtpClient.Credentials = domain?.IsEmpty() == false
                        ? new NetworkCredential(userName, password, domain)
                        : new NetworkCredential(userName, password);
                }
            }

            return smtpClient;
        }
        catch
        {
            smtpClient.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <returns></returns>
    public static SmtpClient BuildClient(String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl)
    {
        var host = Host;
        var port = Port;

        var smtpClient = new SmtpClient(host, port);
        try
        {
            if (EnableSsl)
            {
                smtpClient.EnableSsl = true;
            }

            if (MailSettings.Current.UseDefaultCredentials)
            {
                smtpClient.UseDefaultCredentials = true;
            }
            else
            {
                smtpClient.UseDefaultCredentials = false;
                var userName = UserName;
                if (userName?.IsEmpty() == false)
                {
                    var password = Password;
                    var domain = MailSettings.Current.Domain;
                    smtpClient.Credentials = domain?.IsEmpty() == false
                        ? new NetworkCredential(userName, password, domain)
                        : new NetworkCredential(userName, password);
                }
            }

            return smtpClient;
        }
        catch
        {
            smtpClient.Dispose();
            throw;
        }
    }
}