using MailKit.Net.Smtp;

namespace Pek.MailKit;

/// <summary>
/// MailKit SMTP生成器
/// </summary>
public interface IMailKitSmtpBuilder
{
    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <returns></returns>
    SmtpClient Build();

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <returns></returns>
    SmtpClient Build(String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl);
}
