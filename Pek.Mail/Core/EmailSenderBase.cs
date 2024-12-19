using System.Net.Mail;
using System.Text;

using Pek.Mail.Abstractions;

namespace Pek.Mail.Core;

/// <summary>
/// 电子邮件发送器基类
/// </summary>
public abstract class EmailSenderBase : IEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    public virtual void Send(String to, String subject, String body, Boolean isBodyHtml = true)
    {
        Send(new MailMessage()
        {
            To = { to },
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        });
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    public virtual async Task SendAsync(String to, String subject, String body, Boolean isBodyHtml = true)
    {
        await SendAsync(new MailMessage()
        {
            To = { to },
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from">发件人</param>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    public virtual void Send(String @from, String to, String subject, String body, Boolean isBodyHtml = true) => Send(new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml });

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from">发件人</param>
    /// <param name="to">收件人</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">正文</param>
    /// <param name="isBodyHtml">是否html内容</param>
    public virtual async Task SendAsync(String @from, String to, String subject, String body, Boolean isBodyHtml = true) => await SendAsync(new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml }).ConfigureAwait(false);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">邮件</param>
    public virtual String Send(EmailBox box)
    {
        var mail = new MailMessage();

        var config = MailSettings.Current.FindDefault();

        mail.From = new MailAddress(config.From!);
        PaserMailAddress(box.To, mail.To);
        PaserMailAddress(box.Cc, mail.CC);
        PaserMailAddress(box.Bcc, mail.Bcc);
        PaserMailAddress(config.From, mail.ReplyToList);
        mail.Subject = box.Subject;
        mail.Body = box.Body;
        mail.IsBodyHtml = box.IsBodyHtml;
        HandlerAttachments(box.Attachments, mail.Attachments);
        return Send(mail);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">邮件</param>
    /// <returns></returns>
    public virtual async Task<String> SendAsync(EmailBox box)
    {
        var mail = new MailMessage();
        var config = MailSettings.Current.FindDefault();
        mail.From = new MailAddress(config.From!, config.FromName);
        PaserMailAddress(box.To, mail.To);
        PaserMailAddress(box.Cc, mail.CC);
        PaserMailAddress(box.Bcc, mail.Bcc);
        PaserMailAddress(config.From, mail.ReplyToList);
        mail.Subject = box.Subject;
        mail.Body = box.Body;
        mail.IsBodyHtml = box.IsBodyHtml;
        HandlerAttachments(box.Attachments, mail.Attachments);
        return await SendAsync(mail).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    public virtual String Send(MailMessage mail, Boolean normalize = true)
    {
        if (normalize)
            NormalizeMail(mail);
        return SendEmail(mail);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    /// <param name="normalize">是否规范化邮件，如果是，则设置发件人地址/名称并使邮件编码为UTF-8</param>
    public virtual async Task<String> SendAsync(MailMessage mail, Boolean normalize = true)
    {
        if (normalize)
            NormalizeMail(mail);
        return await SendEmailAsync(mail).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected abstract String SendEmail(MailMessage mail);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    protected abstract String SendEmail(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected abstract Task<String> SendEmailAsync(MailMessage mail);

    /// <summary>
    /// 处理附件
    /// </summary>
    /// <param name="attachments">附件集合</param>
    /// <param name="attachmentCollection">附件集合对象</param>
    protected virtual void HandlerAttachments(IList<IAttachment> attachments, AttachmentCollection attachmentCollection)
    {
        if (attachments == null || !attachments.Any())
            return;
        foreach (var item in attachments)
        {
            var attachment = new Attachment(item.GetFileStream(), item.GetName());
            attachmentCollection.Add(attachment);
        }
    }

    /// <summary>
    /// 规范化邮件，设置发件人地址/名称并使邮件编码为UTF-8
    /// </summary>
    /// <param name="mail">邮件</param>
    protected virtual void NormalizeMail(MailMessage mail)
    {
        if (mail.From == null || mail.From.Address.IsEmpty())
        {
            var config = MailSettings.Current.FindDefault();
            mail.From = new MailAddress(config.From!, config.FromName, Encoding.UTF8);
        }
        mail.HeadersEncoding ??= Encoding.UTF8;
        mail.SubjectEncoding ??= Encoding.UTF8;
        mail.BodyEncoding ??= Encoding.UTF8;
    }

    /// <summary>
    /// 解析分解邮件地址
    /// </summary>
    /// <param name="mailAddress">邮件地址</param>
    /// <param name="mailAddressCollection">邮件地址对象</param>
    protected static void PaserMailAddress(String? mailAddress, MailAddressCollection mailAddressCollection)
    {
        if (mailAddress?.IsEmpty() == true)
            return;
        Char[] separator = [',', ';'];
        var addressArray = mailAddress?.Split(separator);
        PaserMailAddress([.. addressArray], mailAddressCollection);
    }

    /// <summary>
    /// 解析分解邮件地址
    /// </summary>
    /// <param name="mailAddress">邮件地址列表</param>
    /// <param name="mailAddressCollection">邮件地址对象</param>
    protected static void PaserMailAddress(List<String> mailAddress,
        MailAddressCollection mailAddressCollection)
    {
        if (mailAddress == null || mailAddress.Count == 0)
            return;
        foreach (var address in mailAddress)
        {
            if (address.Trim() == String.Empty)
                continue;
            mailAddressCollection.Add(new MailAddress(address));
        }
    }
}