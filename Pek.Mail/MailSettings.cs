using System.ComponentModel;

using MailKit.Security;

using NewLife.Configuration;

using Pek.Ids;

namespace Pek.Mail;

/// <summary>邮箱设置</summary>
[DisplayName("邮箱设置")]
[Config("Mail")]
public class MailSettings : Config<MailSettings>
{
    /// <summary>
    /// 是否使用默认证书
    /// </summary>
    [Description("是否使用默认证书")]
    public Boolean UseDefaultCredentials { get; set; }

    /// <summary>
    /// 域名
    /// </summary>
    [Description("域名")]
    public String? Domain { get; set; }

    /// <summary>
    /// 睡眠间隔。默认：3秒
    /// </summary>
    [Description("睡眠间隔。默认：3秒")]
    public Int32 SleepInterval { get; set; } = 3000;

    /// <summary>
    /// 安全套接字选项
    /// </summary>
    [Description("安全套接字选项")]
    public SecureSocketOptions? SecureSocketOption { get; set; }

    /// <summary>
    /// 邮箱数据
    /// </summary>
    [Description("邮箱数据")]
    public IList<MailData> Data { get; set; } = [];

    /// <summary>实例化</summary>
    public MailSettings() { }

    /// <summary>加载时触发</summary>
    protected override void OnLoaded()
    {
        if (Data == null || Data.Count == 0)
        {
            var list = new List<MailData>
            {
                new() {
                    Code = IdHelper.GetIdString(),
                    Host = "smtp.exmail.qq.com",
                    Port = 465,
                    IsSSL = true,
                    UserName = "service@chuangchu.net",
                    Password = "D28GDxU7boyvn7aA",
                    From = "service@chuangchu.net",
                    FromName = "海凌科",
                    IsEnabled = true,
                    IsDefault = true,
                    EmailSuffix = String.Empty
                }
            };

            Data = list;
        }

        base.OnLoaded();
    }

    /// <summary>获取默认的配置数据</summary>
    public MailData FindDefault() => Data.FirstOrDefault(e => e.IsDefault) ?? Data[0];

    /// <summary>根据惟一标识获取数据</summary>
    public MailData? FindByCode(String Code)
    {
        foreach (var item in Data)
        {
            if (item.Code == Code) return item;
        }

        return null;
    }
}

/// <summary>
/// 邮箱数据
/// </summary>
public class MailData
{
    /// <summary>
    /// 惟一标识
    /// </summary>
    [Description("惟一标识")]
    public String? Code { get; set; }

    /// <summary>
    /// 邮箱服务器
    /// </summary>
    [Description("邮箱服务器")]
    public String? Host { get; set; }

    /// <summary>
    /// 邮箱服务器端口
    /// </summary>
    [Description("邮箱服务器端口")]
    public Int32 Port { get; set; } = 25;

    /// <summary>
    /// 是否SSL协议
    /// </summary>
    [Description("是否SSL协议")]
    public Boolean IsSSL { get; set; }

    /// <summary>
    /// 身份验证用户名
    /// </summary>
    [Description("身份验证用户名")]
    public String? UserName { get; set; }

    /// <summary>
    /// 身份验证密码
    /// </summary>
    [Description("身份验证密码")]
    public String? Password { get; set; }

    /// <summary>
    /// 发信人邮件地址
    /// </summary>
    [Description("发信人邮件地址")]
    public String? From { get; set; }

    /// <summary>
    /// 发送邮箱昵称
    /// </summary>
    [Description("发送邮箱昵称")]
    public String? FromName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Description("是否启用")]
    public Boolean IsEnabled { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    [Description("是否默认")]
    public Boolean IsDefault { get; set; }

    /// <summary>
    /// 邮箱后缀，当包含有指定的邮箱后缀时，使用当前邮箱配置,不包含时使用默认邮箱配置
    /// </summary>
    [Description("邮箱后缀")]
    public String? EmailSuffix { get; set; }
}