using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Routes;

/// <summary>
/// 路線清單 API 端點
/// GET /api/routes
/// </summary>
public class IndexModel : PageModel
{
    private readonly IRouteService _routeService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 初始化路線清單 API
    /// </summary>
    /// <param name="routeService">路線服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(IRouteService routeService, ILogger<IndexModel> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    /// <summary>
    /// 處理路線清單請求
    /// </summary>
    /// <param name="type">路線類型篩選（可選）</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>路線清單的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到路線清單請求，類型篩選: {Type}", type ?? "全部");

        var routes = await _routeService.GetAllAsync(cancellationToken);

        // 如果有指定類型，進行篩選
        if (!string.IsNullOrWhiteSpace(type) && 
            Enum.TryParse<Models.RouteType>(type, ignoreCase: true, out var routeType))
        {
            routes = routes.Where(r => r.Type == routeType).ToList();
        }

        var response = new
        {
            routes = routes.Select(r => new
            {
                id = r.Id,
                nameZh = r.NameZh,
                nameEn = r.NameEn,
                type = r.Type.ToString(),
                description = r.Description,
                estimatedMinutes = r.EstimatedMinutes,
                zoneCount = r.ZoneIds.Count,
                animalCount = r.AnimalIds.Count
            }).ToList(),
            total = routes.Count
        };

        _logger.LogInformation("成功回傳 {Count} 條路線", response.total);

        return new JsonResult(response);
    }
}
