namespace Zoo.Models;

/// <summary>
/// 動物分類資訊
/// </summary>
public class Classification
{
    /// <summary>
    /// 生物分類 (哺乳類、鳥類、爬蟲類、兩棲類、魚類、無脊椎動物)
    /// </summary>
    public required BiologicalClass BiologicalClass { get; init; }

    /// <summary>
    /// 棲息地 (熱帶雨林、沙漠、草原、極地、海洋、淡水、山區)
    /// </summary>
    public required Habitat Habitat { get; init; }

    /// <summary>
    /// 飲食習性 (肉食性、草食性、雜食性)
    /// </summary>
    public required Diet Diet { get; init; }

    /// <summary>
    /// 活動時間 (日行性、夜行性、晨昏性)
    /// </summary>
    public required ActivityPattern ActivityPattern { get; init; }
}
