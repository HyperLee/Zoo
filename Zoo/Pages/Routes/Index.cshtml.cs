using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Routes;

using Route = Zoo.Models.Route;

/// <summary>
/// 路線規劃頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly IRouteService _routeService;
    private readonly IAnimalService _animalService;
    private readonly IZoneService _zoneService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 所有預設路線
    /// </summary>
    public IReadOnlyList<Route> Routes { get; private set; } = [];

    /// <summary>
    /// 所有動物（供自訂路線選擇）
    /// </summary>
    public IReadOnlyList<Animal> Animals { get; private set; } = [];

    /// <summary>
    /// 所有區域（供篩選）
    /// </summary>
    public IReadOnlyList<Zone> Zones { get; private set; } = [];

    /// <summary>
    /// 初始化路線規劃頁面
    /// </summary>
    /// <param name="routeService">路線服務</param>
    /// <param name="animalService">動物服務</param>
    /// <param name="zoneService">區域服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(
        IRouteService routeService,
        IAnimalService animalService,
        IZoneService zoneService,
        ILogger<IndexModel> logger)
    {
        _routeService = routeService;
        _animalService = animalService;
        _zoneService = zoneService;
        _logger = logger;
    }

    /// <summary>
    /// 處理頁面請求
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("載入路線規劃頁面");

        // 平行載入所有資料
        var routesTask = _routeService.GetAllAsync(cancellationToken);
        var animalsTask = _animalService.GetAllAsync(cancellationToken);
        var zonesTask = _zoneService.GetAllAsync(cancellationToken);

        await Task.WhenAll(routesTask, animalsTask, zonesTask);

        Routes = routesTask.Result;
        Animals = animalsTask.Result;
        Zones = zonesTask.Result;

        _logger.LogInformation(
            "路線規劃頁面載入完成: {RouteCount} 條路線, {AnimalCount} 隻動物, {ZoneCount} 個區域",
            Routes.Count,
            Animals.Count,
            Zones.Count);
    }
}
