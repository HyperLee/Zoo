using Zoo.Models;

namespace Zoo.Services;

using Route = Zoo.Models.Route;

/// <summary>
/// 路線服務介面，提供導覽路線資料的存取與規劃功能
/// </summary>
public interface IRouteService
{
    /// <summary>
    /// 取得所有預設導覽路線
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>所有路線的集合</returns>
    /// <example>
    /// <code>
    /// var routes = await routeService.GetAllAsync();
    /// foreach (var route in routes)
    /// {
    ///     Console.WriteLine($"{route.NameZh} - 約 {route.EstimatedMinutes} 分鐘");
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Route>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 取得單一路線
    /// </summary>
    /// <param name="id">路線的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>找到的路線，若不存在則回傳 null</returns>
    /// <example>
    /// <code>
    /// var route = await routeService.GetByIdAsync("route-highlights");
    /// if (route is not null)
    /// {
    ///     Console.WriteLine($"找到路線: {route.NameZh}");
    /// }
    /// </code>
    /// </example>
    Task<Zoo.Models.Route?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據選擇的動物規劃自訂路線
    /// </summary>
    /// <param name="animalIds">使用者選擇的動物 ID 清單</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>規劃好的自訂路線結果</returns>
    /// <example>
    /// <code>
    /// var animalIds = new[] { "lion-001", "penguin-001", "elephant-001" };
    /// var customRoute = await routeService.PlanCustomRouteAsync(animalIds);
    /// Console.WriteLine($"規劃的路線包含 {customRoute.AnimalIds.Count} 隻動物");
    /// </code>
    /// </example>
    Task<CustomRouteResult> PlanCustomRouteAsync(
        IEnumerable<string> animalIds, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得路線包含的動物詳細資訊
    /// </summary>
    /// <param name="routeId">路線的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>路線中的動物集合</returns>
    /// <example>
    /// <code>
    /// var animals = await routeService.GetRouteAnimalsAsync("route-highlights");
    /// foreach (var animal in animals)
    /// {
    ///     Console.WriteLine(animal.ChineseName);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Animal>> GetRouteAnimalsAsync(
        string routeId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得路線途經的區域詳細資訊
    /// </summary>
    /// <param name="routeId">路線的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>路線途經的區域集合</returns>
    /// <example>
    /// <code>
    /// var zones = await routeService.GetRouteZonesAsync("route-highlights");
    /// foreach (var zone in zones)
    /// {
    ///     Console.WriteLine(zone.NameZh);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Zone>> GetRouteZonesAsync(
        string routeId, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 自訂路線規劃結果
/// </summary>
public class CustomRouteResult
{
    /// <summary>
    /// 規劃是否成功
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// 錯誤訊息（若規劃失敗）
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 排序後的動物 ID 清單（依最佳路徑順序）
    /// </summary>
    public required IReadOnlyList<string> AnimalIds { get; init; }

    /// <summary>
    /// 途經的區域 ID 清單（依序）
    /// </summary>
    public required IReadOnlyList<string> ZoneIds { get; init; }

    /// <summary>
    /// 預估步行時間（分鐘）
    /// </summary>
    public required int EstimatedMinutes { get; init; }

    /// <summary>
    /// 可分享的路線編碼
    /// </summary>
    public string? ShareCode { get; init; }
}
