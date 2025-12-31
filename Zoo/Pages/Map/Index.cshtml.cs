using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Map;

using Route = Zoo.Models.Route;

/// <summary>
/// 互動地圖頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly IZoneService _zoneService;
    private readonly IRouteService _routeService;
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
    /// 所有預設路線
    /// </summary>
    public IReadOnlyList<Route> Routes { get; private set; } = [];

    /// <summary>
    /// 當前選擇的路線（若有）
    /// </summary>
    public Route? SelectedRoute { get; private set; }

    /// <summary>
    /// 選擇的路線包含的動物
    /// </summary>
    public IReadOnlyList<Animal> RouteAnimals { get; private set; } = [];

    /// <summary>
    /// 初始化互動地圖頁面模型
    /// </summary>
    /// <param name="zoneService">區域服務</param>
    /// <param name="routeService">路線服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(
        IZoneService zoneService,
        IRouteService routeService,
        ILogger<IndexModel> logger)
    {
        _zoneService = zoneService;
        _routeService = routeService;
        _logger = logger;
    }

    /// <summary>
    /// 處理頁面 GET 請求
    /// </summary>
    /// <param name="route">選擇的路線 ID（可選）</param>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(string? route = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("載入互動地圖頁面，路線: {Route}", route ?? "無");

        // 平行載入資料
        var zonesTask = _zoneService.GetAllAsync(cancellationToken);
        var zoneCountsTask = _zoneService.GetZoneAnimalCountsAsync(cancellationToken);
        var routesTask = _routeService.GetAllAsync(cancellationToken);

        await Task.WhenAll(zonesTask, zoneCountsTask, routesTask);

        Zones = zonesTask.Result;
        ZoneAnimalCounts = zoneCountsTask.Result;
        Routes = routesTask.Result;

        // 如果有選擇路線，載入路線動物
        if (!string.IsNullOrWhiteSpace(route))
        {
            SelectedRoute = await _routeService.GetByIdAsync(route, cancellationToken);
            if (SelectedRoute is not null)
            {
                RouteAnimals = await _routeService.GetRouteAnimalsAsync(route, cancellationToken);
                _logger.LogInformation("載入路線 {RouteId}，包含 {Count} 隻動物", route, RouteAnimals.Count);
            }
        }

        _logger.LogInformation("成功載入 {ZoneCount} 個區域, {RouteCount} 條路線", Zones.Count, Routes.Count);
    }
}

