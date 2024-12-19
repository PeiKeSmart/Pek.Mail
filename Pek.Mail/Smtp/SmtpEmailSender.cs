using System.Net;
using System.Net.Mail;

using NewLife.Log;

using Pek.Mail.Core;

namespace Pek.Mail.Smtp;

/// <summary>
/// 基于SMTP的电子邮件发送器
/// </summary>
public class SmtpEmailSender : EmailSenderBase, ISmtpEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override String SendEmail(MailMessage mail)
    {
        using var smtpClient = BuildClient();

        try
        {
            smtpClient.Send(mail);
        }
        catch(Exception ex)
        {
            smtpClient.Dispose();
            XTrace.WriteException(ex);
            return "Fail";
        }

        return "OK";
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

        try
        {
            smtpClient.Send(mail);
        }
        catch (Exception ex)
        {
            smtpClient.Dispose();
            XTrace.WriteException(ex);
            return "Fail";
        }

        return "OK";
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override async Task<String> SendEmailAsync(MailMessage mail)
    {
        using var smtpClient = BuildClient();
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