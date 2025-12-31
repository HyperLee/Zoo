using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Search;

/// <summary>
/// 搜尋建議 API 端點
/// GET /api/search/suggest?q={keyword}&amp;limit={limit}
/// </summary>
public class SuggestModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SuggestModel> _logger;

    /// <summary>
    /// 初始化搜尋建議 API
    /// </summary>
    /// <param name="searchService">搜尋服務</param>
    /// <param name="logger">日誌記錄器</param>
    public SuggestModel(ISearchService searchService, ILogger<SuggestModel> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// 處理搜尋建議請求
    /// </summary>
    /// <param name="q">搜尋關鍵字</param>
    /// <param name="limit">回傳筆數上限 (預設: 5)</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>搜尋建議清單的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(
        [FromQuery] string? q,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到搜尋建議請求，關鍵字: {Keyword}, 上限: {Limit}", q, limit);

        // 驗證參數
        if (string.IsNullOrWhiteSpace(q))
        {
            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "搜尋關鍵字不得為空",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        // 限制 limit 範圍
        limit = Math.Clamp(limit, 1, 10);

        var suggestions = await _searchService.SuggestAsync(q, limit, cancellationToken);

        _logger.LogInformation("搜尋建議完成，關鍵字: {Keyword}, 結果數: {Count}", q, suggestions.Count);

        return new JsonResult(new
        {
            suggestions = suggestions.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                englishName = s.EnglishName,
                thumbnailUrl = s.ThumbnailUrl
            })
        });
    }
}
