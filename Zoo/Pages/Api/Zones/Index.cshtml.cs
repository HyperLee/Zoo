using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Zones;

/// <summary>
/// 區域清單 API 端點
/// GET /api/zones
/// </summary>
public class IndexModel : PageModel
{
    private readonly IZoneService _zoneService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 初始化區域清單 API
    /// </summary>
    /// <param name="zoneService">區域服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(IZoneService zoneService, ILogger<IndexModel> logger)
    {
        _zoneService = zoneService;
        _logger = logger;
    }

    /// <summary>
    /// 處理區域清單請求
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>區域清單的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到區域清單請求");

        var zoneCounts = await _zoneService.GetZoneAnimalCountsAsync(cancellationToken);

        var response = new
        {
            zones = zoneCounts.Select(kv => new
            {
                id = kv.Key.Id,
                nameZh = kv.Key.NameZh,
                nameEn = kv.Key.NameEn,
                description = kv.Key.Description,
                position = new
                {
                    x = kv.Key.Position.X,
                    y = kv.Key.Position.Y
                },
                svgPathId = kv.Key.SvgPathId,
                color = kv.Key.Color,
                animalCount = kv.Value
            }).ToList()
        };

        _logger.LogInformation("成功回傳 {Count} 個區域", response.zones.Count);

        return new JsonResult(response);
    }
}
