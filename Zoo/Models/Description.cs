namespace Zoo.Models;

/// <summary>
/// 動物特徵描述
/// </summary>
public class Description
{
    /// <summary>
    /// 體型描述 (如: 體長 2-3 公尺)
    /// </summary>
    public required string Size { get; init; }

    /// <summary>
    /// 外觀特色
    /// </summary>
    public required string Appearance { get; init; }

    /// <summary>
    /// 生活習性
    /// </summary>
    public required string Behavior { get; init; }

    /// <summary>
    /// 完整介紹 (中文)
    /// </summary>
    public required string FullDescriptionZh { get; init; }

    /// <summary>
    /// 完整介紹 (英文)
    /// </summary>
    public required string FullDescriptionEn { get; init; }
}
