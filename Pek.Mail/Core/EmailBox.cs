using Pek.Mail.Abstractions;

namespace Pek.Mail.Core;

/// <summary>
/// 电子邮件
/// </summary>
public class EmailBox
{
    /// <summary>
    /// 附件列表
    /// </summary>
    public List<IAttachment> Attachments { get; set; } = [];

    /// <summary>
    /// 正文
    /// </summary>
    public String? Body { get; set; }

    /// <summary>
    /// 抄送人
    /// </summary>
    public List<String> Cc { get; set; } = [];

    /// <summary>
    /// 是否Html内容
    /// </summary>
    public Boolean IsBodyHtml { get; set; } = true;

    /// <summary>
    /// 主题
    /// </summary>
    public String? Subject { get; set; }

    /// <summary>
    /// 收件人
    /// </summary>
    public List<String> To { get; set; } = [];

    /// <summary>
    /// 秘密抄送人
    /// </summary>
    public List<String> Bcc { get; set; } = [];
}