using Pek.Mail.Core;

namespace Pek.Mail.Abstractions;

/// <summary>
/// 邮件队列服务
/// </summary>
public interface IMailQueueService
{
    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="box">邮件</param>
    void Enqueue(EmailBox box);
}