namespace Zoo.Models;

/// <summary>
/// 導覽路線
/// </summary>
public class Route
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 路線名稱 (中文)
    /// </summary>
    public required string NameZh { get; init; }

    /// <summary>
    /// 路線名稱 (英文)
    /// </summary>
    public required string NameEn { get; init; }

    /// <summary>
    /// 路線類型
    /// </summary>
    public required RouteType Type { get; init; }

    /// <summary>
    /// 路線描述
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 預估步行時間 (分鐘)
    /// </summary>
    public required int EstimatedMinutes { get; init; }

    /// <summary>
    /// 途經區域 ID 清單 (依序)
    /// </summary>
    public required IReadOnlyList<string> ZoneIds { get; init; }

    /// <summary>
    /// 途經動物 ID 清單 (依序)
    /// </summary>
    public required IReadOnlyList<string> AnimalIds { get; init; }
}
