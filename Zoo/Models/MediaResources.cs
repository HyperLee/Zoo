namespace Zoo.Models;

/// <summary>
/// 動物多媒體資源
/// </summary>
public class MediaResources
{
    /// <summary>
    /// 圖片路徑清單 (至少 3 張動物森友會風格插畫)
    /// </summary>
    public required IReadOnlyList<string> Images { get; init; }

    /// <summary>
    /// 叫聲音效路徑 (可選)
    /// </summary>
    public string? SoundPath { get; init; }

    /// <summary>
    /// 縮圖路徑
    /// </summary>
    public required string ThumbnailPath { get; init; }
}
