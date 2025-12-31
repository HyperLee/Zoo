using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Zones;

/// <summary>
/// 區域動物清單 API 端點
/// GET /api/zones/{id}/animals
/// </summary>
public class AnimalsModel : PageModel
{
    private readonly IZoneService _zoneService;
    private readonly ILogger<AnimalsModel> _logger;

    /// <summary>
    /// 初始化區域動物清單 API
    /// </summary>
    /// <param name="zoneService">區域服務</param>
    /// <param name="logger">日誌記錄器</param>
    public AnimalsModel(IZoneService zoneService, ILogger<AnimalsModel> logger)
    {
        _zoneService = zoneService;
        _logger = logger;
    }

    /// <summary>
    /// 處理區域動物清單請求
    /// </summary>
    /// <param name="id">區域 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>區域動物清單的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到區域 {ZoneId} 的動物清單請求", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("區域 ID 不可為空");
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "區域 ID 不可為空"
            });
        }

        var zone = await _zoneService.GetByIdAsync(id, cancellationToken);

        if (zone is null)
        {
            _logger.LogWarning("找不到 ID 為 {ZoneId} 的區域", id);
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Not Found",
                status = 404,
                detail = $"找不到 ID 為 '{id}' 的區域"
            });
        }

        var animals = await _zoneService.GetAnimalsByZoneAsync(id, cancellationToken);

        var response = new
        {
            zone = new
            {
                id = zone.Id,
                nameZh = zone.NameZh,
                nameEn = zone.NameEn
            },
            animals = animals.Select(a => new
            {
                id = a.Id,
                chineseName = a.ChineseName,
                englishName = a.EnglishName,
                thumbnailUrl = a.Media.ThumbnailPath,
                conservationStatus = a.ConservationStatus.ToString()
            }).ToList()
        };

        _logger.LogInformation("成功回傳區域 {ZoneId} 的 {Count} 隻動物", id, response.animals.Count);

        return new JsonResult(response);
    }
}
