namespace Zoo.Models;

/// <summary>
/// 園區設施
/// </summary>
public class Facility
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 設施名稱 (中文)
    /// </summary>
    public required string NameZh { get; init; }

    /// <summary>
    /// 設施名稱 (英文)
    /// </summary>
    public required string NameEn { get; init; }

    /// <summary>
    /// 設施類型
    /// </summary>
    public required FacilityType Type { get; init; }

    /// <summary>
    /// 所屬區域 ID
    /// </summary>
    public required string ZoneId { get; init; }

    /// <summary>
    /// 地圖上的座標
    /// </summary>
    public required Coordinate Position { get; init; }

    /// <summary>
    /// 圖示名稱
    /// </summary>
    public required string IconName { get; init; }
}
