using System.Diagnostics;

using NewLife.Log;

using Pek.Mail.Abstractions;

namespace Pek.Mail.Core;

/// <summary>
/// 邮件队列管理器基类
/// </summary>
/// <remarks>
/// 初始化一个<see cref="MailQueueManagerBase"/>类型的实例
/// </remarks>
/// <param name="mailQueueProvider">邮件队列提供程序</param>
public abstract class MailQueueManagerBase(IMailQueueProvider mailQueueProvider) : IMailQueueManager
{
    /// <summary>
    /// 邮件队列提供程序
    /// </summary>
    private readonly IMailQueueProvider _mailQueueProvider = mailQueueProvider;

    /// <summary>
    /// 尝试停止运行
    /// </summary>
    private Boolean _tryStop;

    /// <summary>
    /// 线程
    /// </summary>
    private Thread? _thread;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public Boolean IsRunning { get; protected set; } = false;

    /// <summary>
    /// 队列数
    /// </summary>
    public Int32 Count => _mailQueueProvider.Count;

    /// <summary>
    /// 运行
    /// </summary>
    public virtual void Run()
    {
        if (IsRunning || (_thread != null && _thread.IsAlive))
        {
            WriteLog("已经运行，又被启动了，新线程启动已经取消", LogLevel.Warn);
            return;
        }
        IsRunning = true;
        _thread = new Thread(StartSendMail)
        {
            Name = "EmailQueue",
            IsBackground = true
        };
        WriteLog("线程即将启动", LogLevel.Info);
        _thread.Start();
        WriteLog($"线程已经启动，线程ID是: {_thread.ManagedThreadId}", LogLevel.Info);
    }

    /// <summary>
    /// 停止
    /// </summary>
    public virtual void Stop()
    {
        if (_tryStop)
        {
            return;
        }

        _tryStop = true;
    }

    /// <summary>
    /// 开始发送邮件
    /// </summary>
    protected void StartSendMail()
    {
        var sw = new Stopwatch();
        try
        {
            while (true)
            {
                if (_tryStop)
                {
                    break;
                }

                if (_mailQueueProvider.IsEmpty)
                {
                    WriteLog("队列是空，开始睡眠", LogLevel.All);
                    Thread.Sleep(MailSettings.Current.SleepInterval);
                    continue;
                }

                if (_mailQueueProvider.TryDequeue(out var box))
                {
                    WriteLog($"开始发送邮件 标题：{box.Subject}，收件人：{box.To.First()}", LogLevel.Info);
                    sw.Restart();
                    SendMail(box);
                    sw.Stop();
                    WriteLog($"发送邮件结束 标题：{box.Subject}，收件人：{box.To.First()}，耗时：{sw.Elapsed.TotalSeconds}",
                        LogLevel.Info);
                }
            }
        }
        catch (Exception e)
        {
            WriteLog($"循环中出错，线程即将结束：{e.Message}", LogLevel.Error);
            IsRunning = false;
        }

        WriteLog("邮件发送线程即将停止，人为跳出循环，没有异常发生", LogLevel.Info);
        _tryStop = false;
        IsRunning = false;
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="box">电子邮件</param>
    protected abstract void SendMail(EmailBox box);

    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="log">日志</param>
    /// <param name="level">日志等级</param>
    protected abstract void WriteLog(String log, LogLevel level);
}