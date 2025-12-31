using Zoo.Models;

namespace Zoo.Services;

/// <summary>
/// 搜尋結果項目，包含動物資訊與搜尋相關度
/// </summary>
public class SearchResult
{
    /// <summary>
    /// 動物資料
    /// </summary>
    public required Animal Animal { get; init; }

    /// <summary>
    /// 搜尋相關度分數 (0-100)
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// 符合的搜尋欄位
    /// </summary>
    public IReadOnlyList<string> MatchedFields { get; init; } = [];
}

/// <summary>
/// 搜尋建議項目
/// </summary>
public class SearchSuggestion
{
    /// <summary>
    /// 動物 ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 中文名稱
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 英文名稱
    /// </summary>
    public required string EnglishName { get; init; }

    /// <summary>
    /// 縮圖路徑
    /// </summary>
    public required string ThumbnailUrl { get; init; }
}

/// <summary>
/// 搜尋篩選條件
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// 關鍵字搜尋
    /// </summary>
    public string? Keyword { get; init; }

    /// <summary>
    /// 生物分類篩選
    /// </summary>
    public BiologicalClass? BiologicalClass { get; init; }

    /// <summary>
    /// 棲息地篩選
    /// </summary>
    public Habitat? Habitat { get; init; }

    /// <summary>
    /// 飲食習性篩選
    /// </summary>
    public Diet? Diet { get; init; }

    /// <summary>
    /// 活動時間篩選
    /// </summary>
    public ActivityPattern? ActivityPattern { get; init; }

    /// <summary>
    /// 區域 ID 篩選
    /// </summary>
    public string? ZoneId { get; init; }
}

/// <summary>
/// 搜尋服務介面，提供動物搜尋與篩選功能
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 根據篩選條件搜尋動物
    /// </summary>
    /// <param name="filter">搜尋篩選條件</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>符合條件的搜尋結果</returns>
    /// <example>
    /// <code>
    /// var filter = new SearchFilter { Keyword = "獅子", BiologicalClass = BiologicalClass.Mammal };
    /// var results = await searchService.SearchAsync(filter);
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"{result.Animal.ChineseName} - 分數: {result.Score}");
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<SearchResult>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得搜尋建議（自動完成）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <param name="limit">回傳筆數上限 (預設: 5)</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>搜尋建議清單</returns>
    /// <example>
    /// <code>
    /// var suggestions = await searchService.SuggestAsync("獅", 5);
    /// foreach (var suggestion in suggestions)
    /// {
    ///     Console.WriteLine($"{suggestion.Name} ({suggestion.EnglishName})");
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<SearchSuggestion>> SuggestAsync(string keyword, int limit = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據篩選條件取得動物清單（不含關鍵字搜尋）
    /// </summary>
    /// <param name="filter">篩選條件</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>符合條件的動物清單</returns>
    /// <example>
    /// <code>
    /// var filter = new SearchFilter { Habitat = Habitat.Grassland };
    /// var animals = await searchService.FilterAsync(filter);
    /// </code>
    /// </example>
    Task<IReadOnlyList<Animal>> FilterAsync(SearchFilter filter, CancellationToken cancellationToken = default);
}
