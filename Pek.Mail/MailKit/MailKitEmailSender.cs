using System.Net.Mail;

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
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override String SendEmail(MailMessage mail)
    {
        using var client = BuildSmtpClient();
        var message = mail.ToMimeMessage();
        var mes = client.Send(message);
        client.Disconnect(true);

        return mes;
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
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override async Task<String> SendEmailAsync(MailMessage mail)
    {
        using var client = BuildSmtpClient();
        var message = mail.ToMimeMessage();
        var result = String.Empty;

        try
        {
            result = await client.SendAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            result = ex.Message;
        }
        finally
        {
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }

        return result;
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