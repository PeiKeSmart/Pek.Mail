using System.Net.Mail;

using Pek.Mail;
using Pek.Mail.Core;

namespace Pek.Mail.Abstractions;

/// <summary>
/// 电子邮件发送器
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    void Send(String to, String subject, String body, Boolean isBodyHtml = true);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    Task SendAsync(String to, String subject, String body, Boolean isBodyHtml = true);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from">发件人</param>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    void Send(String from, String to, String subject, String body, Boolean isBodyHtml = true);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from">发件人</param>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    Task SendAsync(String from, String to, String subject, String body, Boolean isBodyHtml = true);

    /// <summary>发送邮件。使用配置文件中所有已启用账号依次尝试</summary>
    /// <param name="box">邮件</param>
    String Send(EmailBox box);

    /// <summary>发送邮件。使用配置文件中所有已启用账号依次尝试</summary>
    /// <param name="box">邮件</param>
    /// <returns></returns>
    Task<String> SendAsync(EmailBox box);

    /// <summary>发送邮件。按 <paramref name="accounts"/> 集合依次尝试，成功即止；全部失败则抛出 <see cref="AggregateException"/></summary>
    /// <param name="box">邮件</param>
    /// <param name="accounts">指定要尝试的 <see cref="MailData"/> 账号集合，集合为空时退回使用所有已启用账号</param>
    String Send(EmailBox box, IList<MailData> accounts);

    /// <summary>异步发送邮件。按 <paramref name="accounts"/> 集合依次尝试，成功即止；全部失败则抛出 <see cref="AggregateException"/></summary>
    /// <param name="box">邮件</param>
    /// <param name="accounts">指定要尝试的 <see cref="MailData"/> 账号集合，集合为空时退回使用所有已启用账号</param>
    Task<String> SendAsync(EmailBox box, IList<MailData> accounts);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    String Send(MailMessage mail, Boolean normalize = true);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <param name="fallback">指定账号发送失败时，是否自动兜底使用配置文件中的已启用账号轮换发送</param>
    String Send(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl, Boolean normalize = true, Boolean fallback = false);

    /// <summary>
    /// 异步发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    /// <param name="fallback">指定账号发送失败时，是否自动兜底使用配置文件中的已启用账号轮换发送</param>
    Task<String> SendAsync(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl, Boolean normalize = true, Boolean fallback = false);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    Task<String> SendAsync(MailMessage mail, Boolean normalize = true);
}