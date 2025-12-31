using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Routes;

/// <summary>
/// 自訂路線規劃 API 端點
/// POST /api/routes/plan
/// </summary>
public class PlanModel : PageModel
{
    private readonly IRouteService _routeService;
    private readonly IAnimalService _animalService;
    private readonly ILogger<PlanModel> _logger;

    /// <summary>
    /// 初始化自訂路線規劃 API
    /// </summary>
    /// <param name="routeService">路線服務</param>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public PlanModel(
        IRouteService routeService,
        IAnimalService animalService,
        ILogger<PlanModel> logger)
    {
        _routeService = routeService;
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理自訂路線規劃請求
    /// </summary>
    /// <param name="request">規劃請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>規劃結果的 JSON 回應</returns>
    public async Task<IActionResult> OnPostAsync(
        [FromBody] PlanRouteRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.AnimalIds is null || request.AnimalIds.Count == 0)
        {
            _logger.LogWarning("自訂路線規劃請求缺少動物 ID");
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "請至少選擇一隻動物"
            });
        }

        _logger.LogDebug("收到自訂路線規劃請求，包含 {Count} 隻動物", request.AnimalIds.Count);

        var result = await _routeService.PlanCustomRouteAsync(
            request.AnimalIds,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("路線規劃失敗: {Error}", result.ErrorMessage);
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = result.ErrorMessage
            });
        }

        // 如果需要包含動物詳細資訊
        object? animalsDetail = null;
        if (request.IncludeAnimals)
        {
            var allAnimals = await _animalService.GetAllAsync(cancellationToken);
            animalsDetail = result.AnimalIds
                .Select(id => allAnimals.FirstOrDefault(a => 
                    a.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                .Where(a => a is not null)
                .Select(a => new
                {
                    id = a!.Id,
                    chineseName = a.ChineseName,
                    englishName = a.EnglishName,
                    thumbnailUrl = a.Media.ThumbnailPath,
                    zoneId = a.ZoneId
                })
                .ToList();
        }

        var response = new Dictionary<string, object?>
        {
            ["success"] = result.Success,
            ["animalIds"] = result.AnimalIds,
            ["zoneIds"] = result.ZoneIds,
            ["estimatedMinutes"] = result.EstimatedMinutes,
            ["shareCode"] = result.ShareCode
        };

        if (animalsDetail is not null)
        {
            response["animals"] = animalsDetail;
        }

        _logger.LogInformation(
            "成功規劃自訂路線: {AnimalCount} 隻動物, {ZoneCount} 個區域",
            result.AnimalIds.Count,
            result.ZoneIds.Count);

        return new JsonResult(response);
    }

    /// <summary>
    /// 處理從分享碼解析路線的請求
    /// GET /api/routes/plan?shareCode=xxx
    /// </summary>
    /// <param name="shareCode">分享碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>解析結果</returns>
    public async Task<IActionResult> OnGetAsync(
        string? shareCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shareCode))
        {
            _logger.LogWarning("分享碼不可為空");
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "請提供分享碼"
            });
        }

        _logger.LogDebug("收到分享碼解析請求: {ShareCode}", shareCode);

        // 解析分享碼
        var animalIds = RouteService.ParseShareCode(shareCode);

        if (animalIds.Count == 0)
        {
            _logger.LogWarning("無效的分享碼: {ShareCode}", shareCode);
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "無效的分享碼"
            });
        }

        // 重新規劃路線（確保動物有效）
        var result = await _routeService.PlanCustomRouteAsync(animalIds, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = result.ErrorMessage
            });
        }

        // 取得動物詳細資訊
        var allAnimals = await _animalService.GetAllAsync(cancellationToken);
        var animalsDetail = result.AnimalIds
            .Select(id => allAnimals.FirstOrDefault(a => 
                a.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            .Where(a => a is not null)
            .Select(a => new
            {
                id = a!.Id,
                chineseName = a.ChineseName,
                englishName = a.EnglishName,
                thumbnailUrl = a.Media.ThumbnailPath,
                zoneId = a.ZoneId
            })
            .ToList();

        var response = new
        {
            success = true,
            animalIds = result.AnimalIds,
            zoneIds = result.ZoneIds,
            estimatedMinutes = result.EstimatedMinutes,
            shareCode = result.ShareCode,
            animals = animalsDetail
        };

        _logger.LogInformation("成功從分享碼解析路線: {Count} 隻動物", result.AnimalIds.Count);

        return new JsonResult(response);
    }
}

/// <summary>
/// 路線規劃請求
/// </summary>
public class PlanRouteRequest
{
    /// <summary>
    /// 選擇的動物 ID 清單
    /// </summary>
    public List<string>? AnimalIds { get; set; }

    /// <summary>
    /// 是否包含動物詳細資訊
    /// </summary>
    public bool IncludeAnimals { get; set; }
}
