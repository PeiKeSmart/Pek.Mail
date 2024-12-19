using Pek.Mail.Abstractions;

namespace Pek.Mail.Attachments;

/// <summary>
/// 内存流附件
/// </summary>
/// <remarks>
/// 初始化一个<see cref="MemoryStreamAttachment"/>类型的实例
/// </remarks>
/// <param name="stream">内存流</param>
/// <param name="fileName">文件名</param>
public class MemoryStreamAttachment(MemoryStream stream, String fileName) : IAttachment
{
    /// <summary>
    /// 内存流
    /// </summary>
    private readonly MemoryStream _stream = stream;

    /// <summary>
    /// 文件名
    /// </summary>
    private readonly String _fileName = fileName;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose() => _stream.Dispose();

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <returns></returns>
    public Stream GetFileStream() => _stream;

    /// <summary>
    /// 获取文件名称
    /// </summary>
    /// <returns></returns>
    public String GetName() => _fileName;
}