namespace Zoo.Models;

/// <summary>
/// 園區區域
/// </summary>
public class Zone
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 區域名稱 (中文)
    /// </summary>
    public required string NameZh { get; init; }

    /// <summary>
    /// 區域名稱 (英文)
    /// </summary>
    public required string NameEn { get; init; }

    /// <summary>
    /// 區域描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 地圖上的座標 (SVG 座標系統)
    /// </summary>
    public required Coordinate Position { get; init; }

    /// <summary>
    /// SVG 路徑 ID (用於地圖點擊偵測)
    /// </summary>
    public required string SvgPathId { get; init; }

    /// <summary>
    /// 區域顏色 (用於地圖顯示)
    /// </summary>
    public required string Color { get; init; }
}
