using System.Text;
using Zoo.Models;

namespace Zoo.Services;

using Route = Zoo.Models.Route;

/// <summary>
/// 路線服務實作，提供導覽路線資料的存取與規劃功能
/// </summary>
public class RouteService : IRouteService
{
    private readonly IJsonDataService _jsonDataService;
    private readonly IAnimalService _animalService;
    private readonly IZoneService _zoneService;
    private readonly ILogger<RouteService> _logger;

    // 每個區域間的預估步行時間（分鐘）
    private const int MinutesPerZone = 15;
    // 每個動物的預估觀賞時間（分鐘）
    private const int MinutesPerAnimal = 5;

    /// <summary>
    /// 初始化路線服務
    /// </summary>
    /// <param name="jsonDataService">JSON 資料服務</param>
    /// <param name="animalService">動物服務</param>
    /// <param name="zoneService">區域服務</param>
    /// <param name="logger">日誌記錄器</param>
    public RouteService(
        IJsonDataService jsonDataService,
        IAnimalService animalService,
        IZoneService zoneService,
        ILogger<RouteService> logger)
    {
        _jsonDataService = jsonDataService;
        _animalService = animalService;
        _zoneService = zoneService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("取得所有導覽路線資料");

        var routes = await _jsonDataService.LoadAsync<Route>(
            "routes.json",
            "routes",
            cancellationToken);

        _logger.LogInformation("成功取得 {Count} 條導覽路線", routes.Count);

        return routes;
    }

    /// <inheritdoc />
    public async Task<Route?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("路線 ID 不可為空");
            return null;
        }

        _logger.LogDebug("根據 ID 取得路線: {Id}", id);

        var routes = await GetAllAsync(cancellationToken);
        var route = routes.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (route is null)
        {
            _logger.LogWarning("找不到 ID 為 {Id} 的路線", id);
        }
        else
        {
            _logger.LogDebug("成功取得路線: {NameZh} ({Id})", route.NameZh, route.Id);
        }

        return route;
    }

    /// <inheritdoc />
    public async Task<CustomRouteResult> PlanCustomRouteAsync(
        IEnumerable<string> animalIds,
        CancellationToken cancellationToken = default)
    {
        var animalIdList = animalIds?.ToList() ?? [];

        if (animalIdList.Count == 0)
        {
            _logger.LogWarning("規劃自訂路線時未提供任何動物 ID");
            return new CustomRouteResult
            {
                Success = false,
                ErrorMessage = "請至少選擇一隻動物",
                AnimalIds = [],
                ZoneIds = [],
                EstimatedMinutes = 0
            };
        }

        _logger.LogDebug("開始規劃自訂路線，選擇了 {Count} 隻動物", animalIdList.Count);

        // 取得所有動物資料
        var allAnimals = await _animalService.GetAllAsync(cancellationToken);
        var allZones = await _zoneService.GetAllAsync(cancellationToken);

        // 驗證並取得選擇的動物
        var selectedAnimals = new List<Animal>();
        var invalidIds = new List<string>();

        foreach (var animalId in animalIdList)
        {
            var animal = allAnimals.FirstOrDefault(a => 
                a.Id.Equals(animalId, StringComparison.OrdinalIgnoreCase));
            
            if (animal is not null)
            {
                selectedAnimals.Add(animal);
            }
            else
            {
                invalidIds.Add(animalId);
            }
        }

        if (invalidIds.Count > 0)
        {
            _logger.LogWarning("以下動物 ID 不存在: {InvalidIds}", string.Join(", ", invalidIds));
        }

        if (selectedAnimals.Count == 0)
        {
            return new CustomRouteResult
            {
                Success = false,
                ErrorMessage = "所有選擇的動物 ID 均無效",
                AnimalIds = [],
                ZoneIds = [],
                EstimatedMinutes = 0
            };
        }

        // 規劃最佳路徑：依區域位置排序
        var orderedAnimals = PlanOptimalRoute(selectedAnimals, allZones);

        // 取得途經的區域（依序，不重複）
        var orderedZoneIds = orderedAnimals
            .Select(a => a.ZoneId)
            .Distinct()
            .ToList();

        // 計算預估時間
        var estimatedMinutes = CalculateEstimatedTime(orderedAnimals.Count, orderedZoneIds.Count);

        // 產生分享碼
        var shareCode = GenerateShareCode(orderedAnimals.Select(a => a.Id));

        var result = new CustomRouteResult
        {
            Success = true,
            AnimalIds = orderedAnimals.Select(a => a.Id).ToList(),
            ZoneIds = orderedZoneIds,
            EstimatedMinutes = estimatedMinutes,
            ShareCode = shareCode
        };

        _logger.LogInformation(
            "成功規劃自訂路線: {AnimalCount} 隻動物, {ZoneCount} 個區域, 約 {Minutes} 分鐘",
            result.AnimalIds.Count,
            result.ZoneIds.Count,
            result.EstimatedMinutes);

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> GetRouteAnimalsAsync(
        string routeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(routeId))
        {
            _logger.LogWarning("路線 ID 不可為空");
            return [];
        }

        var route = await GetByIdAsync(routeId, cancellationToken);
        if (route is null)
        {
            return [];
        }

        var allAnimals = await _animalService.GetAllAsync(cancellationToken);
        
        // 依路線中的動物順序返回
        var routeAnimals = route.AnimalIds
            .Select(id => allAnimals.FirstOrDefault(a => 
                a.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            .Where(a => a is not null)
            .Cast<Animal>()
            .ToList();

        _logger.LogDebug("路線 {RouteId} 包含 {Count} 隻動物", routeId, routeAnimals.Count);

        return routeAnimals;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Zone>> GetRouteZonesAsync(
        string routeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(routeId))
        {
            _logger.LogWarning("路線 ID 不可為空");
            return [];
        }

        var route = await GetByIdAsync(routeId, cancellationToken);
        if (route is null)
        {
            return [];
        }

        var allZones = await _zoneService.GetAllAsync(cancellationToken);

        // 依路線中的區域順序返回
        var routeZones = route.ZoneIds
            .Select(id => allZones.FirstOrDefault(z => 
                z.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            .Where(z => z is not null)
            .Cast<Zone>()
            .ToList();

        _logger.LogDebug("路線 {RouteId} 途經 {Count} 個區域", routeId, routeZones.Count);

        return routeZones;
    }

    /// <summary>
    /// 規劃最佳路徑：使用簡單的貪婪演算法
    /// 以區域座標作為基準，將動物依區域位置排序
    /// </summary>
    private static List<Animal> PlanOptimalRoute(List<Animal> animals, IReadOnlyList<Zone> zones)
    {
        if (animals.Count <= 1)
        {
            return animals;
        }

        // 建立區域位置對照表
        var zonePositions = zones.ToDictionary(
            z => z.Id,
            z => z.Position,
            StringComparer.OrdinalIgnoreCase);

        // 依區域的 X 座標 + Y 座標排序（從左上到右下的簡化路徑）
        var orderedAnimals = animals
            .OrderBy(a => 
            {
                if (zonePositions.TryGetValue(a.ZoneId, out var pos))
                {
                    return pos.X + pos.Y;
                }
                return double.MaxValue;
            })
            .ThenBy(a => a.ChineseName) // 同區域內依名稱排序
            .ToList();

        return orderedAnimals;
    }

    /// <summary>
    /// 計算預估步行時間
    /// </summary>
    private static int CalculateEstimatedTime(int animalCount, int zoneCount)
    {
        // 基本時間 = 區域間移動時間 + 動物觀賞時間
        var zoneTime = Math.Max(zoneCount - 1, 0) * MinutesPerZone;
        var animalTime = animalCount * MinutesPerAnimal;

        return zoneTime + animalTime;
    }

    /// <summary>
    /// 產生路線分享碼
    /// </summary>
    private static string GenerateShareCode(IEnumerable<string> animalIds)
    {
        var idsString = string.Join(",", animalIds);
        var bytes = Encoding.UTF8.GetBytes(idsString);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// 從分享碼解析動物 ID 清單
    /// </summary>
    /// <param name="shareCode">分享碼</param>
    /// <returns>動物 ID 清單，若解析失敗則回傳空集合</returns>
    public static IReadOnlyList<string> ParseShareCode(string shareCode)
    {
        if (string.IsNullOrWhiteSpace(shareCode))
        {
            return [];
        }

        try
        {
            // 還原 Base64 字串
            var base64 = shareCode
                .Replace("-", "+")
                .Replace("_", "/");

            // 補齊 padding
            var padding = base64.Length % 4;
            if (padding > 0)
            {
                base64 += new string('=', 4 - padding);
            }

            var bytes = Convert.FromBase64String(base64);
            var idsString = Encoding.UTF8.GetString(bytes);

            return idsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
