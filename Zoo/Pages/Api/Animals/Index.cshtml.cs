using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Api.Animals;

/// <summary>
/// 動物清單 API 端點
/// GET /api/animals
/// </summary>
public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 初始化動物清單 API
    /// </summary>
    /// <param name="searchService">搜尋服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(ISearchService searchService, ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// 處理動物清單請求
    /// </summary>
    /// <param name="class">生物分類篩選</param>
    /// <param name="habitat">棲息地篩選</param>
    /// <param name="diet">飲食習性篩選</param>
    /// <param name="activity">活動時間篩選</param>
    /// <param name="zone">區域 ID 篩選</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>動物清單的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(
        [FromQuery(Name = "class")] string? biologicalClass,
        [FromQuery] string? habitat,
        [FromQuery] string? diet,
        [FromQuery] string? activity,
        [FromQuery] string? zone,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到動物清單請求，篩選條件: class={Class}, habitat={Habitat}, diet={Diet}, activity={Activity}, zone={Zone}",
            biologicalClass, habitat, diet, activity, zone);

        var filter = new SearchFilter
        {
            BiologicalClass = ParseEnum<BiologicalClass>(biologicalClass),
            Habitat = ParseEnum<Habitat>(habitat),
            Diet = ParseEnum<Diet>(diet),
            ActivityPattern = ParseEnum<ActivityPattern>(activity),
            ZoneId = zone
        };

        var animals = await _searchService.FilterAsync(filter, cancellationToken);

        _logger.LogInformation("動物清單請求完成，結果數: {Count}", animals.Count);

        return new JsonResult(new
        {
            animals = animals.Select(a => new
            {
                id = a.Id,
                chineseName = a.ChineseName,
                englishName = a.EnglishName,
                thumbnailUrl = a.Media.ThumbnailPath,
                conservationStatus = a.ConservationStatus.ToString(),
                classification = new
                {
                    biologicalClass = a.Classification.BiologicalClass.ToString(),
                    habitat = a.Classification.Habitat.ToString()
                }
            }),
            total = animals.Count
        });
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
