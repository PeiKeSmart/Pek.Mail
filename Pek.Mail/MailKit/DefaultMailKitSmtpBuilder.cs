using MailKit.Net.Smtp;
using MailKit.Security;

namespace Pek.Mail.MailKit;

/// <summary>
/// 默认MailKit SMTP生成器
/// </summary>
public class DefaultMailKitSmtpBuilder : IMailKitSmtpBuilder
{
    /// <summary>
    /// 初始化一个<see cref="DefaultMailKitSmtpBuilder"/>类型的实例
    /// </summary>
    public DefaultMailKitSmtpBuilder()
    {
    }

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <returns></returns>
    public virtual SmtpClient Build()
    {
        var client = new SmtpClient();
        try
        {
            ConfigureClient(client);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 生成SMTP客户端
    /// </summary>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <returns></returns>
    public virtual SmtpClient Build(String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl)
    {
        var client = new SmtpClient();
        try
        {
            ConfigureClient(client, Host, Port, UserName, Password, EnableSsl);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 配置SMTP客户端
    /// </summary>
    /// <param name="client">SMTP客户端</param>
    protected virtual void ConfigureClient(SmtpClient client)
    {
        var emailConfig = MailSettings.Current.FindDefault();
        client.Connect(emailConfig.Host, emailConfig.Port, GetSecureSocketOption());
        if (MailSettings.Current.UseDefaultCredentials)
        {
            return;
        }

        client.Authenticate(emailConfig.UserName, emailConfig.Password);
    }

    /// <summary>
    /// 配置SMTP客户端
    /// </summary>
    /// <param name="client">SMTP客户端</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    protected virtual void ConfigureClient(SmtpClient client, String Host, Int32 Port, String UserName, String Password, Boolean EnableSsl)
    {
        client.Connect(Host, Port, GetSecureSocketOption(EnableSsl));
        if (MailSettings.Current.UseDefaultCredentials)
        {
            return;
        }

        client.Authenticate(UserName, Password);
    }

    /// <summary>
    /// 获取安全套接字选项
    /// </summary>
    /// <returns></returns>
    protected virtual SecureSocketOptions GetSecureSocketOption()
    {
        if (MailSettings.Current.SecureSocketOption.HasValue)
        {
            return MailSettings.Current.SecureSocketOption.Value;
        }

        var emailConfig = MailSettings.Current.FindDefault();
        return emailConfig.IsSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
    }

    /// <summary>
    /// 获取安全套接字选项
    /// </summary>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <returns></returns>
    protected virtual SecureSocketOptions GetSecureSocketOption(Boolean EnableSsl)
    {
        if (MailSettings.Current.SecureSocketOption.HasValue)
        {
            return MailSettings.Current.SecureSocketOption.Value;
        }

        return EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
    }
}
