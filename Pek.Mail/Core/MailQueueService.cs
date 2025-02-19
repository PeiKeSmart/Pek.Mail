﻿using Pek.Mail.Abstractions;

namespace Pek.Mail.Core;

/// <summary>
/// 邮件队列服务
/// </summary>
/// <remarks>
/// 初始化一个<see cref="MailQueueService"/>类型的实例
/// </remarks>
/// <param name="provider">邮件队列提供程序</param>
public class MailQueueService(IMailQueueProvider provider) : IMailQueueService
{
    /// <summary>
    /// 邮件队列提供程序
    /// </summary>
    private readonly IMailQueueProvider _provider = provider;

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="box">电子邮件</param>
    public void Enqueue(EmailBox box) => _provider.Enqueue(box);
}