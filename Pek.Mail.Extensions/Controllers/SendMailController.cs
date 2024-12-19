using System.Net.Mail;

using Microsoft.AspNetCore.Mvc;

using NewLife.Log;

using Pek.Infrastructure;
using Pek.MailKit;
using Pek.Models;
using Pek.NCube.BaseControllers;
using Pek.NCubeUI;

using XCode.Membership;

namespace Pek.Mail.Extensions.Controllers;

/// <summary>
/// 邮件发送接口
/// </summary>
[Produces("application/json")]
[Route("api/v1/[controller]")] // 设置路由以包含版本号
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SendMailController(IMailKitEmailSender mailKitEmailSender, IDHFileProvider fileProvider) : ApiControllerBaseX {

    private readonly IMailKitEmailSender _mailKitEmailSender = mailKitEmailSender;
    private readonly IDHFileProvider _fileProvider = fileProvider;

    /// <summary>
    /// 发送测试邮件
    /// </summary>
    /// <returns></returns>
    [DHAuthorize(IsAjax = true)]
    [HttpPost("SendMailTest")]
    public IActionResult SendMailTest([FromForm] String email_host, [FromForm] Boolean email_secure, [FromForm] Int32 email_port, [FromForm] String email_addr, [FromForm] String email_id, [FromForm] String email_pass, [FromForm] String email_test, [FromForm] String fromname, [FromForm] String emailSuffix)
    {
        var user = ManageProvider.User;
        if (user!.Ex1 != 1)
        {
            return Json(new DResult { msg = GetResource("没有权限") });
        }

        try
        {
            SendEmail(email_host, email_port, fromname, email_id, email_pass, email_secure, GetResource("测试邮件"), GetResource("用于系统邮件测试"), email_addr, email_test, EmailSuffix: emailSuffix);
            return Json(new DResult { msg = GetResource("发送成功") });
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return Json(new DResult { msg = GetResource("发送失败") });
        }
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="Subject">主题</param>
    /// <param name="Body">内容</param>
    /// <param name="FromAddress">发件人地址</param>
    /// <param name="FromName">发送邮箱的昵称</param>
    /// <param name="ToAddress">收件人地址</param>
    /// <param name="ToName">收件人名称</param>
    /// <param name="ReplyTo">回复地址</param>
    /// <param name="ReplyToName">回复名称</param>
    /// <param name="Bcc">密件抄送地址列表</param>
    /// <param name="Cc">抄送地址列表</param>
    /// <param name="AttachmentFilePath">附件文件路径</param>
    /// <param name="AttachmentFileName">附件文件名。 如果指定，则此文件名将发送给收件人。 否则，将使用"AttachmentFilePath"名称。</param>
    /// <param name="Headers">标头</param>
    /// <param name="EnableSsl">是否启用SSL,0为否,1为是</param>
    /// <param name="Host">服务器地址</param>
    /// <param name="Password">邮箱密码</param>
    /// <param name="Port">服务器端口</param>
    /// <param name="UserName">邮箱账号</param>
    /// <param name="EmailSuffix">邮箱后缀</param>
    private String SendEmail(String Host, Int32 Port, String FromName, String UserName, String Password, Boolean EnableSsl, String Subject, String Body,
        String FromAddress, String ToAddress, String? ToName = null,
         String? ReplyTo = null, String? ReplyToName = null,
        IEnumerable<String>? Bcc = null, IEnumerable<String>? Cc = null,
        String? AttachmentFilePath = null, String? AttachmentFileName = null,
        IDictionary<String, String>? Headers = null, String? EmailSuffix = null)
    {
        var message = new MailMessage
        {
            //from, to, reply to
            From = new MailAddress(FromAddress, FromName)
        };
        message.To.Add(new MailAddress(ToAddress, ToName));
        if (!String.IsNullOrEmpty(ReplyTo))
        {
            message.ReplyToList.Add(new MailAddress(ReplyTo, ReplyToName));
        }

        //BCC
        if (Bcc != null)
        {
            foreach (var address in Bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
            {
                message.Bcc.Add(address.Trim());
            }
        }

        //CC
        if (Cc != null)
        {
            foreach (var address in Cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
            {
                message.CC.Add(address.Trim());
            }
        }

        //content
        message.Subject = Subject;
        message.Body = Body;
        message.IsBodyHtml = true;

        //headers
        if (Headers != null)
            foreach (var header in Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }

        //create the file attachment for this e-mail message
        if (!String.IsNullOrEmpty(AttachmentFilePath) &&
            _fileProvider.FileExists(AttachmentFilePath))
        {
            var attachment = new Attachment(AttachmentFilePath)!;
            attachment.ContentDisposition!.CreationDate = _fileProvider.GetCreationTime(AttachmentFilePath);
            attachment.ContentDisposition.ModificationDate = _fileProvider.GetLastWriteTime(AttachmentFilePath);
            attachment.ContentDisposition.ReadDate = _fileProvider.GetLastAccessTime(AttachmentFilePath);
            if (!String.IsNullOrEmpty(AttachmentFileName))
            {
                attachment.Name = AttachmentFileName;
            }

            message.Attachments.Add(attachment);
        }

        return _mailKitEmailSender.Send(message);
    }
}
