using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Search;

/// <summary>
/// 搜尋結果頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Keyword { get; set; }

    /// <summary>
    /// 生物分類篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "class")]
    public string? BiologicalClass { get; set; }

    /// <summary>
    /// 棲息地篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "habitat")]
    public string? HabitatFilter { get; set; }

    /// <summary>
    /// 飲食習性篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "diet")]
    public string? DietFilter { get; set; }

    /// <summary>
    /// 活動時間篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "activity")]
    public string? ActivityFilter { get; set; }

    /// <summary>
    /// 搜尋結果
    /// </summary>
    public IReadOnlyList<SearchResult> Results { get; private set; } = [];

    /// <summary>
    /// 初始化搜尋結果頁面模型
    /// </summary>
    /// <param name="searchService">搜尋服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(ISearchService searchService, ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求，執行搜尋
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("載入搜尋結果頁面，關鍵字: {Keyword}", Keyword);

        var filter = new SearchFilter
        {
            Keyword = Keyword,
            BiologicalClass = ParseEnum<BiologicalClass>(BiologicalClass),
            Habitat = ParseEnum<Habitat>(HabitatFilter),
            Diet = ParseEnum<Diet>(DietFilter),
            ActivityPattern = ParseEnum<ActivityPattern>(ActivityFilter)
        };

        Results = await _searchService.SearchAsync(filter, cancellationToken);

        _logger.LogInformation("搜尋完成，找到 {Count} 筆結果", Results.Count);
    }

    /// <summary>
    /// 解析列舉值
    /// </summary>
    /// <typeparam name="T">列舉型別</typeparam>
    /// <param name="value">字串值</param>
    /// <returns>解析後的列舉值，若無效則回傳 null</returns>
    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        return null;
    }
}
