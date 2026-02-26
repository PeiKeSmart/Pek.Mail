using System.Net.Mail;

namespace Pek.Mail.Core;

/// <summary>
/// 空电子邮件发送器
/// </summary>
public class NullEmailSender : EmailSenderBase
{
    /// <summary>
    /// 初始化一个<see cref="NullEmailSender"/>类型的实例
    /// </summary>
    public NullEmailSender() : base()
    {
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    protected override String SendEmail(MailMessage mail) => "Fail";

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    protected override String SendEmail(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl) => "Fail";

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <returns></returns>
    protected override Task<String> SendEmailAsync(MailMessage mail) => Task.FromResult("空电子邮件发送器");

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="EnableSsl">是否启用SSL</param>
    /// <returns></returns>
    protected override Task<String> SendEmailAsync(MailMessage mail, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl) => Task.FromResult("空电子邮件发送器");
}