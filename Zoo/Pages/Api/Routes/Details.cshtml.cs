using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Routes;

/// <summary>
/// 路線詳情 API 端點
/// GET /api/routes/{id}
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IRouteService _routeService;
    private readonly ILogger<DetailsModel> _logger;

    /// <summary>
    /// 初始化路線詳情 API
    /// </summary>
    /// <param name="routeService">路線服務</param>
    /// <param name="logger">日誌記錄器</param>
    public DetailsModel(IRouteService routeService, ILogger<DetailsModel> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    /// <summary>
    /// 處理路線詳情請求
    /// </summary>
    /// <param name="id">路線 ID</param>
    /// <param name="includeAnimals">是否包含動物詳細資訊</param>
    /// <param name="includeZones">是否包含區域詳細資訊</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>路線詳情的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(
        string id,
        bool includeAnimals = false,
        bool includeZones = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("路線 ID 不可為空");
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "路線 ID 不可為空"
            });
        }

        _logger.LogDebug("收到路線詳情請求: {Id}", id);

        var route = await _routeService.GetByIdAsync(id, cancellationToken);

        if (route is null)
        {
            _logger.LogWarning("找不到 ID 為 {Id} 的路線", id);
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Not Found",
                status = 404,
                detail = $"找不到 ID 為 '{id}' 的路線"
            });
        }

        // 基本路線資訊
        var response = new Dictionary<string, object>
        {
            ["id"] = route.Id,
            ["nameZh"] = route.NameZh,
            ["nameEn"] = route.NameEn,
            ["type"] = route.Type.ToString(),
            ["description"] = route.Description,
            ["estimatedMinutes"] = route.EstimatedMinutes,
            ["zoneIds"] = route.ZoneIds,
            ["animalIds"] = route.AnimalIds
        };

        // 如果需要包含動物詳細資訊
        if (includeAnimals)
        {
            var animals = await _routeService.GetRouteAnimalsAsync(id, cancellationToken);
            response["animals"] = animals.Select(a => new
            {
                id = a.Id,
                chineseName = a.ChineseName,
                englishName = a.EnglishName,
                thumbnailUrl = a.Media.ThumbnailPath,
                conservationStatus = a.ConservationStatus.ToString(),
                zoneId = a.ZoneId
            }).ToList();
        }

        // 如果需要包含區域詳細資訊
        if (includeZones)
        {
            var zones = await _routeService.GetRouteZonesAsync(id, cancellationToken);
            response["zones"] = zones.Select(z => new
            {
                id = z.Id,
                nameZh = z.NameZh,
                nameEn = z.NameEn,
                color = z.Color,
                position = new { x = z.Position.X, y = z.Position.Y }
            }).ToList();
        }

        _logger.LogInformation("成功回傳路線 {Id} 的詳情", id);

        return new JsonResult(response);
    }
}
