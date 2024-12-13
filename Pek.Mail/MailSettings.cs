using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.Mail;

/// <summary>邮箱设置</summary>
[DisplayName("邮箱设置")]
[Config("Mail")]
public class MailSettings : Config<MailSettings>
{
    /// <summary>
    /// 邮箱列表
    /// </summary>
    [Description("邮箱列表")]
    public IList<MailData>? MailList{ get; set; }
}

/// <summary>
/// 邮箱数据
/// </summary>
public class MailData
{
    /// <summary>
    /// 邮箱服务器的地址，如 smtp.exmail.qq.com
    /// </summary>
    public String? Host { get; set; }

    /// <summary>
    /// SSL协议
    /// </summary>
    public Boolean IsSSL { get; set; }

    /// <summary>
    /// SSL协议
    /// </summary>
    public Boolean UserName { get; set; }
}