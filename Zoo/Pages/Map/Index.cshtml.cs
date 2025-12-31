using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Map;

/// <summary>
/// 互動地圖頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly IZoneService _zoneService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 園區區域清單
    /// </summary>
    public IReadOnlyList<Zone> Zones { get; private set; } = [];

    /// <summary>
    /// 區域與動物數量對應
    /// </summary>
    public IReadOnlyDictionary<Zone, int> ZoneAnimalCounts { get; private set; } = new Dictionary<Zone, int>();

    /// <summary>
    /// 初始化互動地圖頁面模型
    /// </summary>
    /// <param name="zoneService">區域服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(IZoneService zoneService, ILogger<IndexModel> logger)
    {
        _zoneService = zoneService;
        _logger = logger;
    }

    /// <summary>
    /// 處理頁面 GET 請求
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("載入互動地圖頁面");

        Zones = await _zoneService.GetAllAsync(cancellationToken);
        ZoneAnimalCounts = await _zoneService.GetZoneAnimalCountsAsync(cancellationToken);

        _logger.LogInformation("成功載入 {Count} 個區域", Zones.Count);
    }
}
