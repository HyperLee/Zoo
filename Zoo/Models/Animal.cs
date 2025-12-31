namespace Zoo.Models;

/// <summary>
/// 動物實體，包含動物的基本資訊、分類、多媒體資源等
/// </summary>
public class Animal
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 中文名稱
    /// </summary>
    public required string ChineseName { get; init; }

    /// <summary>
    /// 英文名稱
    /// </summary>
    public required string EnglishName { get; init; }

    /// <summary>
    /// 學名 (拉丁文)
    /// </summary>
    public required string ScientificName { get; init; }

    /// <summary>
    /// 所屬區域 ID
    /// </summary>
    public required string ZoneId { get; init; }

    /// <summary>
    /// 分類資訊
    /// </summary>
    public required Classification Classification { get; init; }

    /// <summary>
    /// 特徵描述
    /// </summary>
    public required Description Description { get; init; }

    /// <summary>
    /// 趣味事實 (3-5 項)
    /// </summary>
    public required IReadOnlyList<string> FunFacts { get; init; }

    /// <summary>
    /// IUCN 保育等級
    /// </summary>
    public required ConservationStatus ConservationStatus { get; init; }

    /// <summary>
    /// 多媒體資源
    /// </summary>
    public required MediaResources Media { get; init; }

    /// <summary>
    /// 相關動物 ID 清單
    /// </summary>
    public IReadOnlyList<string> RelatedAnimalIds { get; init; } = [];
}
