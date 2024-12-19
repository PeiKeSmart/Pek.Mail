using System.Net.Mail;

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

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">邮件</param>
    String Send(EmailBox box);

    ///// <summary>
    ///// 发送邮件
    ///// </summary>
    ///// <param name="box">邮件</param>
    ///// <param name="Host">服务器地址</param>
    ///// <param name="Password">邮箱密码</param>
    ///// <param name="Port">服务器端口</param>
    ///// <param name="UserName">邮箱账号</param>
    ///// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    //String Send(EmailBox box, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">邮件</param>
    /// <returns></returns>
    Task<String> SendAsync(EmailBox box);

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
    Task<String> SendAsync(MailMessage mail, Boolean normalize = true);
}