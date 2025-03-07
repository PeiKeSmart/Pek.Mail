﻿namespace Pek.Mail.Abstractions;

/// <summary>
/// 邮件队列管理器
/// </summary>
public interface IMailQueueManager
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    Boolean IsRunning { get; }

    /// <summary>
    /// 队列数
    /// </summary>
    Int32 Count { get; }

    /// <summary>
    /// 运行
    /// </summary>
    void Run();

    /// <summary>
    /// 停止
    /// </summary>
    void Stop();
}