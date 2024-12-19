using NewLife.Log;

using Pek.Mail.Abstractions;
using Pek.Mail.Core;

namespace Pek.Mail.Smtp;

/// <summary>
/// 基于SMTP的邮件队列管理器
/// </summary>
/// <remarks>
/// 初始化一个<see cref="SmtpMailQueueManager"/>类型的实例
/// </remarks>
/// <param name="mailQueueProvider">邮件队列提供程序</param>
/// <param name="smtpEmailSender">SMTP电子邮件发送器</param>
public class SmtpMailQueueManager(IMailQueueProvider mailQueueProvider, ISmtpEmailSender smtpEmailSender) : MailQueueManagerBase(mailQueueProvider), IMailQueueManager
{
    /// <summary>
    /// SMTP电子邮件发送器
    /// </summary>
    private readonly ISmtpEmailSender _smtpEmailSender = smtpEmailSender;

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">电子邮件</param>
    protected override void SendMail(EmailBox box) => _smtpEmailSender.Send(box);

    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="log">日志</param>
    /// <param name="level">日志等级</param>
    protected override void WriteLog(String log, LogLevel level) => Console.WriteLine(log);
}